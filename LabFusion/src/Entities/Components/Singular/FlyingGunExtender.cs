using LabFusion.Utilities;
using LabFusion.Network;

using Il2CppSLZ.Marrow;

namespace LabFusion.Entities;

public class FlyingGunExtender : EntityComponentExtender<FlyingGun>
{
    public static readonly FusionComponentCache<FlyingGun, NetworkEntity> Cache = new();

    private TimedDespawnHandler _despawnHandler = null;

    protected override void OnRegister(NetworkEntity networkEntity, FlyingGun component)
    {
        Cache.Add(component, networkEntity);

        if (NetworkInfo.IsServer)
        {
            _despawnHandler = new();
            _despawnHandler.Register(component._host, component._host.marrowEntity._poolee);
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, FlyingGun component)
    {
        Cache.Remove(component);

        if (_despawnHandler != null)
        {
            _despawnHandler.Unregister();
            _despawnHandler = null;
        }
    }
}