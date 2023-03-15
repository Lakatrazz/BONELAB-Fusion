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
    public class PowerableJointVoltageData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + sizeof(ushort) + sizeof(float);

        public byte smallId;
        public ushort syncId;
        public float voltage;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(syncId);
            writer.Write(voltage);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            syncId = reader.ReadUInt16();
            voltage = reader.ReadSingle();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PowerableJointVoltageData Create(byte smallId, ushort syncId, float voltage)
        {
            return new PowerableJointVoltageData()
            {
                smallId = smallId,
                syncId = syncId,
                voltage = voltage,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class PowerableJointVoltageMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PowerableJointVoltage;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<PowerableJointVoltageData>())
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
                        if (SyncManager.TryGetSyncable(data.syncId, out var syncable) && syncable is PropSyncable propSyncable && propSyncable.TryGetExtender<PowerableJointExtender>(out var extender)) {
                            PowerableJointPatches.IgnorePatches = true;
                            extender.Component.SETJOINT(data.voltage);
                            PowerableJointPatches.IgnorePatches = false;
                        }
                    }
                }
            }
        }
    }
}
