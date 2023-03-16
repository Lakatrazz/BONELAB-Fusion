using Il2CppSystem.Reflection;
using LabFusion.Data;
using LabFusion.Patching;

using SLZ.Interaction;
using SLZ.Marrow.Utilities;
using SLZ.Props.Weapons;
using UnityEngine;

namespace LabFusion.Extensions
{
    public static class GripExtensions {
        public static void TryAutoHolster(this Grip grip, RigReferenceCollection collection) {
            if (!grip.HasHost)
                return;

            var host = grip.Host;
            var weaponSlot = WeaponSlot.Cache.Get(host.GetHostGameObject());

            if (!weaponSlot)
                return;

            for (var i = 0; i < collection.RigSlots.Length; i++) {
                var slot = collection.RigSlots[i];

                if (slot._slottedWeapon != null)
                    continue;

                if ((slot.slotType & weaponSlot.slotType) != 0) {
                    slot.OnHandDrop(host);
                    break;
                }
            }
        }

        public static SerializedTransform GetRelativeHand(this GripPair pair) {
            var handTransform = pair.hand.transform;
            var gripTransform = pair.grip.Host.GetTransform();

            return new SerializedTransform(gripTransform.InverseTransformPoint(handTransform.position), gripTransform.InverseTransformRotation(handTransform.rotation));
        }

        public static void SetRelativeHand(this Grip grip, Hand hand, SerializedTransform transform)
        {
            // Set the hand position so that the grip is created in the right spot
            if (transform != null)
            {
                var gripTransform = grip.Host.GetTransform();

                hand.transform.SetPositionAndRotation(gripTransform.TransformPoint(transform.position), gripTransform.TransformRotation(transform.rotation.Expand()));
            }
        }

        public static void MoveIntoHand(this Grip grip, Hand hand) {
            var host = grip.Host.GetTransform();
            var handTarget = grip.SolveHandTarget(hand);

            var localHost = handTarget.InverseTransform(SimpleTransform.Create(host.transform));
            var worldHost = SimpleTransform.Create(hand.transform).Transform(localHost);

            host.position = worldHost.position;
            host.rotation = worldHost.rotation;

            if (grip.HasRigidbody) {
                var rb = grip.Host.Rb;

                rb.velocity = hand.rb.velocity;
                rb.angularVelocity = hand.rb.angularVelocity;
            }
        }

        public static bool CheckInstantAttach(this Grip grip) {
            bool isInstant = true;

            if (grip.IsStatic || grip.TryCast<WorldGrip>() || grip.TryCast<GenericGrip>() || grip.TryCast<BarrelGrip>() || grip.TryCast<BoxGrip>())
                isInstant = false;

            return isInstant;
        }

        public static void TryAttach(this Grip grip, Hand hand, bool isInstant = false, SimpleTransform? targetInBase = null) {
            // Detach an existing grip
            hand.TryDetach();

            // Confirm the grab
            hand.GrabLock = false;

            var inventoryHand = InventoryHand.Cache.Get(hand.gameObject);
            if (inventoryHand) {
                inventoryHand.IgnoreUnlock();
            }

            // Prevent other hovers
            hand.HoverLock();

            // Start the hover
            SimpleTransform handTransform = SimpleTransform.Create(hand.transform);
            grip.ValidateGripScore(hand, handTransform);
            grip.OnHandHoverBegin(hand, true);
            grip.ValidateGripScore(hand, handTransform);

            // Modify the target grab point
            if (targetInBase.HasValue) {
                SetTargetInBase(grip, hand, targetInBase.Value.position, targetInBase.Value.rotation);
            }

            // Confirm the grab and end the hover
            hand._mHoveringReceiver = grip;
            grip.OnGrabConfirm(hand, isInstant);

            // Re-apply the target grab point
            if (targetInBase.HasValue) {
                SetTargetInBase(grip, hand, targetInBase.Value.position, targetInBase.Value.rotation);
            }
        }

        private static void SetTargetInBase(Grip grip, Hand hand, Vector3 position, Quaternion rotation) {
            grip.SetTargetInBase(hand, position, rotation);

            var handState = grip.GetHandState(hand);
            handState.amplifyRotationInBase = rotation;
            handState.targetRotationInBase = rotation;
        }

        public static void TryDetach(this Grip grip, Hand hand) {
            // Make sure the hand is attached to this grip
            if (hand.m_CurrentAttachedGO == grip.gameObject) {
                // Begin the initial detach
                grip.ForceDetach(hand);

                // Instant detach the hand in one frame
                grip.OnDetachedFromHand(hand);
            }
        }
    }
}
