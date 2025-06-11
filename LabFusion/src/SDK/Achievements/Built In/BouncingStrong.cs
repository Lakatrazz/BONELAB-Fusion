using LabFusion.Bonelab;
using LabFusion.Data;
using LabFusion.Marrow;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;

namespace LabFusion.SDK.Achievements;

public class BouncingStrong : Achievement
{
    public override string Title => "Bouncing Strong";

    public override string Description => "Jump as Strong 1000 times across servers.";

    public override int BitReward => 1000;

    public override int MaxTasks => 1000;

    protected override void OnRegister()
    {
        MultiplayerHooking.OnPlayerAction += OnPlayerAction;
    }

    protected override void OnUnregister()
    {
        MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
    }

    protected override void OnComplete()
    {
        LocalAudioPlayer.Play2dOneShot(new AudioReference(FusionMonoDiscReferences.FistfightFusionReference), LocalAudioPlayer.MusicSettings);
    }

    private void OnPlayerAction(PlayerID player, PlayerActionType type, PlayerID otherPlayer)
    {
        // Make sure there's other players
        if (!PlayerIDManager.HasOtherPlayers)
            return;

        // Make sure this is us, and that we jumped
        if (player.IsMe && type == PlayerActionType.JUMP)
        {
            // Check current avatar
            if (RigData.RigAvatarId == BonelabAvatarReferences.StrongBarcode)
            {
                IncrementTask();
            }
        }
    }
}
