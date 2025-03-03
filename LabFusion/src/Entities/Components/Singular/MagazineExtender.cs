using LabFusion.Utilities;
using LabFusion.Network;

using Il2CppSLZ.Marrow;

namespace LabFusion.Entities;

public class MagazineExtender : EntityComponentExtender<Magazine>
{
    public static FusionComponentCache<Magazine, NetworkEntity> Cache = new();

    private TimedDespawnHandler _despawnHandler = null;

    protected override void OnRegister(NetworkEntity entity, Magazine component)
    {
        Cache.Add(component, entity);

        if (NetworkInfo.IsServer)
        {
            _despawnHandler = new();
            _despawnHandler.Register(component.interactableHost, component._poolee);
        }
    }

    protected override void OnUnregister(NetworkEntity entity, Magazine component)
    {
        Cache.Remove(component);

        if (_despawnHandler != null)
        {
            _despawnHandler.Unregister();
            _despawnHandler = null;
        }
    }
}