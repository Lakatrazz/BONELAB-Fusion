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
    public class StaticGrabGroupHandler : GrabGroupHandler<SerializedStaticGrab>
    {
        public override GrabGroup? Group => GrabGroup.STATIC;
    }

    public class SerializedStaticGrab : SerializedGrab
    {
        public string fullPath;

        public SerializedStaticGrab() { }

        public SerializedStaticGrab(string fullPath) {
            this.fullPath = fullPath;
        }

        public override void Serialize(FusionWriter writer) {
            writer.Write(fullPath);
        }

        public override void Deserialize(FusionReader reader) {
            fullPath = reader.ReadString();
        }

        public override Grip GetGrip() {
            var go = GameObject.Find(fullPath);

            if (go) {
                var grip = Grip.Cache.Get(go);
                return grip;
            }
            else {
#if DEBUG
                FusionLogger.Log($"Failed to find static grip!");
#endif
            }

            return null;
        }
    }
}
