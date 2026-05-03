using LabFusion.Math;

namespace LabFusion.Network;

public static class NetworkTickManager
{
    public static readonly float TickRate = 20f;
    
    public static readonly float SecondsBetweenTicks = 1f / TickRate;

    public static readonly float LinearInterpolationLength = 0.05f;

    public static readonly float SmoothInterpolationDecayRate = 36f;

    private static float _tickElapsed = 0f;

    /// <summary>
    /// Whether server updates, such as positions, should be sent this frame.
    /// </summary>
    public static bool IsTickThisFrame { get; private set; } = false;

    /// <summary>
    /// The amount, or t value, that interpolated values should move this frame.
    /// </summary>
    public static float SmoothInterpolationTime { get; private set; } = 0f;

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
