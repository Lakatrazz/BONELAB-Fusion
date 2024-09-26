using LabFusion.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Data;

namespace LabFusion.Network;

[Net.DelayWhileTargetLoading]
public class ObjectDestructableDestroyMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.ObjectDestructibleDestroy;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<ComponentIndexData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag, bytes);
            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);

            return;
        }

        var destructible = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.syncId);

        if (destructible == null)
        {
            return;
        }

        var extender = destructible.GetExtender<ObjectDestructibleExtender>();

        if (extender == null)
        {
            return;
        }

        var objectDestructible = extender.GetComponent(data.componentIndex);
        ObjectDestructiblePatches.IgnorePatches = true;
        PooleeDespawnPatch.IgnorePatch = true;

        objectDestructible._hits = objectDestructible.reqHitCount + 1;
        objectDestructible.TakeDamage(Vector3Extensions.up, objectDestructible._health + 1f, false, AttackType.Blunt);

        ObjectDestructiblePatches.IgnorePatches = false;
        PooleeDespawnPatch.IgnorePatch = false;
    }
}