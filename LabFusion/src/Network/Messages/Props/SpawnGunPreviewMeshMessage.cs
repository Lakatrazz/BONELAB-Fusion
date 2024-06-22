using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Patching;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Network;

public class SpawnGunPreviewMeshData : IFusionSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public byte smallId;
    public ushort syncId;
    public string barcode;

    public static int GetSize(string barcode)
    {
        return Size + barcode.GetSize();
    }

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);
        writer.Write(syncId);
        writer.Write(barcode);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();
        syncId = reader.ReadUInt16();
        barcode = reader.ReadString();
    }

    public static SpawnGunPreviewMeshData Create(byte smallId, ushort syncId, string barcode)
    {
        return new SpawnGunPreviewMeshData()
        {
            smallId = smallId,
            syncId = syncId,
            barcode = barcode,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class SpawnGunPreviewMeshMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.SpawnGunPreviewMesh;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<SpawnGunPreviewMeshData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);

            return;
        }

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.syncId);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<SpawnGunExtender>();

        if (extender == null)
        {
            return;
        }

        var crateRef = new SpawnableCrateReference(data.barcode);

        if (crateRef.Crate != null)
        {
            SpawnGunPatches.IgnorePatches = true;

            extender.Component._selectedCrate = crateRef.Crate;
            extender.Component.SetPreviewMesh();

            SpawnGunPatches.IgnorePatches = false;
        }
    }
}