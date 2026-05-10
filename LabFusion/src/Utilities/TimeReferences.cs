using UnityEngine;

namespace LabFusion.Utilities;

/// <summary>
/// References to <see cref="UnityEngine.Time"/> properties, but cached to avoid IL2CPP performance losses.
/// </summary>
public static class TimeReferences
{
    public static float DeltaTime { get; private set; } = 1f;

    public static float UnscaledDeltaTime { get; private set; } = 1f;

    public static float FixedDeltaTime { get; private set; } = 0.02f;

    public static float FixedUnscaledDeltaTime { get; private set; } = 0.02f;

    public static float TimeSinceStartup { get; private set; } = 0f;

    public static float TimeScale { get; private set; } = 1f;

    public static float SafeTimeScale => MathF.Max(0.005f, TimeScale);

    public static int FrameCount { get; private set; } = 0;

    public static void OnEarlyUpdate()
    {
        TimeScale = Time.timeScale;

        DeltaTime = Time.deltaTime;
        UnscaledDeltaTime = Time.unscaledDeltaTime;

        TimeSinceStartup += DeltaTime;

        FrameCount++;
    }

    public static void OnEarlyFixedUpdate()
    {
        FixedDeltaTime = Time.fixedDeltaTime;
        FixedUnscaledDeltaTime = Time.fixedUnscaledDeltaTime;
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