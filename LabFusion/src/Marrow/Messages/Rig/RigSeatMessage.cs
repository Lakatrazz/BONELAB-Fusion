using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Marrow.Patching;
using LabFusion.Marrow.Extenders;
using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Network.Messages;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Messages;

public class RigSeatData : INetSerializable
{
    public const int Size = NetworkEntityReference.Size + ComponentIndexData.Size + sizeof(bool) * 2;

    public NetworkEntityReference RigReference;

    public ComponentIndexData SeatReference;

    public bool IsSeated;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref RigReference);

        serializer.SerializeValue(ref SeatReference);

        serializer.SerializeValue(ref IsSeated);
    }

    public bool TryGetSeatAndEntity(out Seat seat, out NetworkEntity seatEntity)
    {
        return SeatReference.TryGetComponentAndEntity<Seat, SeatExtender>(out seat, out seatEntity);
    }
}

[Net.SkipHandleWhileLoading]
public class RigSeatMessage : ModuleMessageHandler
{
    public static readonly float SeatIgnoreTime = 0.5f;

    protected override bool OnPreRelayMessage(ReceivedMessage received) => CommonMessageValidation.ValidateSenderOwnsEntity(received);

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RigSeatData>();

        if (!data.RigReference.TryGetEntity(out var rigEntity))
        {
            return;
        }

        var networkRig = rigEntity.GetExtender<NetworkRig>();

        if (networkRig == null)
        {
            return;
        }

        networkRig.HookOnReady(OnRigReady);

        void OnRigReady()
        {
            if (!data.TryGetSeatAndEntity(out var seat, out var seatEntity))
            {
                return;
            }

            var marrowEntityExtender = seatEntity.GetExtender<IMarrowEntityExtender>();

            MarrowEntity marrowEntity = null;

            if (marrowEntityExtender != null)
            {
                marrowEntity = marrowEntityExtender.MarrowEntity;
            }

            SeatPatches.IgnorePatches = true;

            if (data.IsSeated)
            {
                seat.IngressRig(networkRig.RigRefs.RigManager);

                if (marrowEntity != null)
                {
                    networkRig.Ignorer.TimedIgnoreEntity(marrowEntity, SeatIgnoreTime);
                }
            }
            else if (networkRig.RigRefs.RigManager.activeSeat)
            {
                networkRig.RigRefs.RigManager.activeSeat.EgressRig(true);

                if (marrowEntity != null)
                {
                    networkRig.Ignorer.CancelIgnoreEntity(marrowEntity);
                }
            }
        }
    }
}
