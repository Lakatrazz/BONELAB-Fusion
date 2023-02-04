#if DEBUG
using LabFusion.Utilities;

using System;
using System.Diagnostics;

namespace LabFusion.Debugging {
    public class FusionStopwatch {
        private const ConsoleColor _color = ConsoleColor.Cyan;

        private static FusionStopwatch _active;

        private Stopwatch _watch;

        public static void Create() {
            if (_active != null) {

            }

            _active = new FusionStopwatch {
                _watch = Stopwatch.StartNew()
            };
        }

        public static void BreakPoint(string task, out float ms) {
            ms = 0f;

            if (_active == null)
                FusionLogger.Warn("Tried to break a FusionStopwatch, but there is no active watch!");
            else {
                _active._watch.Stop();
                ms = (float)_active._watch.Elapsed.TotalMilliseconds;

                if (task != null)
                    FusionLogger.Log($"{task}: {ms}", _color);

                _active._watch.Restart();
            }
        }

        public static void BreakPoint(string task) {
            BreakPoint(task, out _);
        }

        public static void Finish(string task, out float ms) {
            ms = 0f;

            if (_active == null)
                FusionLogger.Warn("Tried to finish a FusionStopwatch, but there is no active watch!");
            else {
                _active._watch.Stop();
                ms = (float)_active._watch.Elapsed.TotalMilliseconds;

                if (task != null)
                    FusionLogger.Log($"{task}: {ms}", _color);
                
                _active = null;
            }
        }

        public static void Finish(string task) {
            Finish(task, out _);
        }
    }
}
#endif