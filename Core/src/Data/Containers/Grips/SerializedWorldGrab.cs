using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;

using SLZ.Interaction;

using UnityEngine;

namespace LabFusion.Data
{
    public class WorldGrabGroupHandler : GrabGroupHandler<SerializedWorldGrab> {
        public override GrabGroup? Group => GrabGroup.WORLD_GRIP;
    }

    public class SerializedWorldGrab : SerializedGrab
    {
        public byte grabberId;
        public SerializedTransform gripTransform;

        public SerializedWorldGrab() { }

        public SerializedWorldGrab(byte grabberId, SerializedTransform gripTransform)
        {
            this.grabberId = grabberId;
            this.gripTransform = gripTransform;
        }

        public override void Serialize(FusionWriter writer)
        {
            writer.Write(grabberId);
            writer.Write(gripTransform);
        }

        public override void Deserialize(FusionReader reader)
        {
            grabberId = reader.ReadByte();
            gripTransform = reader.ReadFusionSerializable<SerializedTransform>();
        }

        public override Grip GetGrip()
        {
            if (PlayerRep.Representations.ContainsKey(grabberId)) {
                var rep = PlayerRep.Representations[grabberId];
                
                if (rep.RigReferences.RigManager) {
                    var worldGrip = rep.RigReferences.RigManager.worldGrip;
                    worldGrip.transform.position = gripTransform.position;
                    worldGrip.transform.rotation = gripTransform.rotation.Expand();
                    return worldGrip;
                }
            }

            return null;
        }
    }
}
