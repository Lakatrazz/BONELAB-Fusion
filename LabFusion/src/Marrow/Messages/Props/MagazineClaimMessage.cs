using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Marrow.Messages;

public class MagazineClaimData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + sizeof(ushort);

    public int? GetSize() => Size;

    public byte OwnerID;
    public ushort EntityID;
    public Handedness Handedness;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref OwnerID);
        serializer.SerializeValue(ref EntityID);
        serializer.SerializeValue(ref Handedness, Precision.OneByte);
    }
}

[Net.SkipHandleWhileLoading]
public class MagazineClaimMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<MagazineClaimData>();

        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.EntityID);

        if (entity == null)
        {
            return;
        }

        var magazineExtender = entity.GetExtender<MagazineExtender>();

        if (magazineExtender == null)
        {
            return;
        }

        if (NetworkPlayerManager.TryGetPlayer(data.OwnerID, out var player))
        {
            MagazineUtilities.GrabMagazine(magazineExtender.Component, player, data.Handedness);
        }
    }
}