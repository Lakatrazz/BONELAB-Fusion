using LabFusion.SDK.Achievements;

namespace LabFusion.Bonelab.Achievements;

public class OneMoreTime : Achievement
{
    public override string Title => "One More Time";

    public override string Description => "Enter Jay's taxi while in multiplayer.";

    public override int BitReward => 200;
}
