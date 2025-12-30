using LabFusion.SDK.Achievements;

namespace LabFusion.Bonelab.Achievements;

public class Betrayal : Achievement
{
    public override string Title => "Betrayal";

    public override string Description => "Save the hung peasant in Descent.";

    public override int BitReward => 300;
}
