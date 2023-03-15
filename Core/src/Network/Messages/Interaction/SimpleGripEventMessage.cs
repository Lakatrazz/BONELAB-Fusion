using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Syncables;
using LabFusion.Patching;

using SLZ;
using SLZ.Interaction;
using SLZ.Props.Weapons;

namespace LabFusion.Network
{
    public enum SimpleGripEventType {
        TRIGGER_DOWN = 0,
        MENU_TAP = 1,
    }

    public class SimpleGripEventData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) * 3 + sizeof(ushort);

        public byte smallId;
        public ushort syncId;
        public byte gripEventIndex;
        public SimpleGripEventType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(syncId);
            writer.Write(gripEventIndex);
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            syncId = reader.ReadUInt16();
            gripEventIndex = reader.ReadByte();
            type = (SimpleGripEventType)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static SimpleGripEventData Create(byte smallId, ushort syncId, byte gripEventIndex, SimpleGripEventType type)
        {
            return new SimpleGripEventData()
            {
                smallId = smallId,
                syncId = syncId,
                gripEventIndex = gripEventIndex,
                type = type
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class SimpleGripEventMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SimpleGripEvent;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<SimpleGripEventData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled)
                    {
                        using (var message = FusionMessage.Create(Tag.Value, bytes))
                        {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else
                    {
                        if (SyncManager.TryGetSyncable(data.syncId, out var syncable) && syncable is PropSyncable propSyncable && propSyncable.TryGetExtender<SimpleGripEventsExtender>(out var extender)) {
                            var gripEvent = extender.GetComponent(data.gripEventIndex);

                            if (gripEvent) {
                                switch (data.type) {
                                    default:
                                    case SimpleGripEventType.TRIGGER_DOWN:
                                        gripEvent.OnIndexDown.Invoke();
                                        break;
                                    case SimpleGripEventType.MENU_TAP:
                                        gripEvent.OnMenuTapDown.Invoke();
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
