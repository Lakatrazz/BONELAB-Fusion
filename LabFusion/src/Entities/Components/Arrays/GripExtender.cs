using Il2CppSLZ.Interaction;
using Il2CppSLZ.Rig;

using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Entities;

public class GripExtender : EntityComponentArrayExtender<Grip>
{
    public static readonly FusionComponentCache<Grip, NetworkEntity> Cache = new();

    private Grip.HandDelegate _onAttachDelegate = null;
    private Grip.HandDelegate _onDetachDelegate = null;

    protected override void OnRegister(NetworkEntity networkEntity, Grip[] components)
    {
        _onAttachDelegate = (Grip.HandDelegate)((hand) => { OnAttach(hand); });
        _onDetachDelegate = (Grip.HandDelegate)((hand) => { OnDetach(hand); });

        foreach (var grip in components)
        {
            Cache.Add(grip, networkEntity);

            grip.attachedHandDelegate += _onAttachDelegate;
            grip.detachedHandDelegate += _onDetachDelegate;
        }
    }

    protected override void OnUnregister(NetworkEntity networkEntity, Grip[] components)
    {
        foreach (var grip in components)
        {
            Cache.Remove(grip);

            grip.attachedHandDelegate -= _onAttachDelegate;
            grip.detachedHandDelegate -= _onDetachDelegate;
        }

        _onAttachDelegate = null;
        _onDetachDelegate = null;
    }

    protected void OnAttach(Hand hand)
    {
        OnTransferOwner(hand);
    }

    protected void OnDetach(Hand hand)
    {
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
        if (hand.manager.IsSelf())
        {
            NetworkEntityManager.TakeOwnership(NetworkEntity);
        }
    }
}