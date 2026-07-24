using LabFusion.Utilities;

namespace LabFusion.Entities;

public class RigComponentManager
{
    public HashSet<IEntityComponentExtender> RegisteredComponentExtenders { get; private set; } = null;

    public HashSet<IEntityComponentExtender> DynamicComponentExtenders { get; private set; } = null;

    public void RegisterComponents(NetworkEntity networkEntity, RigRefs rigRefs)
    {
        var physicsRig = rigRefs.RigManager.physicsRig;

        var detacher = new TemporaryTransformDetacher();
        rigRefs.DetachSlottedTransforms(detacher);

        RegisteredComponentExtenders = EntityComponentManager.ApplyComponents(networkEntity, physicsRig.gameObject);

        detacher.ReattachTransforms();

        RegisterDynamicComponents(networkEntity, rigRefs);
    }

    public void RegisterDynamicComponents(NetworkEntity networkEntity, RigRefs rigRefs)
    {
        UnregisterDynamicComponents();

        var avatar = rigRefs.RigManager.avatar;

        DynamicComponentExtenders = EntityComponentManager.ApplyDynamicComponents(networkEntity, avatar.gameObject);
    }

    public void UnregisterComponents()
    {
        UnregisterDynamicComponents();

        if (RegisteredComponentExtenders != null)
        {
            foreach (var extender in RegisteredComponentExtenders)
            {
                extender.Unregister();
            }

            RegisteredComponentExtenders.Clear();
        }
    }

    public void UnregisterDynamicComponents()
    {
        if (RegisteredComponentExtenders != null)
        {
            foreach (var extender in RegisteredComponentExtenders)
            {
                extender.UnregisterDynamics();
            }
        }

        if (DynamicComponentExtenders != null)
        {
            foreach (var extender in DynamicComponentExtenders)
            {
                extender.Unregister();
            }

            DynamicComponentExtenders.Clear();
        }
    }
}
