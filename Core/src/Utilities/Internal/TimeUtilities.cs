using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using static Ara.AraTrail;

namespace LabFusion.Utilities
{
    internal static class TimeUtilities
    {
        public static float DeltaTime { get { return _deltaTime; } }

        public static float FixedDeltaTime { get { return _fixedDeltaTime; } }

        public static float TimeSinceStartup { get { return _timeSinceStartup; } }

        public static float TimeScale { get { return _timeScale; } }

        public static int FrameCount { get { return _frameCount; } }

        private static float _deltaTime = 1f;
        private static float _fixedDeltaTime = 0.02f;
        private static float _timeSinceStartup = 0f;

        private static float _timeScale = 1f;

        private static int _frameCount = 0;

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
