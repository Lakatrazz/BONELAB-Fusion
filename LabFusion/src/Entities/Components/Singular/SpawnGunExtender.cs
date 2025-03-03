using LabFusion.Utilities;
using LabFusion.Network;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow;

namespace LabFusion.Entities;

public class SpawnGunExtender : EntityComponentExtender<SpawnGun>
{
    public static readonly FusionComponentCache<SpawnGun, NetworkEntity> Cache = new();

    private TimedDespawnHandler _despawnHandler = null;

    private Grip.HandDelegate _onAttachDelegate = null;

    private Poolee _poolee = null;


    protected override void OnRegister(NetworkEntity entity, SpawnGun component)
    {
        Cache.Add(component, entity);

        if (NetworkInfo.IsServer)
        {
            _despawnHandler = new();
            _despawnHandler.Register(component.host, component._poolee);

            _poolee = component._poolee;

            _onAttachDelegate = (Grip.HandDelegate)((hand) => { OnAttach(hand); });

            component.triggerGrip.attachedHandDelegate += _onAttachDelegate;
        }
    }

    protected override void OnUnregister(NetworkEntity entity, SpawnGun component)
    {
        Cache.Remove(component);

        if (_despawnHandler != null)
        {
            _despawnHandler.Unregister();
            _despawnHandler = null;
        }

        if (_onAttachDelegate != null)
        {
            component.triggerGrip.attachedHandDelegate -= _onAttachDelegate;

            _onAttachDelegate = null;
            _poolee = null;
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

        if (FusionDevTools.DespawnDevTool(player.PlayerId))
        {
            _poolee.Despawn();
        }
    }
}