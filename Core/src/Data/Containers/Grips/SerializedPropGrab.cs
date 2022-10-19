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
        public bool isGrabbed;

        public SerializedPropGrab() { }

        public SerializedPropGrab(string fullPath, ushort index, ushort id, bool isGrabbed)
        {
            this.fullPath = fullPath;
            this.index = index;
            this.id = id;
            this.isGrabbed = isGrabbed;
        }

        public override void Serialize(FusionWriter writer)
        {
            writer.Write(fullPath);
            writer.Write(index);
            writer.Write(id);
            writer.Write(isGrabbed);
        }

        public override void Deserialize(FusionReader reader)
        {
            fullPath = reader.ReadString();
            index = reader.ReadUInt16();
            id = reader.ReadUInt16();
            isGrabbed = reader.ReadBoolean();
        }

        public override Grip GetGrip()
        {
            GameObject go;
            InteractableHost host;

            if (SyncManager.TryGetSyncable(id, out var syncable)) {
#if DEBUG
                FusionLogger.Log($"Found existing prop grip!");
#endif

                return syncable.GetGrip(index);
            }
            else if ((go = GameObjectUtilities.GetGameObject(fullPath)) && (host = InteractableHost.Cache.Get(go))) {
                syncable = new PropSyncable(go, (Grip[])host._grips.ToArray());
                SyncManager.RegisterSyncable(syncable, id);

#if DEBUG
                FusionLogger.Log($"Creating new prop grip with id {id} at index {index}!");
#endif

                return syncable.GetGrip(index);
            }
            else
            {
#if DEBUG
                FusionLogger.Log($"Failed to find prop grip!");
#endif
            }

            return null;
        }

        public override void RequestGrab(PlayerRep rep, Handedness handedness, Grip grip) {
            if (isGrabbed)
                base.RequestGrab(rep, handedness, grip);
        }
    }
}
