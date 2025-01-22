namespace LabFusion.Math;

public static class Smoothing
{
    public static float CalculateDecay(float decay, float deltaTime)
    {
        return 1f - MathF.Exp(-decay * deltaTime);
    }
}
