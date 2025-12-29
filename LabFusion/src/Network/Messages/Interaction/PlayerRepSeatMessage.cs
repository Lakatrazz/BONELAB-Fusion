using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Patching;
using LabFusion.Marrow.Extenders;

namespace LabFusion.Network;

public class PlayerRepSeatData : INetSerializable
{
    public const int Size = sizeof(ushort) + sizeof(byte) * 2;

    public ushort SeatID;
    public byte SeatIndex;
    public bool IsIngress;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref SeatID);
        serializer.SerializeValue(ref SeatIndex);
        serializer.SerializeValue(ref IsIngress);
    }
}

[Net.SkipHandleWhileLoading]
public class PlayerRepSeatMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepSeat;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepSeatData>();

        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        if (!NetworkPlayerManager.TryGetPlayer(sender.Value, out var player))
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
