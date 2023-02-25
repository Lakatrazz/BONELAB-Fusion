using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Syncables;

using SLZ;
using SLZ.Interaction;

using UnityEngine;

using LabFusion.Grabbables;
using LabFusion.Extensions;

namespace LabFusion.Data
{
    public class PropGrabGroupHandler : GrabGroupHandler<SerializedPropGrab>
    {
        public override GrabGroup? Group => GrabGroup.PROP;
    }

    public class SerializedPropGrab : SerializedGrab {
        public string fullPath;
        public ushort index;
        public ushort id;

        public SerializedPropGrab() { }

        public SerializedPropGrab(string fullPath, ushort index, ushort id)
        {
            this.fullPath = fullPath;
            this.index = index;
            this.id = id;
        }

        public override void Serialize(FusionWriter writer)
        {
            base.Serialize(writer);

            writer.Write(fullPath);
            writer.Write(index);
            writer.Write(id);
        }

        public override void Deserialize(FusionReader reader)
        {
            base.Deserialize(reader);

            fullPath = reader.ReadString();
            index = reader.ReadUInt16();
            id = reader.ReadUInt16();
        }

        public Grip GetGrip(out PropSyncable syncable) {
            GameObject go;
            InteractableHost host;
            syncable = null;

            if (SyncManager.TryGetSyncable(id, out var foundSyncable))
            {

                if (foundSyncable is PropSyncable prop) {
                    syncable = prop;
                    return syncable.GetGrip(index);
                }
            }
            else if (fullPath != "_" && (go = GameObjectUtilities.GetGameObject(fullPath)) && (host = InteractableHost.Cache.Get(go)))
            {
                syncable = new PropSyncable(host);
                SyncManager.RegisterSyncable(syncable, id);

                return syncable.GetGrip(index);
            }

            return null;
        }

        public override Grip GetGrip() {
            return GetGrip(out _);
        }
    }
}
