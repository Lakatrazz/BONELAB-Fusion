using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Network;

using SLZ.Interaction;

namespace LabFusion.Data {
    public class SerializedPlayerBodyGrab : SerializedGrab {
        public byte gripIndex;

        public SerializedPlayerBodyGrab(byte gripIndex) {
            this.gripIndex = gripIndex;
        }

        public SerializedPlayerBodyGrab() { }

        public override void Serialize(FusionWriter writer) {
            writer.Write(gripIndex);
        }

        public override void Deserialize(FusionReader reader) {
            gripIndex = reader.ReadByte();
        }

        public override Grip GetGrip() {
            return RigData.RigReferences.GetGrip(gripIndex);
        }
    }
}
