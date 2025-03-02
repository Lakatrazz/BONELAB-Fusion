using LabFusion.Extensions;
using LabFusion.Patching;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class SpawnGunSelectData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public byte smallId;
    public ushort gunId;
    public string barcode;

    public static int GetSize(string barcode)
    {
        return Size + barcode.GetSize();
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref gunId);
        serializer.SerializeValue(ref barcode);
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
public class SpawnGunSelectMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.SpawnGunSelect;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<SpawnGunSelectData>();

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