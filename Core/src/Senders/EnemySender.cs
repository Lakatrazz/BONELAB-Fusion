using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;

using PuppetMasta;

using SLZ.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders {
    public static class EnemySender {
        public static void SendLocoState(PropSyncable syncable, BehaviourBaseNav.LocoState locoState)
        {
            using (var writer = FusionWriter.Create(BehaviourBaseNavLocoData.Size))
            {
                using (var data = BehaviourBaseNavLocoData.Create(PlayerIdManager.LocalSmallId, syncable, locoState))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.BehaviourBaseNavLoco, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendMentalState(PropSyncable syncable, BehaviourBaseNav.MentalState mentalState, TriggerRefProxy proxy = null) {
            using (var writer = FusionWriter.Create(BehaviourBaseNavMentalData.Size))
            {
                using (var data = BehaviourBaseNavMentalData.Create(PlayerIdManager.LocalSmallId, syncable, mentalState, proxy))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.BehaviourBaseNavMental, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }
    }
}
