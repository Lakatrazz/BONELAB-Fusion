using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;
using SLZ.Rig;
using SLZ.VRMK;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Network
{
    public class PlayerRepSeatData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) * 3 + sizeof(ushort);

        public byte smallId;
        public ushort syncId;
        public byte seatIndex;
        public bool isIngress;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(syncId);
            writer.Write(seatIndex);
            writer.Write(isIngress);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            syncId = reader.ReadUInt16();
            seatIndex = reader.ReadByte();
            isIngress = reader.ReadBoolean();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static PlayerRepSeatData Create(byte smallId, ushort syncId, byte seatIndex, bool isIngress)
        {
            return new PlayerRepSeatData
            {
                smallId = smallId,
                syncId = syncId,
                seatIndex = seatIndex,
                isIngress = isIngress,
            };
        }
    }

    [Net.SkipHandleWhileLoading]
    public class PlayerRepSeatMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PlayerRepSeat;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (var reader = FusionReader.Create(bytes))
            {
                var data = reader.ReadFusionSerializable<PlayerRepSeatData>();

                // Send message to other clients if server
                if (NetworkInfo.IsServer && isServerHandled) {
                    using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                        MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                    }
                }
                else if (PlayerRepManager.TryGetPlayerRep(data.smallId, out var rep) && SyncManager.TryGetSyncable(data.syncId, out var syncable) && syncable is PropSyncable seatSyncable && seatSyncable.TryGetExtender<SeatExtender>(out var extender)) {
                    var seat = extender.GetComponent(data.seatIndex);

                    if (seat) {
                        SeatPatches.IgnorePatches = true;

                        if (data.isIngress)
                            seat.IngressRig(rep.RigReferences.RigManager);
                        else if (rep.RigReferences.RigManager.activeSeat)
                            rep.RigReferences.RigManager.activeSeat.EgressRig(true);

                        seatSyncable.PushUpdate();

                        SeatPatches.IgnorePatches = false;
                    }
                }
            }
        }
    }
}
