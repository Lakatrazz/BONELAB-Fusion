using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Senders;

using UnityEngine;



namespace LabFusion.Utilities
{
    internal static class PhysicsUtilities
    {
        internal static bool CanModifyGravity = false;

        private static readonly Vector3 DefaultGravity = new(0f, -9.81f, 0f);

        public static Vector3 Gravity = DefaultGravity;

        internal static void OnInitializeMelon()
        {
            MultiplayerHooking.OnPlayerCatchup += OnPlayerCatchup;
        }

        private static void OnPlayerCatchup(ulong id)
        {
            using var writer = FusionWriter.Create(WorldGravityMessageData.Size);
            var data = WorldGravityMessageData.Create(Gravity);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.WorldGravity, writer);
            MessageSender.SendFromServer(id, NetworkChannel.Reliable, message);
        }

        internal static void SendGravity(Vector3 value)
        {
            using var writer = FusionWriter.Create(WorldGravityMessageData.Size);
            var data = WorldGravityMessageData.Create(value);
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

                            Vector3 force = -Gravity * mult;

                            foreach (var rb in rm.physicsRig.selfRbs)
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
    }
}
