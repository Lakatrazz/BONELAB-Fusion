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
    public enum TwoButtonRemoteControllerEventType {
        UNKNOWN = 0,
        DEENERGIZEJOINT = 1,
        ENERGIZEJOINT = 2,
        ENERGIZEJOINTNEGATIVE = 3,
    }

    public class TwoButtonRemoteControllerEventData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) * 2 + sizeof(ushort);

        public byte smallId;
        public ushort syncId;
        public TwoButtonRemoteControllerEventType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(syncId);
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            syncId = reader.ReadUInt16();
            type = (TwoButtonRemoteControllerEventType)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static TwoButtonRemoteControllerEventData Create(byte smallId, ushort syncId, TwoButtonRemoteControllerEventType type)
        {
            return new TwoButtonRemoteControllerEventData()
            {
                smallId = smallId,
                syncId = syncId,
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class TwoButtonRemoteControllerEventMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.TwoButtonRemoteControllerEvent;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<TwoButtonRemoteControllerEventData>())
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
                        if (SyncManager.TryGetSyncable(data.syncId, out var syncable) && syncable is PropSyncable propSyncable && propSyncable.TryGetExtender<TwoButtonRemoteControllerExtender>(out var extender)) {
                            TwoButtonRemoteControllerPatches.IgnorePatches = true;
                            
                            switch (data.type) {
                                default:
                                case TwoButtonRemoteControllerEventType.UNKNOWN:
                                    break;
                                case TwoButtonRemoteControllerEventType.DEENERGIZEJOINT:
                                    extender.Component.DEENERGIZEJOINT();
                                    break;
                                case TwoButtonRemoteControllerEventType.ENERGIZEJOINT:
                                    extender.Component.ENERGIZEJOINT();
                                    break;
                                case TwoButtonRemoteControllerEventType.ENERGIZEJOINTNEGATIVE:
                                    extender.Component.ENERGIZEJOINTNEGATIVE();
                                    break;
                            }

                            TwoButtonRemoteControllerPatches.IgnorePatches = false;
                        }
                    }
                }
            }
        }
    }
}
