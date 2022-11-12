using LabFusion.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Utilities {
    internal static class PhysicsUtilities {
        internal static bool CanModifyGravity = false;

        internal static void OnSendPhysicsInformation() {
            if (NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create()) {
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
