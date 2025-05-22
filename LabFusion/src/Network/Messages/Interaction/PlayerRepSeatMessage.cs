using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Network;

public class PlayerRepSeatData : INetSerializable
{
    public const int Size = sizeof(byte) * 3 + sizeof(ushort);

    public byte SitterId;
    public ushort SeatId;
    public byte SeatIndex;
    public bool IsIngress;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref SitterId);
        serializer.SerializeValue(ref SeatId);
        serializer.SerializeValue(ref SeatIndex);
        serializer.SerializeValue(ref IsIngress);
    }

    public static PlayerRepSeatData Create(byte sitterId, ushort seatId, byte seatIndex, bool isIngress)
    {
        return new PlayerRepSeatData
        {
            SitterId = sitterId,
            SeatId = seatId,
            SeatIndex = seatIndex,
            IsIngress = isIngress,
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

        if (!NetworkPlayerManager.TryGetPlayer(data.SitterId, out var player))
        {
            return;
        }

        NetworkEntity seatEntity = null;

        NetworkEntityManager.HookEntityRegistered(data.SeatId, OnSeatRegistered);

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

            var seat = seatExtender.GetComponent(data.SeatIndex);

            if (seat == null)
            {
                return;
            }

            SeatPatches.IgnorePatches = true;

            if (data.IsIngress)
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
