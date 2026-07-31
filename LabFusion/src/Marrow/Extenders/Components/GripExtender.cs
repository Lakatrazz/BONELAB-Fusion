using Il2CppSLZ.Marrow;

using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.Player;

namespace LabFusion.Marrow.Extenders;

public class GripExtender : EntityComponentArrayExtender<Grip>
{
    public static readonly FusionComponentCache<Grip, NetworkEntity> Cache = new();

    private Grip.HandDelegate _onAttachDelegate = null;
    private Grip.HandDelegate _onDetachDelegate = null;

    protected override void OnRegister(NetworkEntity entity, Grip[] components)
    {
        _onAttachDelegate = (Grip.HandDelegate)((hand) => { OnAttach(hand); });
        _onDetachDelegate = (Grip.HandDelegate)((hand) => { OnDetach(hand); });

        foreach (var grip in components)
        {
            Cache.Add(grip, entity);

            grip.attachedHandDelegate += _onAttachDelegate;
            grip.detachedHandDelegate += _onDetachDelegate;
        }

        entity.EntityDataCatchingUp += OnEntityDataCatchup;
    }

    protected override void OnUnregister(NetworkEntity entity, Grip[] components)
    {
        foreach (var grip in components)
        {
            Cache.Remove(grip);

            grip.attachedHandDelegate -= _onAttachDelegate;
            grip.detachedHandDelegate -= _onDetachDelegate;
        }

        _onAttachDelegate = null;
        _onDetachDelegate = null;

        entity.EntityDataCatchingUp -= OnEntityDataCatchup;
    }

    protected void OnAttach(Hand hand)
    {
        OnTransferOwner(hand);
    }

    protected void OnDetach(Hand hand)
    {
        // Check if any other rigs are still holding this
        // If they are, we shouldn't take ownership on detach
        foreach (var grip in Components)
        {
            foreach (var grabbingHand in grip.attachedHands)
            {
                if (grabbingHand.manager != hand.manager)
                {
                    return;
                }
            }
        }

        OnTransferOwner(hand);
    }

    public bool IsHeldBy(RigManager rigManager)
    {
        foreach (var grip in Components)
        {
            foreach (var hand in grip.attachedHands)
            {
                if (hand.HasAttachedObject() && hand.manager == rigManager)
                    return true;
            }
        }

        return false;
    }

    public bool CheckHeld()
    {
        foreach (var grip in Components)
        {
            if (grip.attachedHands.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    public void OnTransferOwner(Hand hand)
    {
        // Check if the owner is locked
        if (NetworkEntity.IsOwnerLocked)
        {
            return;
        }

        // Determine the manager
        // Main player
        if (hand.manager.IsLocalPlayer())
        {
            NetworkEntityManager.TakeOwnership(NetworkEntity);
        }
    }

    private void OnEntityDataCatchup(NetworkEntity entity, PlayerID player)
    {
        foreach (var component in Components)
        {
            OnEntityDataCatchup(component, entity, player);
        }
    }

    private static void OnEntityDataCatchup(Grip grip, NetworkEntity entity, PlayerID player)
    {
        foreach (var hand in grip.attachedHands)
        {
            if (hand == null)
            {
                continue;
            }

            if (!NetworkRig.Cache.TryGet(hand.manager, out var networkRig))
            {
                continue;
            }

            var networkRigEntity = networkRig.NetworkEntity;

            if (!networkRigEntity.IsOwner)
            {
                continue;
            }

            networkRigEntity.HookOnDataCatchup(player, (playerEntity, playerPlayer) =>
            {
                if (hand.AttachedReceiver != grip)
                {
                    return;
                }

                networkRig.RigGrabber.TrySendGrab(hand, grip, player);
            });
        }
    }
}