using LabFusion.Network;
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

        public const float PRECISION_MULTIPLIER = 255f;

        public SerializedHand() { }

        public SerializedHand(BaseController controller)
        {
            indexCurl = controller._processedIndex;
            middleCurl = controller._processedMiddle;
            ringCurl = controller._processedRing;
            pinkyCurl = controller._processedPinky;
            thumbCurl = controller._processedThumb;
        }

        public void CopyTo(BaseController controller) {
            controller._processedIndex = indexCurl;
            controller._processedMiddle = middleCurl;
            controller._processedRing = ringCurl;
            controller._processedPinky = pinkyCurl;
            controller._processedThumb = thumbCurl;
        }

        public void Serialize(FusionWriter writer) {
            writer.Write((byte)(indexCurl * PRECISION_MULTIPLIER));
            writer.Write((byte)(middleCurl * PRECISION_MULTIPLIER));
            writer.Write((byte)(ringCurl * PRECISION_MULTIPLIER));
            writer.Write((byte)(pinkyCurl * PRECISION_MULTIPLIER));
            writer.Write((byte)(thumbCurl * PRECISION_MULTIPLIER));
        }

        public void Deserialize(FusionReader reader) {
            indexCurl = ((float)reader.ReadByte()) / PRECISION_MULTIPLIER;
            middleCurl = ((float)reader.ReadByte()) / PRECISION_MULTIPLIER;
            ringCurl = ((float)reader.ReadByte()) / PRECISION_MULTIPLIER;
            pinkyCurl = ((float)reader.ReadByte()) / PRECISION_MULTIPLIER;
            thumbCurl = ((float)reader.ReadByte()) / PRECISION_MULTIPLIER;
        }
    }
}
