namespace LabFusion.SDK.Achievements;

public class StyleAddict : StyleAchievement
{
    public override string Title => "Style Addict";

    public override string Description => "Purchase your twentieth cosmetic.";

    public override int BitReward => 5000;

    public override int MaxTasks => 20;
}
