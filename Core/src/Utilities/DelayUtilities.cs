using MelonLoader;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities {
    public static class DelayUtilities {
        public static void DelayFrames(Action action, int frames) {
            MelonCoroutines.Start(Internal_CoDelayFrames(action, frames));
        }

        private static IEnumerator Internal_CoDelayFrames(Action action, int frames) {
            for (var i = 0; i < frames; i++)
                yield return null;

            action();
        }
    }
}
