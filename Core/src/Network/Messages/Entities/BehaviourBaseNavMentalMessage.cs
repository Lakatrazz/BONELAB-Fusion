using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Syncables;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LabFusion.Extensions;
using PuppetMasta;
using SLZ.AI;

namespace LabFusion.Network
{
    public class BehaviourBaseNavMentalData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) * 2 + sizeof(ushort);

        public byte ownerId;
        public ushort syncId;
        public BehaviourBaseNav.MentalState mentalState;
        public SerializedTriggerRefReference triggerRef;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(ownerId);
            writer.Write(syncId);

            writer.Write((byte)mentalState);

            switch (mentalState) {
                case BehaviourBaseNav.MentalState.Engaged:
                case BehaviourBaseNav.MentalState.Agroed:
                    writer.Write(triggerRef);
                    break;
            }
        }

        public void Deserialize(FusionReader reader)
        {
            ownerId = reader.ReadByte();
            syncId = reader.ReadUInt16();

            mentalState = (BehaviourBaseNav.MentalState)reader.ReadByte();

            switch (mentalState) {
                case BehaviourBaseNav.MentalState.Engaged:
                case BehaviourBaseNav.MentalState.Agroed:
                    triggerRef = reader.ReadFusionSerializable<SerializedTriggerRefReference>();
                    break;
            }
        }

        public PropSyncable GetPropSyncable() {
            if (SyncManager.TryGetSyncable(syncId, out var syncable) && syncable is PropSyncable propSyncable)
                return propSyncable;

            return null;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static BehaviourBaseNavMentalData Create(byte ownerId, PropSyncable syncable, BehaviourBaseNav.MentalState mentalState, TriggerRefProxy proxy)
        {
            var syncId = syncable.GetId();

            var data = new BehaviourBaseNavMentalData
            {
                ownerId = ownerId,
                syncId = syncId,
                mentalState = mentalState,
                triggerRef = new SerializedTriggerRefReference(proxy),
            };

            return data;
        }
    }

    [Net.SkipHandleWhileLoading]
    public class BehaviourBaseNavMentalMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.BehaviourBaseNavMental;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<BehaviourBaseNavMentalData>()) {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled)
                    {
                        using (var message = FusionMessage.Create(Tag.Value, bytes))
                        {
                            MessageSender.BroadcastMessageExcept(data.ownerId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        // Get the prop syncable, check if it has a behaviour
                        // Then, set the mental state
                        var syncable = data.GetPropSyncable();
                        if (syncable != null && syncable.TryGetExtender<BehaviourBaseNavExtender>(out var extender)) {
                            extender.SwitchMentalState(data.mentalState, data.triggerRef?.proxy);
                        }
                    }
                }
            }
        }
    }
}
