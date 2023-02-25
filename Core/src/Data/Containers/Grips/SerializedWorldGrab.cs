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

        public SerializedWorldGrab() { }

        public SerializedWorldGrab(byte grabberId)
        {
            this.grabberId = grabberId;
        }

        public override void Serialize(FusionWriter writer)
        {
            base.Serialize(writer);

            writer.Write(grabberId);
        }

        public override void Deserialize(FusionReader reader)
        {
            base.Deserialize(reader);

            grabberId = reader.ReadByte();
        }

        public override Grip GetGrip()
        {
            if (PlayerRepManager.TryGetPlayerRep(grabberId, out var rep)) {
                if (rep.RigReferences.RigManager) {
                    var worldGrip = rep.RigReferences.RigManager.worldGrip;
                    return worldGrip;
                }
            }

            return null;
        }
    }
}
