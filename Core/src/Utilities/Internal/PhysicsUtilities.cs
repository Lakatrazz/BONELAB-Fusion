using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Senders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities {
    internal static class PhysicsUtilities {
        internal static bool CanModifyGravity = false;

        internal static void OnUpdateTimescale() {
            if (NetworkInfo.HasServer) {
                var mode = FusionPreferences.TimeScaleMode;

                switch (mode)
                {
                    case TimeScaleMode.DISABLED:
                        Time.timeScale = 1f;
                        break;
                    case TimeScaleMode.LOW_GRAVITY:
                        Time.timeScale = 1f;

                        if (RigData.HasPlayer)
                        {
                            var controlTime = RigData.RigReferences.RigManager.openControllerRig.globalTimeControl;
                            float mult = 1f - (1f / controlTime.cur_intensity);
                            if (float.IsNaN(mult) || mult == 0f || float.IsPositiveInfinity(mult) || float.IsNegativeInfinity(mult))
                                break;

                            Vector3 force = -Physics.gravity * mult;

                            if (RigData.RigReferences.RigRigidbodies == null)
                                RigData.RigReferences.GetRigidbodies();

                            var rbs = RigData.RigReferences.RigRigidbodies;

                            foreach (var rb in rbs)
                            {
                                if (rb.useGravity)
                                {
                                    rb.AddForce(force, ForceMode.Acceleration);
                                }
                            }
                        }

                        break;
                }
            }
        }

        internal static void OnSendPhysicsInformation() {
            if (NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create(WorldGravityMessageData.Size)) {
                    using (var data = WorldGravityMessageData.Create(Physics.gravity)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.WorldGravity, writer)) {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Unreliable, message);
                        }
                    }
                }
            }
        }
    }
}
