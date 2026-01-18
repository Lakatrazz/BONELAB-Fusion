using UnityEngine;

namespace LabFusion.Utilities;

internal static class TimeUtilities
{
    public static float DeltaTime { get { return _deltaTime; } }

    public static float UnscaledDeltaTime { get { return _unscaledDeltaTime; } }

    public static float FixedDeltaTime { get { return _fixedDeltaTime; } }
    
    public static float TimeSinceStartup { get { return _timeSinceStartup; } }

    public static float TimeScale { get { return _timeScale; } }

    public static int FrameCount { get { return _frameCount; } }

    private static float _deltaTime = 1f;
    private static float _unscaledDeltaTime = 1f;
    private static float _fixedDeltaTime = 0.02f;
    private static float _timeSinceStartup = 0f;

    private static float _timeScale = 1f;

    private static int _frameCount = 0;

    internal static void OnEarlyUpdate()
    {
        _timeScale = Time.timeScale;

        _deltaTime = Time.deltaTime;
        _timeSinceStartup += _deltaTime;
        
        _unscaledDeltaTime = Time.unscaledDeltaTime;

        _frameCount++;
    }

    internal static void OnEarlyFixedUpdate()
    {
        _fixedDeltaTime = Time.fixedDeltaTime;
    }

    public static bool IsMatchingFrame(int interval)
    {
        return FrameCount % interval == 0;
    }

    public static bool IsMatchingFrame(int interval, int offset)
    {
        return (FrameCount + offset) % interval == 0;
    }
}