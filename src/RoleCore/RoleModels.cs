namespace Polus.RoleCore;

public enum RoleTeam { Crewmate, Impostor, Neutral }

public sealed record RoleDefinition(
    string Id,
    string Name,
    RoleTeam Team,
    string Description,
    bool EnabledByDefault = false,
    int SpawnChance = 50,
    int CooldownSeconds = 30);

public sealed record HostSettings(
    bool HostOnlyMode,
    int MaxRolesPerGame,
    bool AllowNeutralRoles,
    IReadOnlyDictionary<string, bool> EnabledRoles);

public static class HostSettingsValidator
{
    public static HostSettings Validate(HostSettings settings)
    {
        if (settings.MaxRolesPerGame is < 1 or > 15)
            throw new ArgumentOutOfRangeException(nameof(settings.MaxRolesPerGame));
        if (settings.EnabledRoles.Count > 128)
            throw new ArgumentException("Too many role settings.", nameof(settings));
        return settings with
        {
            HostOnlyMode = true,
            EnabledRoles = new Dictionary<string, bool>(settings.EnabledRoles, StringComparer.OrdinalIgnoreCase)
        };
    }
}
