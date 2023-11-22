using UnityEngine;

namespace LabFusion.Utilities
{
    internal static class TimeUtilities
    {
        public static float DeltaTime => _deltaTime;

        public static float FixedDeltaTime => _fixedDeltaTime;

        public static float TimeSinceStartup => _timeSinceStartup;

        public static float TimeScale => _timeScale;

        public static int FrameCount => _frameCount;

        private static float _deltaTime = 1f;
        private static float _fixedDeltaTime = 0.02f;
        private static float _timeSinceStartup;

        private static float _timeScale = 1f;

        private static int _frameCount;

        internal static void OnEarlyUpdate()
        {
            _timeScale = Time.timeScale;

            _deltaTime = Time.deltaTime;
            _timeSinceStartup += _deltaTime;

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
    }
}
