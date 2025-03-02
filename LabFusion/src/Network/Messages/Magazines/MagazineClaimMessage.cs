using LabFusion.Utilities;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class MagazineClaimData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public byte owner;
    public ushort entityId;
    public Handedness handedness;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref owner);
        serializer.SerializeValue(ref entityId);
        serializer.SerializeValue(ref handedness, Precision.OneByte);
    }

    public static MagazineClaimData Create(byte owner, ushort entityId, Handedness handedness)
    {
        return new MagazineClaimData()
        {
            owner = owner,
            entityId = entityId,
            handedness = handedness,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class MagazineClaimMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.MagazineClaim;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<MagazineClaimData>();

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.entityId);

        if (entity == null)
        {
            return;
        }

        var magazineExtender = entity.GetExtender<MagazineExtender>();

        if (magazineExtender == null)
        {
            return;
        }

        if (NetworkPlayerManager.TryGetPlayer(data.owner, out var player))
        {
            MagazineUtilities.GrabMagazine(magazineExtender.Component, player, data.handedness);
        }
    }
}