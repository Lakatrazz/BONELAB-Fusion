using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities
{
    public static partial class FusionSceneManager {
        private static Action _onLevelLoad = null;

        public static void HookOnLevelLoad(Action action) {
            if (!HasTargetLoaded()) {
                _onLevelLoad += action;
            }
            else {
                action?.Invoke();
            }
        }
    }
}
