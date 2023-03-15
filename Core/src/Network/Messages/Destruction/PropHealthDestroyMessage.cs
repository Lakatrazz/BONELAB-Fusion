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
    public class PropHealthDestroyData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) * 2 + sizeof(ushort);

        public byte smallId;
        public ushort syncId;
        public byte healthIndex;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(syncId);
            writer.Write(healthIndex);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            syncId = reader.ReadUInt16();
            healthIndex = reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PropHealthDestroyData Create(byte smallId, ushort syncId, byte healthIndex)
        {
            return new PropHealthDestroyData()
            {
                smallId = smallId,
                syncId = syncId,
                healthIndex = healthIndex,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class PropHealthDestroyMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PropHealthDestroy;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<PropHealthDestroyData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else
                    {
                        if (SyncManager.TryGetSyncable(data.syncId, out var health) && health is PropSyncable healthSyncable && healthSyncable.TryGetExtender<PropHealthExtender>(out var extender))
                        {
                            var propHealth = extender.GetComponent(data.healthIndex);
                            PropHealthPatches.IgnorePatches = true;

                            propHealth.hits = propHealth.req_hit_count + 1;
                            propHealth.bloodied = true;

                            propHealth.TIMEDKILL();
                            propHealth.SETPROP();

                            PropHealthPatches.IgnorePatches = false;
                        }
                    }
                }
            }
        }
    }
}
