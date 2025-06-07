namespace LabFusion.Safety;

public static class LimitedActivityManager
{
    public static Dictionary<string, LimitedActivityTracker> IDToTrackerLookup { get; } = new();

    public static LimitedActivityTracker GetTracker(string id, float activityPeriod = 1f)
    {
        if (IDToTrackerLookup.TryGetValue(id, out var result))
        {
            result.ActivityPeriod = activityPeriod;
            return result;
        }

        var newTracker = new LimitedActivityTracker(activityPeriod);
        IDToTrackerLookup[id] = newTracker;

        return newTracker;
    }
}
