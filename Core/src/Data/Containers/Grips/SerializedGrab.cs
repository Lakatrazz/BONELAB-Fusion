using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Network;

using SLZ.Interaction;

namespace LabFusion.Data {
    public abstract class SerializedGrab : IFusionSerializable {
        public abstract void Serialize(FusionWriter writer);

        public abstract void Deserialize(FusionReader reader);

        public abstract Grip GetGrip();
    }
}
