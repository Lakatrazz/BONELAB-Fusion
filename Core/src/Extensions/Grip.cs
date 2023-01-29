using LabFusion.Patching;
using SLZ.Interaction;
using SLZ.Marrow.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Extensions
{
    public static class GripExtensions {
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

        public static void TryAttach(this Grip grip, Hand hand) {
            // Detach an existing grip
            if (hand.m_CurrentAttachedGO != null) {
                var other = Grip.Cache.Get(hand.m_CurrentAttachedGO);

                if (other != null)
                    other.TryDetach(hand);
            }

            // Confirm the grab
            hand.GrabLock = false;

            var inventoryHand = InventoryHand.Cache.Get(hand.gameObject);
            if (inventoryHand) {
                inventoryHand.IgnoreUnlock();
            }

            grip.OnGrabConfirm(hand, true);
        }

        public static void TryDetach(this Grip grip, Hand hand) {
            // Make sure the hand is attached to this grip
            if (hand.m_CurrentAttachedGO == grip.gameObject || grip._handStates.ContainsKey(hand) || grip.attachedHands.Has(hand)) {
                grip.ForceDetach(hand);
            }
        }

        public static void FreeJoints(this Grip grip, Hand hand) {
            if (hand.joint)
                Internal_FreeJoint(hand.joint);

            // Check cylinder grip
            var cylinder = grip.TryCast<CylinderGrip>();
            if (cylinder) {
                foreach (var pair in cylinder._constraintJoints) {
                    if (pair.key == hand) {
                        Internal_FreeJoint(pair.value._joint);
                    }
                }
            }
        }

        internal static void Internal_FreeJoint(ConfigurableJoint joint) {
            if (!joint)
                return;

            joint.xDrive = joint.yDrive = joint.zDrive = joint.angularXDrive = joint.angularYZDrive = joint.slerpDrive = default;
            joint.xMotion = joint.yMotion = joint.zMotion =
                joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Free;
        }
    }
}
