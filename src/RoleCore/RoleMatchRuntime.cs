using System.Security.Cryptography;

namespace Polus.RoleCore;

public sealed record PlayerSlot(byte PlayerId, bool IsAlive = true);

public sealed record RoleAssignment(byte PlayerId, RoleDefinition Role);

public sealed record RoleActionResult(bool Accepted, string Code, string Message);

public sealed record ObjectiveProgress(byte PlayerId, string RoleId, int Current, int Required, bool Completed);

/// <summary>
/// Game-independent, host-authoritative match state.
/// The Among Us/BepInEx adapter must call these methods from validated game events.
/// This class deliberately does not pretend to provide game hooks or networking.
/// </summary>
public sealed class RoleMatchRuntime
{
    private readonly object gate = new();
    private readonly Dictionary<byte, RoleDefinition> assignments = new();
    private readonly Dictionary<byte, DateTimeOffset> cooldownEnds = new();
    private readonly Dictionary<byte, int> objectiveProgress = new();
    private readonly Dictionary<byte, HashSet<string>> completedObjectives = new();
    private readonly Dictionary<byte, DateTimeOffset> lastActionAt = new();

    public bool IsHost { get; }
    public bool IsRunning { get; private set; }
    public IReadOnlyDictionary<byte, RoleDefinition> Assignments
    {
        get { lock (gate) return new Dictionary<byte, RoleDefinition>(assignments); }
    }

    public RoleMatchRuntime(bool isHost) => IsHost = isHost;

    public IReadOnlyList<RoleAssignment> StartMatch(
        IReadOnlyList<PlayerSlot> players,
        IReadOnlyList<RoleDefinition> catalog,
        HostSettings settings,
        int impostorCount,
        int neutralCount,
        int? seed = null)
    {
        EnsureHost();
        lock (gate)
        {
            if (IsRunning) throw new InvalidOperationException("A match is already running.");
            if (players.Count < 4) throw new ArgumentException("At least four players are required.");
            if (players.Select(p => p.PlayerId).Distinct().Count() != players.Count)
                throw new ArgumentException("Player IDs must be unique.");
            if (impostorCount < 1 || neutralCount < 0 || impostorCount + neutralCount >= players.Count)
                throw new ArgumentOutOfRangeException(nameof(impostorCount));

            var validated = HostSettingsValidator.Validate(settings);
            var random = seed.HasValue ? new Random(seed.Value) : new Random(RandomNumberGenerator.GetInt32(int.MaxValue));
            var available = catalog
                .Where(r => validated.EnabledRoles.TryGetValue(r.Id, out var enabled) && enabled)
                .Where(r => r.SpawnChance > 0)
                .GroupBy(r => r.Id, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            var crew = available.Where(r => r.Team == RoleTeam.Crewmate).ToList();
            var impostors = available.Where(r => r.Team == RoleTeam.Impostor).ToList();
            var neutrals = validated.AllowNeutralRoles
                ? available.Where(r => r.Team == RoleTeam.Neutral).ToList()
                : new List<RoleDefinition>();

            if (impostors.Count < impostorCount)
                throw new InvalidOperationException("Not enough enabled Impostor roles for the requested count.");
            if (neutrals.Count < neutralCount)
                throw new InvalidOperationException("Not enough enabled Neutral roles for the requested count.");

            var shuffledPlayers = players.Where(p => p.IsAlive).OrderBy(_ => random.Next()).ToList();
            if (shuffledPlayers.Count != players.Count)
                throw new InvalidOperationException("Dead players cannot be included in initial role assignment.");

            var chosen = new List<(PlayerSlot Player, RoleDefinition Role)>();
            AddTeam(shuffledPlayers, impostorCount, impostors, chosen, random);
            AddTeam(shuffledPlayers, neutralCount, neutrals, chosen, random);
            AddTeam(shuffledPlayers, Math.Min(validated.MaxRolesPerGame - chosen.Count, shuffledPlayers.Count - chosen.Count),
                crew, chosen, random);

            assignments.Clear();
            cooldownEnds.Clear();
            objectiveProgress.Clear();
            completedObjectives.Clear();
            lastActionAt.Clear();

            foreach (var item in chosen)
            {
                assignments[item.Player.PlayerId] = item.Role;
                objectiveProgress[item.Player.PlayerId] = 0;
                completedObjectives[item.Player.PlayerId] = new HashSet<string>(StringComparer.Ordinal);
            }

            IsRunning = true;
            return chosen.Select(x => new RoleAssignment(x.Player.PlayerId, x.Role)).ToArray();
        }
    }

    private static void AddTeam(
        List<PlayerSlot> players,
        int count,
        List<RoleDefinition> pool,
        List<(PlayerSlot Player, RoleDefinition Role)> result,
        Random random)
    {
        if (count <= 0) return;
        var candidates = pool.OrderBy(_ => random.Next()).Take(count).ToArray();
        if (candidates.Length < count) throw new InvalidOperationException("Role pool is too small.");
        var alreadyUsed = result.Select(x => x.Role.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (candidates.Any(r => alreadyUsed.Contains(r.Id)))
            throw new InvalidOperationException("A role cannot be assigned more than once in a match.");
        var remaining = players.Where(p => result.All(x => x.Player.PlayerId != p.PlayerId)).ToList();
        for (var i = 0; i < count; i++) result.Add((remaining[i], candidates[i]));
    }

    public RoleActionResult TryUseAbility(byte playerId, DateTimeOffset now)
    {
        EnsureHost();
        lock (gate)
        {
            if (!IsRunning) return Reject("match_not_running", "The match is not running.");
            if (!assignments.TryGetValue(playerId, out var role))
                return Reject("no_role", "This player has no Polus role.");
            if (cooldownEnds.TryGetValue(playerId, out var until) && now < until)
                return Reject("cooldown", $"Ability is ready at {until:O}.");
            if (lastActionAt.TryGetValue(playerId, out var previous) && now < previous)
                return Reject("invalid_time", "Action timestamps must be monotonic.");

            cooldownEnds[playerId] = now.AddSeconds(Math.Clamp(role.CooldownSeconds, 0, 600));
            lastActionAt[playerId] = now;
            return new RoleActionResult(true, "accepted", "Ability request accepted by the role runtime.");
        }
    }

    /// <summary>Progress is credited only after the host adapter validates the corresponding in-game event.</summary>
    public ObjectiveProgress RecordObjective(byte playerId, string objectiveId, int amount, int required)
    {
        EnsureHost();
        if (string.IsNullOrWhiteSpace(objectiveId)) throw new ArgumentException("Objective ID is required.");
        if (amount <= 0 || required <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        lock (gate)
        {
            if (!IsRunning) throw new InvalidOperationException("No active match.");
            if (!assignments.TryGetValue(playerId, out var role))
                throw new InvalidOperationException("Player has no assigned role.");
            if (!completedObjectives.TryGetValue(playerId, out var completed))
                throw new InvalidOperationException("Player objective state is missing.");
            if (completed.Contains(objectiveId))
                return new ObjectiveProgress(playerId, role.Id, objectiveProgress[playerId], required, true);

            objectiveProgress[playerId] = Math.Min(required, checked(objectiveProgress[playerId] + amount));
            var done = objectiveProgress[playerId] >= required;
            if (done) completed.Add(objectiveId);
            return new ObjectiveProgress(playerId, role.Id, objectiveProgress[playerId], required, done);
        }
    }

    public void EndMatch()
    {
        EnsureHost();
        lock (gate)
        {
            IsRunning = false;
            assignments.Clear();
            cooldownEnds.Clear();
            objectiveProgress.Clear();
            completedObjectives.Clear();
            lastActionAt.Clear();
        }
    }

    private void EnsureHost()
    {
        if (!IsHost) throw new InvalidOperationException("Only the authoritative host may mutate match role state.");
    }

    private static RoleActionResult Reject(string code, string message) => new(false, code, message);
}
