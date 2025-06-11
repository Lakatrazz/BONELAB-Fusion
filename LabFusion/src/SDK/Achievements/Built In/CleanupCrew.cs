namespace LabFusion.SDK.Achievements;

public class CleanupCrew : Achievement
{
    public override string Title => "Cleanup Crew";

    public override string Description => "Despawn 1000 things across servers.";

    public override int BitReward => 1000;

    public override int MaxTasks => 1000;
}
