using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Network;

public class MagazineClaimData : IFusionSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public byte owner;
    public ushort entityId;
    public Handedness handedness;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(owner);
        writer.Write(entityId);
        writer.Write((byte)handedness);
    }

    public void Deserialize(FusionReader reader)
    {
        owner = reader.ReadByte();
        entityId = reader.ReadUInt16();
        handedness = (Handedness)reader.ReadByte();
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