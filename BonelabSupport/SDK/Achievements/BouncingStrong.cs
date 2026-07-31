using LabFusion.SDK.Achievements;
using LabFusion.Marrow;
using LabFusion.Player;
using LabFusion.Marrow.Rig;

namespace MarrowFusion.Bonelab.Achievements;

public class BouncingStrong : Achievement
{
    public override string Title => "Bouncing Strong";

    public override string Description => "Jump as Strong 1000 times across servers.";

    public override int BitReward => 1000;

    public override int MaxTasks => 1000;

    protected override void OnRegister()
    {
        RigActionManager.PlayerRigActed += OnPlayerActed;
    }

    protected override void OnUnregister()
    {
        RigActionManager.PlayerRigActed -= OnPlayerActed;
    }

    protected override void OnComplete()
    {
        LocalAudioPlayer.Play2dOneShot(new AudioReference(FusionMonoDiscReferences.FistfightFusionReference), LocalAudioPlayer.MusicSettings);
    }

    private void OnPlayerActed(PlayerID playerID, RigActionType type)
    {
        if (!PlayerIDManager.HasOtherPlayers)
        {
            return;
        }

        if (!playerID.IsMe)
        {
            return;
        }

        if (type != RigActionType.Jump)
        {
            return;
        }

        if (LocalAvatar.AvatarBarcode == BonelabAvatarReferences.StrongReference.Barcode.ID)
        {
            IncrementTask();
        }
    }
}
