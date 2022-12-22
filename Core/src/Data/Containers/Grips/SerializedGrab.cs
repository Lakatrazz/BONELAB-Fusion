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
    public abstract class SerializedGrab : IFusionSerializable {
        public SerializedTransform relativeHand = null;

        public abstract void Serialize(FusionWriter writer);

        public abstract void Deserialize(FusionReader reader);

        public abstract Grip GetGrip();

        public virtual void RequestGrab(PlayerRep rep, Handedness handedness, Grip grip, bool useCustomJoint = true) {
            if (relativeHand != null) {
                var hand = rep.RigReferences.GetHand(handedness);
                hand.transform.position = grip.transform.TransformPoint(relativeHand.position);
                hand.transform.rotation = grip.transform.rotation * relativeHand.rotation.Expand();
            }
            
            rep.AttachObject(handedness, grip, useCustomJoint);
        }
    }
}
