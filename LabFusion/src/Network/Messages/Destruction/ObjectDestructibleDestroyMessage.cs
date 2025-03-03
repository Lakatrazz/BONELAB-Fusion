using LabFusion.Patching;
using LabFusion.Extensions;
using LabFusion.Entities;
using LabFusion.Utilities;

namespace LabFusion.Network;

[Net.DelayWhileTargetLoading]
public class ObjectDestructibleDestroyMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ObjectDestructibleDestroy;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ComponentIndexData>();

        NetworkEntityManager.HookEntityRegistered(data.EntityId, OnDestructibleRegistered);

        void OnDestructibleRegistered(NetworkEntity entity)
        {
            var extender = entity.GetExtender<ObjectDestructibleExtender>();

            if (extender == null)
            {
                return;
            }

            var objectDestructible = extender.GetComponent(data.ComponentIndex);
            ObjectDestructiblePatches.IgnorePatches = true;
            PooleeDespawnPatch.IgnorePatch = true;

            try
            {
                objectDestructible._bloodied = true;
                objectDestructible._hits = objectDestructible.reqHitCount + 1;
                objectDestructible.TakeDamage(Vector3Extensions.up, float.PositiveInfinity, false);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("destroying ObjectDestructible", e);
            }

            ObjectDestructiblePatches.IgnorePatches = false;
            PooleeDespawnPatch.IgnorePatch = false;
        }
    }
}