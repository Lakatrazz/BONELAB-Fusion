using LabFusion.Marrow.Patching;
using LabFusion.Marrow.Extenders;
using LabFusion.Extensions;
using LabFusion.Entities;
using LabFusion.Utilities;
using LabFusion.SDK.Modules;
using LabFusion.Network;

namespace LabFusion.Marrow.Messages;

[Net.SkipHandleWhileLoading]
public class ObjectDestructibleDestroyMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ComponentIndexData>();

        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.EntityID);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<ObjectDestructibleExtender>();

        if (extender == null)
        {
            return;
        }

        var objectDestructible = extender.GetComponent(data.ComponentIndex);

        ObjectDestructiblePatches.IgnorePatches = true;

        try
        {
            objectDestructible._hits = objectDestructible.reqHitCount + 1;
            objectDestructible.TakeDamage(Vector3Extensions.up, objectDestructible._health + 1f, false);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("destroying ObjectDestructible", e);
        }

        ObjectDestructiblePatches.IgnorePatches = false;
    }
}