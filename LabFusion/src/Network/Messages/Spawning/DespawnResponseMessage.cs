using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Marrow;
using LabFusion.Entities;

using MelonLoader;

using System.Collections;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Audio;

namespace LabFusion.Network;

public class DespawnResponseData : IFusionSerializable
{
    public const int Size = sizeof(ushort) + sizeof(byte) * 2;

    public ushort syncId;
    public byte despawnerId;
    public bool isMag;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(syncId);
        writer.Write(despawnerId);
        writer.Write(isMag);
    }

    public void Deserialize(FusionReader reader)
    {
        syncId = reader.ReadUInt16();
        despawnerId = reader.ReadByte();
        isMag = reader.ReadBoolean();
    }

    public static DespawnResponseData Create(ushort syncId, byte despawnerId, bool isMag = false)
    {
        return new DespawnResponseData()
        {
            syncId = syncId,
            despawnerId = despawnerId,
            isMag = isMag,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class DespawnResponseMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.DespawnResponse;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        // Despawn the poolee if it exists
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<DespawnResponseData>();

        MelonCoroutines.Start(Internal_WaitForValidDespawn(data.syncId, data.despawnerId, data.isMag));
    }

    private static IEnumerator Internal_WaitForValidDespawn(ushort syncId, byte despawnerId, bool isMag)
    {
        // Delay at most 300 frames until this entity exists
        int i = 0;
        while (!NetworkEntityManager.IdManager.RegisteredEntities.HasEntity(syncId))
        {
            yield return null;

            i++;

            if (i >= 300)
            {
                yield break;
            }
        }

        // Get the entity from the valid id
        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(syncId);

        if (entity == null)
        {
            yield break;
        }

        var pooleeExtender = entity.GetExtender<PooleeExtender>();

        if (pooleeExtender == null)
        {
            yield break;
        }

        PooleeUtilities.CanDespawn = true;

        var poolee = pooleeExtender.Component;

        if (isMag)
        {
            AmmoInventory ammoInventory = AmmoInventory.Instance;

            if (NetworkPlayerManager.TryGetPlayer(despawnerId, out var rep))
            {
                ammoInventory = rep.RigReferences.RigManager.GetComponentInChildren<AmmoInventory>(true);
            }

            SafeAudio3dPlayer.PlayAtPoint(ammoInventory.ammoReceiver.grabClips, ammoInventory.ammoReceiver.transform.position, Audio3dManager.softInteraction, 0.2f);

            poolee.gameObject.SetActive(false);
        }
        else
        {
            poolee.Despawn();
        }

        NetworkEntityManager.IdManager.UnregisterEntity(entity);

        PooleeUtilities.CanDespawn = false;
    }
}