using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ;
using SLZ.Interaction;

namespace LabFusion.Data {
    public abstract class SerializedGrab : IFusionSerializable {
        public bool isGrabbed;

        public virtual void Serialize(FusionWriter writer) {
            writer.Write(isGrabbed);
        }

        public virtual void Deserialize(FusionReader reader) {
            isGrabbed = reader.ReadBoolean();
        }

        public abstract Grip GetGrip();

        public virtual void RequestGrab(PlayerRep rep, Handedness handedness, Grip grip) {
            // Don't do anything if this isn't grabbed anymore
            if (!isGrabbed)
                return;

            rep.AttachObject(handedness, grip);
        }
    }
}
