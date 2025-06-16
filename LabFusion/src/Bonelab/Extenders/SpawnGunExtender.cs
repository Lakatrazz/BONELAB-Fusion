using LabFusion.Utilities;
using LabFusion.Network;
using LabFusion.Entities;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow;

namespace LabFusion.Bonelab.Extenders;

public class SpawnGunExtender : EntityComponentExtender<SpawnGun>
{
    public static readonly FusionComponentCache<SpawnGun, NetworkEntity> Cache = new();

    private EntityCleaner _cleaner = null;

    private Grip.HandDelegate _onAttachDelegate = null;

    private Poolee _poolee = null;


    protected override void OnRegister(NetworkEntity entity, SpawnGun component)
    {
        Cache.Add(component, entity);

        RegisterCleaner();

        if (NetworkInfo.IsHost)
        {
            _poolee = component._poolee;

            _onAttachDelegate = (Grip.HandDelegate)((hand) => { OnAttach(hand); });

            component.triggerGrip.attachedHandDelegate += _onAttachDelegate;
        }
    }

    protected override void OnUnregister(NetworkEntity entity, SpawnGun component)
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
        _cleaner.Register(NetworkEntity, Component.host, Component._poolee);
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