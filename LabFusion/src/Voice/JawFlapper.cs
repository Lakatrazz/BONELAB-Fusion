using LabFusion.Math;

namespace LabFusion.Voice;

public class JawFlapper
{
    public const float MaxAngle = 40f;

    private float _voiceLoudness = 0f;

    public float GetAngle()
    {
        return _voiceLoudness * MaxAngle;
    }

    public void ClearJaw()
    {
        _voiceLoudness = 0f;
    }

    public void UpdateJaw(float amplitude, float deltaTime)
    {
        float target = ManagedMathf.Clamp01(amplitude * 2f);

        _voiceLoudness = ManagedMathf.Lerp(_voiceLoudness, target, Smoothing.CalculateDecay(12f, deltaTime));
    }
}
