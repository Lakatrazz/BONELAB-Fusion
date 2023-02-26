using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities {
    public delegate bool UserOverride(PlayerId id);

    public static class FusionOverrides {
        private static UserOverride _onValidateNametag;
        public static event UserOverride OnValidateNametag {
            add {
                _onValidateNametag += value;

                ForceUpdateOverrides();
            }
            remove {
                _onValidateNametag -= value;

                ForceUpdateOverrides();
            }
        }

        public static event Action OnOverridesChanged;

        public static bool ValidateNametag(PlayerId id) {
            if (_onValidateNametag == null)
                return true;

            foreach (var invocation in _onValidateNametag.GetInvocationList()) {
                var accessEvent = (UserOverride)invocation;

                if (!accessEvent.Invoke(id))
                    return false;
            }

            return true;
        }

        public static void ForceUpdateOverrides() => OnOverridesChanged.InvokeSafe("executing OnOverridesChanged");
    }
}
