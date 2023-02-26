using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Extensions;
using LabFusion.Grabbables;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ;
using SLZ.Interaction;

using UnityEngine;

namespace LabFusion.Data {
    public class PlayerGrabGroupHandler : GrabGroupHandler<SerializedPlayerBodyGrab> {
        public override GrabGroup? Group => GrabGroup.PLAYER_BODY;
    }

    public class SerializedPlayerBodyGrab : SerializedGrab {
        public byte grabbedUser;
        public byte gripIndex;
        public bool isAvatarGrip;

        public SerializedPlayerBodyGrab(byte grabbedUser, byte gripIndex, bool isAvatarGrip) {
            this.grabbedUser = grabbedUser;
            this.gripIndex = gripIndex;
            this.isAvatarGrip = isAvatarGrip;
        }

        public SerializedPlayerBodyGrab() { }

        public override void Serialize(FusionWriter writer) {
            base.Serialize(writer);

            writer.Write(grabbedUser);
            writer.Write(gripIndex);
            writer.Write(isAvatarGrip);
        }

        public override void Deserialize(FusionReader reader) {
            base.Deserialize(reader);

            grabbedUser = reader.ReadByte();
            gripIndex = reader.ReadByte();
            isAvatarGrip = reader.ReadBoolean();
        }

        public override Grip GetGrip() {
            RigReferenceCollection references = null;
            if (grabbedUser == PlayerIdManager.LocalSmallId)
                references = RigData.RigReferences;
            else if (PlayerRepManager.TryGetPlayerRep(grabbedUser, out var rep))
                references = rep.RigReferences;

            return references?.GetGrip(gripIndex, isAvatarGrip);
        }
    }
}
