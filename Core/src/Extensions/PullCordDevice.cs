using SLZ.Props;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Extensions {
    public static class PullCordDeviceExtensions {
        public static void TryEnableBall(this PullCordDevice device) {
            if (!device.ballShown) {
                device.EnableBall();
                device.ballShown = true;
                device.ballHost.EnableInteraction();
            }
        }

        public static void TryDisableBall(this PullCordDevice device) {
            if (device.ballShown)
            {
                device.DisableBall();
                device.ballShown = false;
                device.ballHost.DisableInteraction();
            }
        }
    }
}
