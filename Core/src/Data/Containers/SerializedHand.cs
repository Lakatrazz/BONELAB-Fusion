using LabFusion.Network;
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

        public const float PRECISION_MULTIPLIER = 255f;

        public SerializedHand() { }

        public SerializedHand(BaseController controller)
        {
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
        }

        public void CopyTo(BaseController controller) {
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

            // Down/up button logic (so we don't need to send the value)
            if (primaryInteractionButton) {
                controller._primaryInteractionButtonUp = false;

                if (!controller._primaryInteractionButton)
                    controller._primaryInteractionButtonDown = true;
                else
                    controller._primaryInteractionButtonDown = false;
            }
            else {
                controller._primaryInteractionButtonDown = false;

                if (controller._primaryInteractionButton)
                    controller._primaryInteractionButtonUp = true;
                else
                    controller._primaryInteractionButtonUp = false;
            }

            controller._primaryInteractionButton = primaryInteractionButton;
        }

        public void Serialize(FusionWriter writer) {
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
        }

        public void Deserialize(FusionReader reader) {
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
        }

        private float ReadCompressedFloat(FusionReader reader) {
            return ((float)reader.ReadByte()) / PRECISION_MULTIPLIER;
        }
    }
}
