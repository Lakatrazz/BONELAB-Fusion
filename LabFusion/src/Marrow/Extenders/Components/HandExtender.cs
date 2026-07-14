using Il2CppSLZ.Marrow;

using LabFusion.Entities;

namespace LabFusion.Marrow.Extenders;

public class HandExtender : EntityComponentArrayExtender<Hand>
{
    public IMarrowEntityExtender MarrowEntityExtender { get; set; } = null;

    protected override void OnRegister(NetworkEntity entity, Hand[] components)
    {
        MarrowEntityExtender = entity.GetExtender<IMarrowEntityExtender>();

        if (MarrowEntityExtender != null)
        {
            MarrowEntityExtender.OnAfterTeleportToPose += OnAfterTeleportToPose;
        }
    }

    protected override void OnUnregister(NetworkEntity entity, Hand[] components)
    {
        if (MarrowEntityExtender != null)
        {
            MarrowEntityExtender.OnAfterTeleportToPose -= OnAfterTeleportToPose;
            MarrowEntityExtender = null;
        }
    }

    private void OnAfterTeleportToPose()
    {
        foreach (var hand in Components)
        {
            TeleportHandToPose(hand);
        }
    }

    private static void TeleportHandToPose(Hand hand)
    {
        var attachedReceiver = hand.AttachedReceiver;

        if (attachedReceiver == null)
        {
            return;
        }

        var grip = attachedReceiver.TryCast<Grip>();

        if (grip == null)
        {
            return;
        }

        var gripNetworkEntity = GripExtender.Cache.Get(grip);

        if (gripNetworkEntity == null)
        {
            return;
        }

        if (gripNetworkEntity.IsOwner)
        {
            return;
        }

        var gripMarrowEntityExtender = gripNetworkEntity.GetExtender<IMarrowEntityExtender>();

        if (gripMarrowEntityExtender == null)
        {
            return;
        }

        gripMarrowEntityExtender.TeleportToPoseWithoutNotify();
    }
}
