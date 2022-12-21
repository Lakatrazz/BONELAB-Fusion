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
        }

        private float ReadCompressedFloat(FusionReader reader) {
            return ((float)reader.ReadByte()) / PRECISION_MULTIPLIER;
        }
    }
}
