using System.Text.Json;
using System.Text.Json.Serialization;

namespace Polus.RoleCore;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoleTeam { Crewmate, Impostor, Neutral }

public sealed record RoleDefinition(
    string Id,
    string Name,
    RoleTeam Team,
    string Description,
    bool EnabledByDefault = false,
    int SpawnChance = 50,
    int CooldownSeconds = 30,
    string Mechanic = "",
    string WinCondition = "",
    string Status = "design-only");

public sealed record HostSettings(
    bool HostOnlyMode,
    int MaxRolesPerGame,
    bool AllowNeutralRoles,
    IReadOnlyDictionary<string, bool> EnabledRoles);

public static class HostSettingsValidator
{
    public static HostSettings Validate(HostSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (settings.MaxRolesPerGame is < 1 or > 15)
            throw new ArgumentOutOfRangeException(nameof(settings.MaxRolesPerGame));
        if (settings.EnabledRoles is null)
            throw new ArgumentException("EnabledRoles cannot be null.", nameof(settings));
        if (settings.EnabledRoles.Count > 128)
            throw new ArgumentException("Too many role settings.", nameof(settings));
        if (settings.EnabledRoles.Keys.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Role setting IDs cannot be empty.", nameof(settings));

        return settings with
        {
            HostOnlyMode = true,
            EnabledRoles = new Dictionary<string, bool>(settings.EnabledRoles, StringComparer.OrdinalIgnoreCase)
        };
    }
}

public static class RoleCatalogLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static IReadOnlyList<RoleDefinition> Load(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("JSON cannot be empty.", nameof(json));
        var roles = JsonSerializer.Deserialize<List<RoleDefinition>>(json, Options)
            ?? throw new InvalidDataException("The role catalogue is empty.");

        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var role in roles)
        {
            if (string.IsNullOrWhiteSpace(role.Id) || !ids.Add(role.Id))
                throw new InvalidDataException($"Role ID is missing or duplicated: '{role.Id}'.");
            if (string.IsNullOrWhiteSpace(role.Name))
                throw new InvalidDataException($"Role '{role.Id}' has no name.");
            if (role.SpawnChance is < 0 or > 100)
                throw new InvalidDataException($"Role '{role.Id}' has an invalid spawn chance.");
            if (role.CooldownSeconds is < 0 or > 600)
                throw new InvalidDataException($"Role '{role.Id}' has an invalid cooldown.");
            if (role.Team == RoleTeam.Neutral && string.IsNullOrWhiteSpace(role.WinCondition))
                throw new InvalidDataException($"Neutral role '{role.Id}' must define its own win condition.");
        }

        return roles.AsReadOnly();
    }

    public static IReadOnlyList<RoleDefinition> LoadFile(string path) =>
        Load(File.ReadAllText(path));
}
