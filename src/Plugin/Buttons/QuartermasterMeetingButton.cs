using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using Object = UnityEngine.Object;
using Polus.Plugin;
using Polus.Plugin.Roles;

namespace Polus.Plugin.Buttons;

public sealed class QuartermasterMeetingButton : CustomActionButton
{
    public override string Name => "Polus Meeting";
    public override float Cooldown => 0f;
    public override int MaxUses => 1;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.CrewmateFile;
    public override MiraKeybind? Keybind => MiraGlobalKeybinds.SecondaryAbility;

    public override bool Enabled(RoleBehaviour? role) =>
        role is QuartermasterRole &&
        PlayerControl.LocalPlayer != null &&
        PolusRoleEvents.GetCompletedTaskCount(PlayerControl.LocalPlayer.PlayerId) >= 4;

    protected override void OnClick()
    {
        var emergency = ShipStatus.Instance.EmergencyButton;
        PlayerControl.LocalPlayer.NetTransform.Halt();
        var minigame = Object.Instantiate(emergency.MinigamePrefab, Camera.main!.transform, false);
        minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
        minigame.Begin(null);
    }
}
