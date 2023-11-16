using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Senders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SystemVector3 = System.Numerics.Vector3;

namespace LabFusion.Utilities
{
    internal static class PhysicsUtilities
    {
        internal static bool CanModifyGravity = false;

        private static readonly SystemVector3 DefaultGravity = new(0f, -9.81f, 0f);

        public static SystemVector3 Gravity = DefaultGravity;

        internal static void OnInitializeMelon()
        {
            MultiplayerHooking.OnPlayerCatchup += OnPlayerCatchup;
        }

        private static void OnPlayerCatchup(ulong id)
        {
            using var writer = FusionWriter.Create(WorldGravityMessageData.Size);
            using var data = WorldGravityMessageData.Create(Gravity);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.WorldGravity, writer);
            MessageSender.SendFromServer(id, NetworkChannel.Reliable, message);
        }

        internal static void SendGravity(SystemVector3 value)
        {
            using var writer = FusionWriter.Create(WorldGravityMessageData.Size);
            using var data = WorldGravityMessageData.Create(value);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.WorldGravity, writer);
            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
        }

        internal static void OnUpdateTimescale()
        {
            if (NetworkInfo.HasServer)
            {
                var mode = FusionPreferences.TimeScaleMode;
                var references = RigData.RigReferences;
                var rm = references.RigManager;

                switch (mode)
                {
                    case TimeScaleMode.DISABLED:
                        Time.timeScale = 1f;
                        break;
                    case TimeScaleMode.LOW_GRAVITY:
                        Time.timeScale = 1f;

                        if (RigData.HasPlayer)
                        {
                            var controlTime = rm.openControllerRig.globalTimeControl;
                            float intensity = controlTime.cur_intensity;
                            if (intensity <= 0f)
                                break;

                            float mult = 1f - (1f / controlTime.cur_intensity);

                            SystemVector3 force = -Gravity * mult;

                            if (references.RigRigidbodies == null)
                                references.GetRigidbodies();

                            var rbs = references.RigRigidbodies;

                            foreach (var rb in rbs)
                            {
                                if (rb == null)
                                    continue;

                                if (rb.useGravity)
                                {
                                    rb.AddForce(force.ToUnityVector3(), ForceMode.Acceleration);
                                }
                            }
                        }

                        break;
                }
            }
        }
    }
}
