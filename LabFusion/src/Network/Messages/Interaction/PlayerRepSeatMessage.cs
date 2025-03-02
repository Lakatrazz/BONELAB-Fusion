using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Network;

public class PlayerRepSeatData : INetSerializable
{
    public const int Size = sizeof(byte) * 3 + sizeof(ushort);

    public byte smallId;
    public ushort syncId;
    public byte seatIndex;
    public bool isIngress;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref syncId);
        serializer.SerializeValue(ref seatIndex);
        serializer.SerializeValue(ref isIngress);
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
public class PlayerRepSeatMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepSeat;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepSeatData>();

        if (!NetworkPlayerManager.TryGetPlayer(data.smallId, out var player))
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
                seat.IngressRig(player.RigRefs.RigManager);
            }
            else if (player.RigRefs.RigManager.activeSeat)
            {
                player.RigRefs.RigManager.activeSeat.EgressRig(true);
            }

            SeatPatches.IgnorePatches = false;
        }
    }
}
