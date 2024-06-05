using LabFusion.Data;
using LabFusion.Syncables;
using PuppetMasta;

namespace LabFusion.Network
{
    public class BehaviourBaseNavLocoData : IFusionSerializable
    {
        public const int Size = sizeof(byte) * 2 + sizeof(ushort);

        public byte ownerId;
        public ushort syncId;
        public BehaviourBaseNav.LocoState locoState;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(ownerId);
            writer.Write(syncId);

            writer.Write((byte)locoState);
        }

        public void Deserialize(FusionReader reader)
        {
            ownerId = reader.ReadByte();
            syncId = reader.ReadUInt16();

            locoState = (BehaviourBaseNav.LocoState)reader.ReadByte();
        }

        public PropSyncable GetPropSyncable()
        {
            if (SyncManager.TryGetSyncable<PropSyncable>(syncId, out var syncable))
                return syncable;

            return null;
        }

        public static BehaviourBaseNavLocoData Create(byte ownerId, PropSyncable syncable, BehaviourBaseNav.LocoState locoState)
        {
            var syncId = syncable.GetId();

            var data = new BehaviourBaseNavLocoData
            {
                ownerId = ownerId,
                syncId = syncId,
                locoState = locoState,
            };

            return data;
        }
    }

    [Net.SkipHandleWhileLoading]
    public class BehaviourBaseNavLocoMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.BehaviourBaseNavLoco;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using var reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<BehaviourBaseNavLocoData>();
            // Send message to other clients if server
            if (NetworkInfo.IsServer && isServerHandled)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.ownerId, NetworkChannel.Reliable, message, false);
            }
            else
            {
                // Find the prop syncable and update its behaviour nav
                var syncable = data.GetPropSyncable();
                if (syncable != null && syncable.TryGetExtender<BehaviourBaseNavExtender>(out var extender))
                {
                    extender.SwitchLocoState(data.locoState);
                }
            }
        }
    }
}
