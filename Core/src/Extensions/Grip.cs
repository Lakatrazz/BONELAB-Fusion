using LabFusion.Data;
using LabFusion.Patching;

using SLZ.Interaction;
using SLZ.Marrow.Utilities;
using UnityEngine;

namespace LabFusion.Extensions
{
    public static class GripExtensions {
        public static SerializedTransform GetRelativeHand(this GripPair pair) {
            var handTransform = pair.hand.transform;
            var gripTransform = pair.grip.Host.GetTransform();

            return new SerializedTransform(gripTransform.InverseTransformPoint(handTransform.position), gripTransform.InverseTransformRotation(handTransform.rotation));
        }

        public static void SetRelativeHand(this Grip grip, Hand hand, SerializedTransform transform) {
            // Set the hand position so that the grip is created in the right spot
            if (transform != null) {
                var gripTransform = grip.Host.GetTransform();

                hand.transform.SetPositionAndRotation(gripTransform.TransformPoint(transform.position), gripTransform.TransformRotation(transform.rotation.Expand()));
            }
        }

        public static void PrepareGrab(this Grip grip, Hand hand) {
            hand.HoveringReceiver = grip;

            SimpleTransform transform = SimpleTransform.Create(hand.transform);

            // ValidateGripScore usually calculates targets on a grip
            // I originally also had OnHandHoverUpdate here, but that checks for grabbing (I think) and can cause players to turn into spaghetti!
            for (var i = 0; i < 20; i++) {
                grip.ValidateGripScore(hand, transform);
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

        public static void TryAttach(this Grip grip, Hand hand, bool isInstant = false) {
            // Detach an existing grip
            hand.TryDetach();

            // Confirm the grab
            hand.GrabLock = false;

            var inventoryHand = InventoryHand.Cache.Get(hand.gameObject);
            if (inventoryHand) {
                inventoryHand.IgnoreUnlock();
            }

            // Prepare the grip
            grip.PrepareGrab(hand);
            
            // Begin grabbing
            grip.OnGrabConfirm(hand, isInstant);
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
