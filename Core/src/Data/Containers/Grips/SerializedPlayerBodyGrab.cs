using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ.Interaction;

namespace LabFusion.Data {

    [SerializedGrabGroup(group = SyncUtilities.SyncGroup.PLAYER_BODY)]
    public class SerializedPlayerBodyGrab : SerializedGrab {
        public byte grabbedUser;
        public byte gripIndex;

        public SerializedPlayerBodyGrab(byte grabbedUser, byte gripIndex) {
            this.grabbedUser = grabbedUser;
            this.gripIndex = gripIndex;
        }

        public SerializedPlayerBodyGrab() { }

        public override void Serialize(FusionWriter writer) {
            writer.Write(grabbedUser);
            writer.Write(gripIndex);
        }

        public override void Deserialize(FusionReader reader) {
            grabbedUser = reader.ReadByte();
            gripIndex = reader.ReadByte();
        }

        public override Grip GetGrip() {
            RigReferenceCollection references = null;
            if (grabbedUser == PlayerIdManager.LocalSmallId)
                references = RigData.RigReferences;
            else if (PlayerRep.Representations.ContainsKey(grabbedUser))
                references = PlayerRep.Representations[grabbedUser].RigReferences;

            return references?.GetGrip(gripIndex);
        }
    }
}
