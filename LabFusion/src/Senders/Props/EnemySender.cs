using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;

using Il2CppSLZ.Marrow.PuppetMasta;
using Il2CppSLZ.Marrow.AI;

namespace LabFusion.Senders
{
    public static class EnemySender
    {
        public static void SendLocoState(PropSyncable syncable, BehaviourBaseNav.LocoState locoState)
        {
            using var writer = FusionWriter.Create(BehaviourBaseNavLocoData.Size);
            var data = BehaviourBaseNavLocoData.Create(PlayerIdManager.LocalSmallId, syncable, locoState);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.BehaviourBaseNavLoco, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }

        public static void SendMentalState(PropSyncable syncable, BehaviourBaseNav.MentalState mentalState, TriggerRefProxy proxy = null)
        {
            using var writer = FusionWriter.Create(BehaviourBaseNavMentalData.Size);
            var data = BehaviourBaseNavMentalData.Create(PlayerIdManager.LocalSmallId, syncable, mentalState, proxy);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.BehaviourBaseNavMental, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }
    }
}
