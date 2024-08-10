using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Patching;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Network;

public class SpawnGunSelectData : IFusionSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public byte smallId;
    public ushort gunId;
    public string barcode;

    public static int GetSize(string barcode)
    {
        return Size + barcode.GetSize();
    }

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(gunId);
        writer.Write(barcode);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        gunId = reader.ReadUInt16();
        barcode = reader.ReadString();
    }

    public static SpawnGunSelectData Create(byte smallId, ushort gunId, string barcode)
    {
        return new SpawnGunSelectData()
        {
            smallId = smallId,
            gunId = gunId,
            barcode = barcode,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class SpawnGunSelectMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.SpawnGunSelect;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<SpawnGunSelectData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            return;
        }

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.gunId);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<SpawnGunExtender>();

        if (extender == null)
        {
            return;
        }

        var crateReference = new SpawnableCrateReference(data.barcode);

        // Don't update the preview mesh, we don't have that item
        if (crateReference.Crate == null)
        {
            return;
        }

        SpawnGunPatches.IgnorePatches = true;

        extender.Component.OnSpawnableSelected(crateReference.Crate);

        SpawnGunPatches.IgnorePatches = false;
    }
}