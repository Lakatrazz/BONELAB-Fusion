namespace LabFusion.Utilities;

public static class DelayUtilities
{
    private class FrameDelayInfo
    {
        public Action Action;

        public int Counter;
    }

    private static readonly List<FrameDelayInfo> _delays = new();

    /// <summary>
    /// Invokes an action after a certain amount of frames.
    /// </summary>
    /// <param name="action">The action to invoke.</param>
    /// <param name="frames">The amount of frames to wait.</param>
    public static void InvokeDelayed(Action action, int frames)
    {
        _delays.Add(new FrameDelayInfo()
        {
            Action = action,
            Counter = frames,
        });
    }

    /// <summary>
    /// Invokes an action in the next frame.
    /// </summary>
    /// <param name="action">The action to invoke.</param>
    public static void InvokeNextFrame(Action action) => InvokeDelayed(action, 1);

    internal static void OnProcessDelays()
    {
        // We go backwards so we can remove items without disrupting the list
        int count = _delays.Count;

        for (var i = count - 1; i >= 0; i--)
        {
            // Check the counter, if we've reached 0 then invoke the event
            var delay = _delays[i];

            if (delay.Counter <= 0)
            {
                try
                {
                    delay.Action();
                }
                catch (Exception e)
                {
                    FusionLogger.Error(e.ToString());
                }

                _delays.RemoveAt(i);
                continue;
            }

            // Otherwise, decrement it and wait for the next frame
            delay.Counter--;
        }
    }
}