using LabFusion.Utilities;

namespace LabFusion.Voice
{
    public class JawFlapper
    {
        private float _voiceLoudness = 0f;

        private const float _sinAmplitude = 5f;
        private const float _sinOmega = 10f;

        public float GetAngle()
        {
            return _voiceLoudness * 20f;
        }

        public void ClearJaw()
        {
            _voiceLoudness = 0f;
        }

        public void UpdateJaw(float amplitude)
        {
            // Update the amplitude
            float target = amplitude;

            // Add affectors
            target *= 1000f;
            target = ManagedMathf.Clamp(target, 0f, 2f);

            // Lerp towards the desired value
            float sin = Math.Abs(_sinAmplitude * ManagedMathf.Sin(_sinOmega * TimeUtilities.TimeSinceStartup));
            sin = ManagedMathf.Clamp01(sin);

            _voiceLoudness = ManagedMathf.LerpUnclamped(_voiceLoudness * sin, target, TimeUtilities.DeltaTime * 12f);
        }
    }
}
