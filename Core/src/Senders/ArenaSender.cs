using LabFusion.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders {
    public static class ArenaSender {
        public static void SendArenaTransition(ArenaTransitionType type) {
            if (NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = ArenaTransitionData.Create(type))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.ArenaTransition, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        public static void SendChallengeSelect(byte menuIndex, byte challengeNumber, ChallengeSelectType type) {
            if (NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = ChallengeSelectData.Create(menuIndex, challengeNumber, type))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.ChallengeSelect, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        public static void SendGeometryChange(byte geoIndex)
        {
            if (NetworkInfo.IsServer)
            {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = GeoSelectData.Create(geoIndex))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.GeoSelect, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        public static void SendMenuSelection(byte selectionNumber, ArenaMenuType type) {
            if (NetworkInfo.IsServer)
            {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = ArenaMenuData.Create(selectionNumber, type))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.ArenaMenu, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

    }
}
