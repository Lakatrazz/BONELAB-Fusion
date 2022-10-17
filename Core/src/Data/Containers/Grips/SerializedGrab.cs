using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using SLZ;
using SLZ.Interaction;

namespace LabFusion.Data {
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class SerializedGrabGroup : Attribute {
        public SyncUtilities.SyncGroup group;
    }


    public abstract class SerializedGrab : IFusionSerializable {
        public abstract void Serialize(FusionWriter writer);

        public abstract void Deserialize(FusionReader reader);

        public abstract Grip GetGrip();

        public virtual void RequestGrab(PlayerRep rep, Handedness handedness, Grip grip) {
            rep.AttachObject(handedness, grip);
        }
    }
}
