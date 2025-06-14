using Il2CppSLZ.Marrow;

using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Grabbables;

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

        entity.OnEntityDataCatchup += OnEntityDataCatchup;
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

        entity.OnEntityDataCatchup -= OnEntityDataCatchup;
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
        var localPlayer = LocalPlayer.GetNetworkPlayer();

        if (localPlayer == null)
        {
            return;
        }

        foreach (var hand in grip.attachedHands)
        {
            if (hand == null)
            {
                continue;
            }

            if (hand.manager.IsLocalPlayer())
            {
                localPlayer.NetworkEntity.HookOnDataCatchup(player, (playerEntity, playerPlayer) =>
                {
                    if (hand.AttachedReceiver != grip)
                    {
                        return;
                    }

                    GrabHelper.SendObjectAttach(hand, grip, player);
                });
            }
        }
    }
}