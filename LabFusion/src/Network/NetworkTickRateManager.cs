namespace LabFusion.Network;

public static class NetworkTickRateManager
{
    public static readonly float TickRate = 20f;
    
    public static readonly float SecondsBetweenTicks = 1f / TickRate;

    private static float _tickElapsed = 0f;

    public static bool IsTickThisFrame { get; private set; } = false;

    internal static void OnUpdate(float deltaTime)
    {
        IsTickThisFrame = false;

        _tickElapsed += deltaTime;

        if (_tickElapsed >= SecondsBetweenTicks)
        {
            _tickElapsed -= SecondsBetweenTicks;
            IsTickThisFrame = true;
        }
    }
}
