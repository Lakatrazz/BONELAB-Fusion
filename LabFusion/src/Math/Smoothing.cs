namespace LabFusion.Math;

public static class Smoothing
{
    public static float EaseInCubic(float t)
    {
        return t * t * t;
    }

    public static float EaseOutCubic(float t)
    {
        float oneMinusT = 1f - t;
        return 1f - (oneMinusT * oneMinusT * oneMinusT);
    }

    public static float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) * 0.5f;
    }

    public static float EaseOutElastic(float t)
    {
        if (t <= 0f || t >= 1f)
        {
            return t;
        }

        return MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * (2f * MathF.PI / 3f)) + 1f;
    }

    public static float CalculateDecay(float decay, float deltaTime)
    {
        return 1f - MathF.Exp(-decay * deltaTime);
    }
}
