using BepInEx;
using BepInEx.Unity.IL2CPP;
using Polus.RoleCore;

namespace Polus.Plugin;

[BepInPlugin("org.polus.customroles", "Polus Custom Roles", "1.0.0")]
public sealed class PolusPlugin : BasePlugin
{
    private IReadOnlyList<RoleDefinition> catalog = Array.Empty<RoleDefinition>();

    public override void Load()
    {
        var catalogPath = Path.Combine(Paths.BepInExRootPath, "plugins", "Polus", "roles.json");
        try
        {
            catalog = RoleCatalogLoader.LoadFile(catalogPath);
            Log.LogInfo($"Polus plugin loaded. Validated {catalog.Count} role definitions.");
        }
        catch (Exception ex)
        {
            Log.LogError($"Could not load the Polus role catalogue: {ex}");
        }

        Log.LogWarning("Polus gameplay hooks are not enabled in this build yet. The loader and catalogue are installed, but no in-match role abilities are being advertised as playable.");
    }
}
