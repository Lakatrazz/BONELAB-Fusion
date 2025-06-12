using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Patching;
using LabFusion.Marrow.Extenders;

namespace LabFusion.Network;

public class PlayerRepSeatData : INetSerializable
{
    public const int Size = sizeof(byte) * 3 + sizeof(ushort);

    public byte SitterID;
    public ushort SeatID;
    public byte SeatIndex;
    public bool IsIngress;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref SitterID);
        serializer.SerializeValue(ref SeatID);
        serializer.SerializeValue(ref SeatIndex);
        serializer.SerializeValue(ref IsIngress);
    }

    public static PlayerRepSeatData Create(byte sitterId, ushort seatId, byte seatIndex, bool isIngress)
    {
        return new PlayerRepSeatData
        {
            SitterID = sitterId,
            SeatID = seatId,
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

        if (!NetworkPlayerManager.TryGetPlayer(data.SitterID, out var player))
        {
            return;
        }

        var seatEntity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.SeatID);

        if (seatEntity == null)
        {
            return;
        }

        player.HookOnReady(OnPlayerReady);

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
