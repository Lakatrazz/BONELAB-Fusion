using LabFusion.Utilities;

namespace LabFusion.Safety;

/// <summary>
/// Tracks activity within a certain amount of time tied to an ID.
/// </summary>
public class LimitedActivityTracker
{
    public class Activity
    {
        private int _counter = 0;
        public int Counter
        {
            get
            {
                if (PastActivityPeriod)
                {
                    return 0;
                }

                return _counter;
            }
        }

        public float LastActivityTime { get; private set; } = 0f;

        public float TimeSinceActivity => TimeUtilities.TimeSinceStartup - LastActivityTime;

        public float ActivityPeriod { get; set; } = 1f;

        public bool PastActivityPeriod => TimeSinceActivity >= ActivityPeriod;

        public void Increment()
        {
            if (PastActivityPeriod)
            {
                _counter = 0;
                LastActivityTime = TimeUtilities.TimeSinceStartup;
            }

            _counter++;
        }

        public void Reset()
        {
            LastActivityTime = 0f;
            _counter = 0;
        }
    }

    public Dictionary<int, Activity> IDToActivityLookup { get; } = new();

    private float _activityPeriod = 1f;
    public float ActivityPeriod
    {
        get
        {
            return _activityPeriod;
        }
        set
        {
            if (_activityPeriod == value)
            {
                return;
            }

            _activityPeriod = value;

            foreach (var activity in IDToActivityLookup.Values)
            {
                activity.ActivityPeriod = value;
            }
        }
    }

    public LimitedActivityTracker() : this(1f) { }

    public LimitedActivityTracker(float activityPeriod)
    {
        _activityPeriod = activityPeriod;
    }

    public Activity GetActivity(int id)
    {
        if (IDToActivityLookup.TryGetValue(id, out var result))
        {
            return result;
        }

        var newActivity = new Activity() { ActivityPeriod = _activityPeriod };
        IDToActivityLookup[id] = newActivity;

        return newActivity;
    }

    public void Increment(int id)
    {
        var activity = GetActivity(id);

        activity.Increment();
    }

    public void Clear()
    {
        IDToActivityLookup.Clear();
    }
}
