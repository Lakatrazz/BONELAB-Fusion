using LabFusion.Player;
using LabFusion.Marrow.Extenders;
using LabFusion.Marrow.Rig;

namespace LabFusion.SDK.Achievements;

public class GuardianAngel : Achievement
{
    public override string Title => "Guardian Angel";

    public override string Description => "Save another person from dying.";

    public override int BitReward => 500;

    protected override void OnRegister()
    {
        RigActionManager.PlayerRigActed += OnPlayerActed;
    }

    protected override void OnUnregister()
    {
        RigActionManager.PlayerRigActed -= OnPlayerActed;
    }

    private void OnPlayerActed(PlayerID playerID, RigActionType type)
    {
        // Checking if we saved someone else, so the player shouldn't be us
        if (playerID.IsMe)
        {
            return;
        }

        if (type != RigActionType.Recovery)
        {
            return;
        }

        // Check the most recently killed NPC
        // If we are the owner, we probably saved them
        if (PuppetMasterExtender.LastKilled != null && PuppetMasterExtender.LastKilled.IsOwner)
        {
            IncrementTask();
        }
    }
}
