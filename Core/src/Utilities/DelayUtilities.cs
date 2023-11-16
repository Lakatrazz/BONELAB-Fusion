using MelonLoader;

using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities
{
    public static class DelayUtilities
    {
        private class FrameDelayInfo
        {
            public Action action;
            public int counter;
        }

        private static readonly List<FrameDelayInfo> _delays = new();

        public static void Delay(Action action, int frames)
        {
            _delays.Add(new FrameDelayInfo()
            {
                action = action,
                counter = frames,
            });
        }

        internal static void Internal_OnUpdate()
        {
            // We go backwards so we can remove items without disrupting the list
            int count = _delays.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                // Check the counter, if we've reached 0 then invoke the event
                var delay = _delays[i];

                if (delay.counter <= 0)
                {
                    try
                    {
                        delay.action();
                    }
                    catch (Exception e)
                    {
                        FusionLogger.Error(e.ToString());
                    }

                    _delays.RemoveAt(i);
                    continue;
                }

                // Otherwise, decrement it and wait for the next frame
                delay.counter--;
            }
        }
    }
}
