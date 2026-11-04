using System.Collections.Concurrent;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using Polus.Plugin.Roles;

namespace Polus.Plugin;

public static class PolusRoleEvents
{
    private static readonly ConcurrentDictionary<byte, int> CompletedTasks = new();

    [RegisterEvent]
    public static void OnTaskCompleted(CompleteTaskEvent ev)
    {
        if (ev.Player == null || ev.Player.Data == null) return;
        CompletedTasks.AddOrUpdate(ev.Player.PlayerId, 1, (_, count) => count + 1);
    }

    [RegisterEvent]
    public static void OnMeetingStarted(StartMeetingEvent ev)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
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
}
