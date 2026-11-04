using System.Collections.Concurrent;
using System.Linq;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameEnd;
using MiraAPI.Utilities;
using Polus.Plugin.Roles;

namespace Polus.Plugin;

public static class PolusRoleEvents
{
    private static readonly ConcurrentDictionary<byte, int> CompletedTasks = new();
    private static readonly ConcurrentDictionary<byte, byte> NeutralWinsTriggered = new();

    [RegisterEvent]
    public static void OnTaskCompleted(CompleteTaskEvent ev)
    {
        if (ev.Player == null || ev.Player.Data == null) return;
        var count = CompletedTasks.AddOrUpdate(ev.Player.PlayerId, 1, (_, current) => current + 1);
        if (ev.Player.AmOwner &&
            ev.Player.Data.Role is PolusNeutralRole neutral &&
            count >= neutral.ObjectiveTaskGoal &&
            NeutralWinsTriggered.TryAdd(ev.Player.PlayerId, 0))
        {
            CustomGameOver.Trigger<PolusNeutralVictory>([ev.Player.Data]);
        }
    }

    [RegisterEvent]
    public static void OnMeetingStarted(StartMeetingEvent ev)
    {
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player == null || player.Data == null) continue;

            if (player.Data.Role is AuditorRole)
            {
                player.GetVoteData().IncreaseRemainingVotes(1);
            }
            else if (player.Data.Role is MediatorRole &&
                     CompletedTasks.TryGetValue(player.PlayerId, out var completed) &&
                     completed >= 3)
            {
                player.GetVoteData().IncreaseRemainingVotes(1);
                CompletedTasks[player.PlayerId] = 0;
            }
        }
    }

    public static int GetCompletedTaskCount(byte playerId) =>
        CompletedTasks.TryGetValue(playerId, out var count) ? count : 0;
}
