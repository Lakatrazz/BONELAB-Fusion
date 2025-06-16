using LabFusion.Utilities;
using LabFusion.Network;
using LabFusion.Entities;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Pool;

namespace LabFusion.Marrow.Extenders;

public class FlyingGunExtender : EntityComponentExtender<FlyingGun>
{
    public static readonly FusionComponentCache<FlyingGun, NetworkEntity> Cache = new();

    private EntityCleaner _cleaner = null;

    private Grip.HandDelegate _onAttachDelegate = null;

    private Poolee _poolee = null;

    protected override void OnRegister(NetworkEntity entity, FlyingGun component)
    {
        Cache.Add(component, entity);

        RegisterCleaner();

        if (NetworkInfo.IsHost)
        {
            _poolee = component._host.marrowEntity._poolee;

            _onAttachDelegate = (Grip.HandDelegate)((hand) => { OnAttach(hand); });

            component.triggerGrip.attachedHandDelegate += _onAttachDelegate;
        }
    }

    protected override void OnUnregister(NetworkEntity entity, FlyingGun component)
    {
        Cache.Remove(component);

        UnregisterCleaner();

        if (_onAttachDelegate != null)
        {
            component.triggerGrip.attachedHandDelegate -= _onAttachDelegate;

            _onAttachDelegate = null;
            _poolee = null;
        }
    }

    private void RegisterCleaner()
    {
        _cleaner = new();
        _cleaner.Register(NetworkEntity, Component._host, Component._host.marrowEntity._poolee);
    }

    private void UnregisterCleaner()
    {
        if (_cleaner != null)
        {
            _cleaner.Unregister();
            _cleaner = null;
        }
    }

    private void OnAttach(Hand hand)
    {
        if (_poolee == null)
        {
            return;
        }

        var manager = hand.manager;

        if (!NetworkPlayerManager.TryGetPlayer(manager, out var player))
        {
            return;
        }

        if (FusionDevTools.DespawnDevTool(player.PlayerID))
        {
            _poolee.Despawn();
        }
    }
}