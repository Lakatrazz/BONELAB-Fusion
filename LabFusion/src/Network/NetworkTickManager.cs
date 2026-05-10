using LabFusion.Math;

namespace LabFusion.Network;

public static class NetworkTickManager
{
    public static readonly float TickRate = 20f;
    
    public static readonly float SecondsBetweenTicks = 1f / TickRate;

    public static readonly float SmoothInterpolationDecayRate = 36f;

    private static float _tickElapsed = 0f;

    /// <summary>
    /// Whether server updates, such as positions, should be sent this frame.
    /// </summary>
    public static bool IsTickThisFrame { get; private set; } = false;

    /// <summary>
    /// The amount of time that it should take for basic interpolation to move from the old to the new value.
    /// </summary>
    public static readonly float LinearInterpolationLength = 0.1f;

    /// <summary>
    /// The percent, or t value, that interpolated values should move this frame.
    /// </summary>
    public static float SmoothInterpolationTime { get; private set; } = 0f;

    /// <summary>
    /// The maximum amount of time, in seconds, that a value will predict for.
    /// </summary>
    public static readonly float MaxPredictionTime = 0.1f;

    internal static void OnUpdate(float deltaTime)
    {
        IsTickThisFrame = false;

        SmoothInterpolationTime = Smoothing.CalculateDecay(SmoothInterpolationDecayRate, deltaTime);

        _tickElapsed += deltaTime;

        if (_tickElapsed >= SecondsBetweenTicks)
        {
            _tickElapsed -= SecondsBetweenTicks;
            IsTickThisFrame = true;
        }
    }
}
