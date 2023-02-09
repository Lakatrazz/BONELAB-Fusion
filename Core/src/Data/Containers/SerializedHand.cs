using LabFusion.Network;
using SLZ.Interaction;
using SLZ.Marrow.Input;
using SLZ.Rig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Data
{
    public class SerializedHand : IFusionSerializable {
        public const int Size = sizeof(float) * 3 + sizeof(byte) * 12;

        public float forceMultiplier;

        public float indexCurl;
        public float middleCurl;
        public float ringCurl;
        public float pinkyCurl;
        public float thumbCurl;

        public float solvedGrip;
        public float primaryAxis;

        public XRControllerType controllerType;
        public BaseController.GesturePose gesturePose;
        public float gesturePoseIntensity;

        public bool primaryInteractionButton;
        public bool secondaryInteractionButton;

        public Vector2 thumbstickAxis;

        public const float PRECISION_MULTIPLIER = 255f;

        public SerializedHand() { }

        public SerializedHand(Hand hand, BaseController controller)
        {
            forceMultiplier = hand.physHand.forceMultiplier;

            indexCurl = controller._processedIndex;
            middleCurl = controller._processedMiddle;
            ringCurl = controller._processedRing;
            pinkyCurl = controller._processedPinky;
            thumbCurl = controller._processedThumb;

            solvedGrip = controller._solvedGrip;
            primaryAxis = controller._primaryAxis;

            controllerType = controller.Type;
            gesturePose = controller._gesturePose;
            gesturePoseIntensity = controller._gesturePoseIntensity;

            primaryInteractionButton = controller._primaryInteractionButton;
            secondaryInteractionButton = controller._secondaryInteractionButton;

            thumbstickAxis = controller._thumbstickAxis;
        }

        public void CopyTo(Hand hand, BaseController controller) {
            hand.physHand.forceMultiplier = forceMultiplier;

            controller._processedIndex = indexCurl;
            controller._processedMiddle = middleCurl;
            controller._processedRing = ringCurl;
            controller._processedPinky = pinkyCurl;
            controller._processedThumb = thumbCurl;

            controller._solvedGrip = Mathf.Clamp(solvedGrip, 0f, OpenController.grabThreshold - 0.01f);
            controller._primaryAxis = primaryAxis;

            controller.Type = controllerType;
            controller._gesturePose = gesturePose;
            controller._gesturePoseIntensity = gesturePoseIntensity;

            // Primary interaction button
            bool primaryUp = controller._primaryInteractionButtonUp;
            bool primaryDown = controller._primaryInteractionButtonDown;

            SolveButtonPress(controller._primaryInteractionButton, primaryInteractionButton, ref primaryUp, ref primaryDown);

            controller._primaryInteractionButtonUp = primaryUp;
            controller._primaryInteractionButtonDown = primaryDown;

            controller._primaryInteractionButton = primaryInteractionButton;

            // Secondary interaction button
            bool secondaryUp = controller._secondaryInteractionButtonUp;
            bool secondaryDown = controller._secondaryInteractionButtonDown;

            SolveButtonPress(controller._secondaryInteractionButton, secondaryInteractionButton, ref secondaryUp, ref secondaryDown);

            controller._secondaryInteractionButtonUp = secondaryUp;
            controller._secondaryInteractionButtonDown = secondaryDown;

            controller._secondaryInteractionButton = secondaryInteractionButton;

            // Thumbstick
            controller._thumbstickAxis = thumbstickAxis;

            // Update hovering so that grips solve properly
            controller._lastTimeGrabbed = Time.realtimeSinceStartup;
        }

        public void SolveButtonPress(bool lastValue, bool newValue, ref bool up, ref bool down)
        {
            if (newValue) {
                up = false;

                if (!lastValue)
                    down = true;
                else
                    down = false;
            }
            else {
                down = false;

                if (lastValue)
                    up = true;
                else
                    up = false;
            }
        }

        public void Serialize(FusionWriter writer) {
            writer.Write(forceMultiplier);

            writer.Write((byte)(indexCurl * PRECISION_MULTIPLIER));
            writer.Write((byte)(middleCurl * PRECISION_MULTIPLIER));
            writer.Write((byte)(ringCurl * PRECISION_MULTIPLIER));
            writer.Write((byte)(pinkyCurl * PRECISION_MULTIPLIER));
            writer.Write((byte)(thumbCurl * PRECISION_MULTIPLIER));

            writer.Write((byte)(solvedGrip * PRECISION_MULTIPLIER));
            writer.Write((byte)(primaryAxis * PRECISION_MULTIPLIER));

            writer.Write((byte)controllerType);
            writer.Write((byte)gesturePose);
            writer.Write((byte)(gesturePoseIntensity * PRECISION_MULTIPLIER));

            writer.Write(primaryInteractionButton);
            writer.Write(secondaryInteractionButton);

            writer.Write(thumbstickAxis);
        }

        public void Deserialize(FusionReader reader) {
            forceMultiplier = reader.ReadSingle();

            indexCurl = ReadCompressedFloat(reader);
            middleCurl = ReadCompressedFloat(reader);
            ringCurl = ReadCompressedFloat(reader);
            pinkyCurl = ReadCompressedFloat(reader);
            thumbCurl = ReadCompressedFloat(reader);

            solvedGrip = ReadCompressedFloat(reader);
            primaryAxis = ReadCompressedFloat(reader);

            controllerType = (XRControllerType)reader.ReadByte();
            gesturePose = (BaseController.GesturePose)reader.ReadByte();
            gesturePoseIntensity = ReadCompressedFloat(reader);

            primaryInteractionButton = reader.ReadBoolean();
            secondaryInteractionButton = reader.ReadBoolean();

            thumbstickAxis = reader.ReadVector2();
        }

        private float ReadCompressedFloat(FusionReader reader) {
            return ((float)reader.ReadByte()) / PRECISION_MULTIPLIER;
        }
    }
}
