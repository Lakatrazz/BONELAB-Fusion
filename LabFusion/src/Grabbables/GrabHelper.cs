using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Entities;
using LabFusion.Patching;
using LabFusion.Marrow.Extenders;
using LabFusion.Marrow.Messages;

using Il2CppSLZ.Marrow;
using LabFusion.Marrow.Interaction;

namespace LabFusion.Grabbables;

public static class GrabHelper
{
    public static void SendObjectForcePull(Hand hand, Grip grip)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }
        
        Internal_ObjectForcePull(hand, grip);
    }

    internal static void Internal_ObjectForcePull(Hand hand, Grip grip)
    {
        // Make sure we have a server running
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // Make sure this grip has a marrow entity
        var marrowEntity = grip._marrowEntity;

        if (marrowEntity == null)
        {
            return;
        }

        // Make sure this grip does NOT have a RigManager attached
        if (grip.GetComponentInParent<RigManager>())
        {
            return;
        }

        // Get base values for the message
        byte smallId = PlayerIDManager.LocalSmallID;

        // Do we already have a synced object?
        if (GripExtender.Cache.TryGet(grip, out var entity))
        {
            // Make sure to wait for the entity to be registered
            entity.HookOnRegistered((entity) =>
            {
                NetworkEntityManager.TakeOwnership(entity);
            });
        }
        // Create a new one
        else
        {
            PropSender.SendPropCreation(marrowEntity, null, false);
        }
    }

    public static void SendObjectAttach(Hand hand, Grip grip, PlayerID target = null)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        Internal_ObjectAttach(hand, grip, target);
    }

    internal static void Internal_ObjectAttach(Hand hand, Grip grip, PlayerID target = null)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var handedness = hand.handedness;

        // Get base values for the message
        byte smallId = PlayerIDManager.LocalSmallID;

        // If the grip exists, we'll check its stuff
        if (grip == null)
        {
            return;
        }

        // Check for entity grips
        if (grip.HasRigidbody)
        {
            var marrowEntity = grip._marrowEntity;

            // It SHOULD always have a marrow entity, but just in case
            if (marrowEntity == null)
            {
                return;
            }

            if (GripExtender.Cache.TryGet(grip, out var entity))
            {
                entity.HookOnRegistered(OnEntityRegistered);
            }
            else
            {
                PropSender.SendPropCreation(marrowEntity, OnEntityRegistered);
            }

            void OnEntityRegistered(NetworkEntity networkEntity)
            {
                TrySendGrab(hand, grip);
            }
        }

        void TrySendGrab(Hand hand, Grip grip)
        {
            if (hand.AttachedReceiver != grip)
            {
                return;
            }

            var data = new RigGrabData()
            {
                RigReference = new(smallId),
                Grab = SerializedGrab.CreateFromHandGripPair(hand, grip),
            };

            var route = target != null ? new MessageRoute(target.SmallID, NetworkChannel.Reliable) : CommonMessageRoutes.ReliableToOtherClients;

            MessageRelay.RelayModule<RigGrabMessage, RigGrabData>(data, route);
        }
    }

    public static void SendObjectDetach(Hand hand)
    {
        Internal_ObjectDetach(hand);
    }

    internal static void Internal_ObjectDetach(Hand hand)
    {
        var handedness = hand.handedness;

        if (hand.m_CurrentAttachedGO != null)
        {
            return;
        }

        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = new RigReleaseData()
        {
            RigReference = new(PlayerIDManager.LocalSmallID),
            Handedness = handedness,
        };

        MessageRelay.RelayModule<RigReleaseMessage, RigReleaseData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }
}
