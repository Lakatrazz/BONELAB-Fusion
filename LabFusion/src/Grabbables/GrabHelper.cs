using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Entities;
using LabFusion.Patching;
using LabFusion.Marrow.Extenders;

using Il2CppSLZ.Marrow;

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
        GrabGroup group = GrabGroup.UNKNOWN;
        SerializedGrab serializedGrab = null;

        // If the grip exists, we'll check its stuff
        if (grip == null)
        {
            return;
        }

        // Check for static grips
        if (grip.IsStatic)
        {
            if (grip.TryCast<WorldGrip>() != null)
            {
                group = GrabGroup.WORLD;
                serializedGrab = new SerializedWorldGrab(smallId);
                OnFinish();
            }
            else
            {
                group = GrabGroup.STATIC;

                var gripHash = GripPatches.HashTable.GetDataFromComponent(grip);

                if (gripHash == null)
                {
                    return;
                }

                serializedGrab = new SerializedStaticGrab(gripHash);
                OnFinish();
            }
        }
        // Check for entity grips
        else if (grip.HasRigidbody)
        {
            group = GrabGroup.ENTITY;
            var marrowEntity = grip._marrowEntity;

            // It SHOULD always have a marrow entity, but just in case
            if (marrowEntity == null)
            {
                return;
            }

            // Do we already have a synced object?
            if (GripExtender.Cache.TryGet(grip, out var entity))
            {
                // Make sure to only run after the entity is registered
                entity.HookOnRegistered((entity) =>
                {
                    var gripExtender = entity.GetExtender<GripExtender>();

                    serializedGrab = new SerializedEntityGrab(gripExtender.GetIndex(grip).Value, entity.ID);
                    OnFinish();
                });
            }
            else
            {
                // Invoked when the NetworkProp is finished being created
                void OnEntityFinish(NetworkProp prop)
                {
                    var gripExtender = prop.NetworkEntity.GetExtender<GripExtender>();
                    serializedGrab = new SerializedEntityGrab(gripExtender.GetIndex(grip).Value, prop.NetworkEntity.ID);

                    OnFinish();
                }

                // Send the marrow entity to be registered as a NetworkProp
                PropSender.SendPropCreation(marrowEntity, (networkEntity) =>
                {
                    NetworkProp prop = networkEntity.GetExtender<NetworkProp>();

                    if (prop == null)
                    {
                        return;
                    }

                    OnEntityFinish(prop);
                });
            }
        }

        // Send the message when whatever task is finished
        void OnFinish()
        {
            // Write the default grip values
            serializedGrab.WriteDefaultGrip(hand, grip);

            var data = PlayerRepGrabData.Create(smallId, handedness, group, serializedGrab);

            if (target == null)
            {
                MessageRelay.RelayNative(data, NativeMessageTag.PlayerRepGrab, CommonMessageRoutes.ReliableToOtherClients);
            }
            else
            {
                MessageRelay.RelayNative(data, NativeMessageTag.PlayerRepGrab, new MessageRoute(target.SmallID, NetworkChannel.Reliable));
            }
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

        var data = PlayerRepReleaseData.Create(PlayerIDManager.LocalSmallID, handedness);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerRepRelease, CommonMessageRoutes.ReliableToOtherClients);
    }
}
