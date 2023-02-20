using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Debugging {
    public static class FusionDevMode {
#if DEBUG
        public const bool UnlockEverything = false;
#else
        public const bool UnlockEverything = false;
#endif
    }
}
