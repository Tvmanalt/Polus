using MiraAPI.Roles;
using UnityEngine;

namespace Polus.Plugin.Roles;

public abstract class PolusCrewmateRole : CrewmateRole, ICustomRole
{
    public abstract string IdPart { get; }
    public string IdPrefix => "Polus.Role.Crewmate";
    public abstract string RoleName { get; }
    public abstract string RoleDescription { get; }
    public string RoleMedDescription => RoleDescription;
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor => new Color32(105, 190, 255, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this) { DefaultRoleCount = 1, DefaultChance = 35, MaxRoleCount = 1 };
    public bool CanLocalPlayerSeeRole(PlayerControl player) => true;
}

public abstract class PolusImpostorRole : ImpostorRole, ICustomRole
{
    public abstract string IdPart { get; }
    public string IdPrefix => "Polus.Role.Impostor";
    public abstract string RoleName { get; }
    public abstract string RoleDescription { get; }
    public string RoleMedDescription => RoleDescription;
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor => new Color32(235, 80, 80, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public CustomRoleConfiguration Configuration => new(this) { DefaultRoleCount = 1, DefaultChance = 35, MaxRoleCount = 1 };
    public bool CanLocalPlayerSeeRole(PlayerControl player) => true;
}

public abstract class PolusNeutralRole : CrewmateRole, ICustomRole
{
    public abstract string IdPart { get; }
    public string IdPrefix => "Polus.Role.Neutral";
    public abstract string RoleName { get; }
    public abstract string RoleDescription { get; }
    public abstract int ObjectiveTaskGoal { get; }
    public string RoleMedDescription => RoleDescription;
    public string RoleLongDescription => RoleDescription + $"\\nObjective progress: {(Player ? PolusRoleEvents.GetCompletedTaskCount(Player.PlayerId) : 0)}/{ObjectiveTaskGoal}";
    public Color RoleColor => new Color32(190, 145, 255, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public CustomRoleConfiguration Configuration => new(this)
    {
        DefaultRoleCount = 1,
        DefaultChance = 25,
        MaxRoleCount = 1,
        CanGetKilled = true,
        UseVanillaKillButton = false,
        CanUseVent = false,
        CanUseSabotage = false,
        TasksCountForProgress = false
    };
    public bool CanLocalPlayerSeeRole(PlayerControl player) => true;
    public override bool DidWin(GameOverReason gameOverReason) => gameOverReason == MiraAPI.GameEnd.CustomGameOver.GameOverReason<PolusNeutralVictory>();
}

public sealed class CartographerRole : PolusCrewmateRole
{
    public override string IdPart => "cartographer";
    public override string RoleName => "Cartographer";
    public override string RoleDescription => "Builds a route journal from completed console tasks. Each distinct task type adds one route segment.";
}

public sealed class AuditorRole : PolusCrewmateRole
{
    public override string IdPart => "auditor";
    public override string RoleName => "Auditor";
    public override string RoleDescription => "Receives one additional vote at every meeting through the meeting voting API.";
}

public sealed class RelayRole : PolusCrewmateRole
{
    public override string IdPart => "relay";
    public override string RoleName => "Relay";
    public override string RoleDescription => "Tracks completed task signals for its personal objective.";
}

public sealed class QuartermasterRole : PolusCrewmateRole
{
    public override string IdPart => "quartermaster";
    public override string RoleName => "Quartermaster";
    public override string RoleDescription => "Tracks completed utility tasks and earns a one-use meeting call after its task objective.";
}

public sealed class SurveyorRole : PolusCrewmateRole
{
    public override string IdPart => "surveyor";
    public override string RoleName => "Surveyor";
    public override string RoleDescription => "Records completed tasks by task type to build an environmental survey.";
}

public sealed class CourierRole : PolusCrewmateRole
{
    public override string IdPart => "courier";
    public override string RoleName => "Courier";
    public override string RoleDescription => "Tracks task completions as deliveries for its personal objective.";
}

public sealed class ArchivistRole : PolusCrewmateRole
{
    public override string IdPart => "archivist";
    public override string RoleName => "Archivist";
    public override string RoleDescription => "Records task and meeting events in order for its personal event ledger.";
}

public sealed class BeaconistRole : PolusCrewmateRole
{
    public override string IdPart => "beaconist";
    public override string RoleName => "Beaconist";
    public override string RoleDescription => "Tracks completed tasks as beacon readings.";
}

public sealed class MediatorRole : PolusCrewmateRole
{
    public override string IdPart => "mediator";
    public override string RoleName => "Mediator";
    public override string RoleDescription => "Earns one additional vote at the first meeting after completing three tasks.";
}

public sealed class CalibratorRole : PolusCrewmateRole
{
    public override string IdPart => "calibrator";
    public override string RoleName => "Calibrator";
    public override string RoleDescription => "Tracks completed console tasks as calibration points.";
}

public sealed class SignalwrightRole : PolusCrewmateRole
{
    public override string IdPart => "signalwright";
    public override string RoleName => "Signalwright";
    public override string RoleDescription => "Records task completions as successful signal links.";
}

public sealed class TimekeeperRole : PolusCrewmateRole
{
    public override string IdPart => "timekeeper";
    public override string RoleName => "Timekeeper";
    public override string RoleDescription => "Tracks task progress checkpoints for later comparisons.";
}

public sealed class CounterfeiterRole : PolusImpostorRole
{
    public override string IdPart => "counterfeiter";
    public override string RoleName => "Counterfeiter";
    public override string RoleDescription => "Uses the standard Impostor role toolkit and has its own Polus role identity.";
}

public sealed class EchoistRole : PolusImpostorRole
{
    public override string IdPart => "echoist";
    public override string RoleName => "Echoist";
    public override string RoleDescription => "Uses the standard Impostor role toolkit and has its own Polus role identity.";
}

public sealed class BlackoutArtistRole : PolusImpostorRole
{
    public override string IdPart => "blackout_artist";
    public override string RoleName => "Blackout Artist";
    public override string RoleDescription => "Uses the standard Impostor role toolkit and has its own Polus role identity.";
}

public sealed class BrokerRole : PolusImpostorRole
{
    public override string IdPart => "broker";
    public override string RoleName => "Broker";
    public override string RoleDescription => "Uses the standard Impostor role toolkit and has its own Polus role identity.";
}

public sealed class DecoyistRole : PolusImpostorRole
{
    public override string IdPart => "decoyist";
    public override string RoleName => "Decoyist";
    public override string RoleDescription => "Uses the standard Impostor role toolkit and has its own Polus role identity.";
}

public sealed class LedgeristRole : PolusImpostorRole
{
    public override string IdPart => "ledgerist";
    public override string RoleName => "Ledgerist";
    public override string RoleDescription => "Uses the standard Impostor role toolkit and has its own Polus role identity.";
}

public sealed class SwitchboarderRole : PolusImpostorRole
{
    public override string IdPart => "switchboarder";
    public override string RoleName => "Switchboarder";
    public override string RoleDescription => "Uses the standard Impostor role toolkit and has its own Polus role identity.";
}

public sealed class WeathermakerRole : PolusImpostorRole
{
    public override string IdPart => "weathermaker";
    public override string RoleName => "Weathermaker";
    public override string RoleDescription => "Uses the standard Impostor role toolkit and has its own Polus role identity.";
}

public sealed class MisdirectorRole : PolusImpostorRole
{
    public override string IdPart => "misdirector";
    public override string RoleName => "Misdirector";
    public override string RoleDescription => "Uses the standard Impostor role toolkit and has its own Polus role identity.";
}

public sealed class ScramblerRole : PolusImpostorRole
{
    public override string IdPart => "scrambler";
    public override string RoleName => "Scrambler";
    public override string RoleDescription => "Uses the standard Impostor role toolkit and has its own Polus role identity.";
}

public sealed class ParasiticSignalRole : PolusImpostorRole
{
    public override string IdPart => "parasitic_signal";
    public override string RoleName => "Parasitic Signal";
    public override string RoleDescription => "Uses the standard Impostor role toolkit and has its own Polus role identity.";
}

public sealed class JigsawRole : PolusNeutralRole
{
    public override int ObjectiveTaskGoal => 4;
    public override string IdPart => "jigsaw";
    public override string RoleName => "Jigsaw";
    public override string RoleDescription => "Has a distinct neutral identity and a personal task-completion objective.";
}

public sealed class PilgrimRole : PolusNeutralRole
{
    public override int ObjectiveTaskGoal => 5;
    public override string IdPart => "pilgrim";
    public override string RoleName => "Pilgrim";
    public override string RoleDescription => "Has a distinct neutral identity and a longer personal task-completion objective.";
}

public sealed class CuratorRole : PolusNeutralRole
{
    public override int ObjectiveTaskGoal => 4;
    public override string IdPart => "curator";
    public override string RoleName => "Curator";
    public override string RoleDescription => "Has a distinct neutral identity and a personal collection objective.";
}

public sealed class WayfinderRole : PolusNeutralRole
{
    public override int ObjectiveTaskGoal => 6;
    public override string IdPart => "wayfinder";
    public override string RoleName => "Wayfinder";
    public override string RoleDescription => "Has a distinct neutral identity and a personal route objective.";
}

public sealed class CollectorRole : PolusNeutralRole
{
    public override int ObjectiveTaskGoal => 5;
    public override string IdPart => "collector";
    public override string RoleName => "Collector";
    public override string RoleDescription => "Has a distinct neutral identity and a personal collection objective.";
}

public sealed class MetronomeRole : PolusNeutralRole
{
    public override int ObjectiveTaskGoal => 6;
    public override string IdPart => "metronome";
    public override string RoleName => "Metronome";
    public override string RoleDescription => "Has a distinct neutral identity and a personal sequence objective.";
}

public sealed class CartoucheRole : PolusNeutralRole
{
    public override int ObjectiveTaskGoal => 4;
    public override string IdPart => "cartouche";
    public override string RoleName => "Cartouche";
    public override string RoleDescription => "Has a distinct neutral identity and a personal symbol-pair objective.";
}

public sealed class LighthouseRole : PolusNeutralRole
{
    public override int ObjectiveTaskGoal => 7;
    public override string IdPart => "lighthouse";
    public override string RoleName => "Lighthouse";
    public override string RoleDescription => "Has a distinct neutral identity and a personal signal-chain objective.";
}

public sealed class WeathervaneRole : PolusNeutralRole
{
    public override int ObjectiveTaskGoal => 5;
    public override string IdPart => "weathervane";
    public override string RoleName => "Weathervane";
    public override string RoleDescription => "Has a distinct neutral identity and a personal prediction objective.";
}

public sealed class PuzzlewrightRole : PolusNeutralRole
{
    public override int ObjectiveTaskGoal => 8;
    public override string IdPart => "puzzlewright";
    public override string RoleName => "Puzzlewright";
    public override string RoleDescription => "Has a distinct neutral identity and a personal logic objective.";
}

public sealed class TimeCapsuleRole : PolusNeutralRole
{
    public override int ObjectiveTaskGoal => 6;
    public override string IdPart => "time_capsule";
    public override string RoleName => "Time Capsule";
    public override string RoleDescription => "Has a distinct neutral identity and a personal before-and-after objective.";
}

public sealed class NomadRole : PolusNeutralRole
{
    public override int ObjectiveTaskGoal => 7;
    public override string IdPart => "nomad";
    public override string RoleName => "Nomad";
    public override string RoleDescription => "Has a distinct neutral identity and a personal travel objective.";
}

