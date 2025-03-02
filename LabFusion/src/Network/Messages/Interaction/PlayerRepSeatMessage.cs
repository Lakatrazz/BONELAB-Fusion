using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Network;

public class PlayerRepSeatData : INetSerializable
{
    public const int Size = sizeof(byte) * 3 + sizeof(ushort);

    public byte sitterId;
    public ushort seatId;
    public byte seatIndex;
    public bool isIngress;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref sitterId);
        serializer.SerializeValue(ref seatId);
        serializer.SerializeValue(ref seatIndex);
        serializer.SerializeValue(ref isIngress);
    }

    public static PlayerRepSeatData Create(byte sitterId, ushort seatId, byte seatIndex, bool isIngress)
    {
        return new PlayerRepSeatData
        {
            sitterId = sitterId,
            seatId = seatId,
            seatIndex = seatIndex,
            isIngress = isIngress,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class PlayerRepSeatMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepSeat;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepSeatData>();

        if (!NetworkPlayerManager.TryGetPlayer(data.sitterId, out var player))
        {
            return;
        }

        NetworkEntity seatEntity = null;

        NetworkEntityManager.HookEntityRegistered(data.seatId, OnSeatRegistered);

        void OnSeatRegistered(NetworkEntity entity)
        {
            seatEntity = entity;

            player.HookOnReady(OnPlayerReady);
        }

        void OnPlayerReady()
        {
            var seatExtender = seatEntity.GetExtender<SeatExtender>();

            if (seatExtender == null)
            {
                return;
            }

            var seat = seatExtender.GetComponent(data.seatIndex);

            if (seat == null)
            {
                return;
            }

            SeatPatches.IgnorePatches = true;

            if (data.isIngress)
            {
                seat.IngressRig(player.RigRefs.RigManager);
            }
            else if (player.RigRefs.RigManager.activeSeat)
            {
                player.RigRefs.RigManager.activeSeat.EgressRig(true);
            }
        }
    }
}
