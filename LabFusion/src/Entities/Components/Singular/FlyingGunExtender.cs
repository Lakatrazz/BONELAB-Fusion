﻿using LabFusion.Utilities;
using LabFusion.Network;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Pool;

namespace LabFusion.Entities;

public class FlyingGunExtender : EntityComponentExtender<FlyingGun>
{
    public static readonly FusionComponentCache<FlyingGun, NetworkEntity> Cache = new();

    private TimedDespawnHandler _despawnHandler = null;

    private Grip.HandDelegate _onAttachDelegate = null;

    private Poolee _poolee = null;

    protected override void OnRegister(NetworkEntity networkEntity, FlyingGun component)
    {
        Cache.Add(component, networkEntity);

        if (NetworkInfo.IsServer)
        {
            _despawnHandler = new();
            _despawnHandler.Register(component._host, component._host.marrowEntity._poolee);

            _poolee = component._host.marrowEntity._poolee;

            _onAttachDelegate = (Grip.HandDelegate)((hand) => { OnAttach(hand); });

            component.triggerGrip.attachedHandDelegate += _onAttachDelegate;
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