using LabFusion.Extensions;
using LabFusion.Network;

using SLZ;
using SLZ.Interaction;
using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data
{
    public class SerializedGripAnchor : IFusionSerializable
    {
        public Handedness handedness;
        public Vector3 anchor;
        public Vector3 connectedAnchor;
        public SerializedQuaternion relativeRotation;
        public SerializedQuaternion jointStartRotation;

        public bool isZFlipped;
        public float posInFlippedBase;
        public float rotInFlippedBase;
        public SerializedQuaternion targetRotationInBase;
        public SerializedQuaternion amplifyRotationInBase;

        public SerializedGripAnchor() { }

        public SerializedGripAnchor(Hand hand, Grip grip) {
            handedness = hand.handedness;

            if (hand.joint) {
                Vector3 origin = hand.transform.position;

                anchor = hand.joint.GetLocalAnchor(origin);
                connectedAnchor = hand.joint.GetLocalConnectedAnchor(origin);
                relativeRotation = SerializedQuaternion.Compress(Quaternion.Inverse(grip.transform.rotation) * hand.transform.rotation);
                jointStartRotation = SerializedQuaternion.Compress(hand.jointStartRotation);

                var state = grip.GetHandState(hand);
                isZFlipped = state.isZFlipped;
                posInFlippedBase = state.posInFlippedBase;
                rotInFlippedBase = state.rotInFlippedBase;
                targetRotationInBase = SerializedQuaternion.Compress(state.targetRotationInBase);
                amplifyRotationInBase = SerializedQuaternion.Compress(state.amplifyRotationInBase);
            }
        }

        public void CopyTo(Hand hand, Grip grip) {
            if (hand.joint) {
                var joint = hand.joint;

                hand.jointStartRotation = jointStartRotation.Expand();

                // Update hand state
                var handState = grip.GetHandState(hand);

                handState.isZFlipped = isZFlipped;
                handState.posInFlippedBase = posInFlippedBase;
                handState.rotInFlippedBase = rotInFlippedBase;
                handState.targetRotationInBase = targetRotationInBase.Expand();
                handState.amplifyRotationInBase = amplifyRotationInBase.Expand();
                
                // Pos anchors
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = anchor;
                joint.connectedAnchor = connectedAnchor;

                // Rot anchors
                var initialRot = hand.transform.rotation;
                hand.transform.rotation = grip.transform.rotation * relativeRotation.Expand();

                joint.swapBodies = !joint.swapBodies;
                joint.swapBodies = !joint.swapBodies;

                hand.transform.rotation = initialRot;

                // Update joint targets
                hand.joint.targetPosition = Vector3.zero;
                hand.joint.targetVelocity = Vector3.zero;

                // Update joint drives
                var drive = new JointDrive() { positionSpring = 50000000f, maximumForce = 50000000f };

                var limit = new SoftJointLimit();

                joint.linearLimit = limit;
                joint.xDrive = joint.yDrive = joint.zDrive = drive;

                // Rotation drives
                joint.lowAngularXLimit = joint.highAngularXLimit = joint.angularYLimit = joint.angularZLimit = limit;
                joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Limited;
            }
        }

        public void Serialize(FusionWriter writer) {
            writer.Write((byte)handedness);
            writer.Write(anchor);
            writer.Write(connectedAnchor);
            writer.Write(relativeRotation);
            writer.Write(jointStartRotation);

            writer.Write(isZFlipped);
            writer.Write(posInFlippedBase);
            writer.Write(rotInFlippedBase);
            writer.Write(targetRotationInBase);
            writer.Write(amplifyRotationInBase);
        }

        public void Deserialize(FusionReader reader) {
            handedness = (Handedness)reader.ReadByte();
            anchor = reader.ReadVector3();
            connectedAnchor = reader.ReadVector3();
            relativeRotation = reader.ReadFusionSerializable<SerializedQuaternion>();
            jointStartRotation = reader.ReadFusionSerializable<SerializedQuaternion>();

            isZFlipped = reader.ReadBoolean();
            posInFlippedBase = reader.ReadSingle();
            rotInFlippedBase = reader.ReadSingle();
            targetRotationInBase = reader.ReadFusionSerializable<SerializedQuaternion>();
            amplifyRotationInBase = reader.ReadFusionSerializable<SerializedQuaternion>();
        }
    }
}
