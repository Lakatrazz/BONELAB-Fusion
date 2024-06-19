using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Patching;
using LabFusion.Representation;

namespace LabFusion.Network
{
    public class PlayerRepSeatData : IFusionSerializable
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
            using var reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<PlayerRepSeatData>();

            // Send message to other clients if server
            if (isServerHandled)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                return;
            }

            if (!NetworkPlayerManager.TryGetPlayer(data.smallId, out var rep))
            {
                return;
            }

            var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.syncId);

            if (entity == null)
            {
                return;
            }

            var seatExtender = entity.GetExtender<SeatExtender>();

            if (seatExtender == null)
            {
                return;
            }

            var seat = seatExtender.GetComponent(data.seatIndex);

            if (seat)
            {
                SeatPatches.IgnorePatches = true;

                if (data.isIngress)
                {
                    seat.IngressRig(rep.RigReferences.RigManager);
                }
                else if (rep.RigReferences.RigManager.activeSeat)
                {
                    rep.RigReferences.RigManager.activeSeat.EgressRig(true);
                }

                SeatPatches.IgnorePatches = false;
            }
        }
    }
}
