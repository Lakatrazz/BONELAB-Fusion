using LabFusion.Extensions;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Network.Serialization;
using LabFusion.Utilities;
using LabFusion.Bonelab.Patching;
using LabFusion.Bonelab.Extenders;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Bonelab.Messages;

public class SpawnGunSelectData : INetSerializable
{
    public int? GetSize() => sizeof(ushort) + Barcode.GetSize();

    public ushort SpawnGunID;
    public string Barcode;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref SpawnGunID);
        serializer.SerializeValue(ref Barcode);
    }
}

[Net.DelayWhileTargetLoading]
public class SpawnGunSelectMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<SpawnGunSelectData>();

        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.SpawnGunID);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<SpawnGunExtender>();

        if (extender == null)
        {
            return;
        }

        var crateReference = new SpawnableCrateReference(data.Barcode);

        // Don't update the preview mesh, we don't have that item
        if (crateReference.Crate == null)
        {
            return;
        }

        SpawnGunPatches.IgnorePatches = true;

        try
        {
            extender.Component.OnSpawnableSelected(crateReference.Crate);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("executing SpawnGun.OnSpawnableSelected", e);
        }

        SpawnGunPatches.IgnorePatches = false;
    }
}