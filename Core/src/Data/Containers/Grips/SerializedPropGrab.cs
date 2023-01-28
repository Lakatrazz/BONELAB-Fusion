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

        public SerializedTransform relativeGrip = null;

        public SerializedPropGrab() { }

        public SerializedPropGrab(string fullPath, ushort index, ushort id, GripPair pair)
        {
            this.fullPath = fullPath;
            this.index = index;
            this.id = id;

            var handTransform = pair.hand.transform;
            var gripTransform = pair.grip.Host.GetTransform();

            relativeGrip = new SerializedTransform(handTransform.InverseTransformPoint(gripTransform.position), handTransform.InverseTransformRotation(gripTransform.rotation));
        }

        public override void Serialize(FusionWriter writer)
        {
            base.Serialize(writer);

            writer.Write(fullPath);
            writer.Write(index);
            writer.Write(id);
            writer.Write(relativeGrip);
        }

        public override void Deserialize(FusionReader reader)
        {
            base.Deserialize(reader);

            fullPath = reader.ReadString();
            index = reader.ReadUInt16();
            id = reader.ReadUInt16();
            relativeGrip = reader.ReadFusionSerializable<SerializedTransform>();
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

        public override void RequestGrab(PlayerRep rep, Handedness handedness, Grip grip, bool useCustomJoint = true) {
            // Don't do anything if this isn't grabbed anymore
            if (!isGrabbed)
                return;

            // Set the host position so that the grip is created in the right spot
            var host = grip.Host.GetTransform();
            Vector3 position = host.position;
            Quaternion rotation = host.rotation;

            if (relativeGrip != null) {
                var hand = rep.RigReferences.GetHand(handedness).transform;

                host.SetPositionAndRotation(hand.TransformPoint(relativeGrip.position), hand.TransformRotation(relativeGrip.rotation.Expand()));
            }

            // There is no need for custom joints on prop grips
            // Since friction works properly and the grab is attached in the same spot
            useCustomJoint = false;

            // Apply the grab
            base.RequestGrab(rep, handedness, grip, useCustomJoint);

            // Reset the host position
            host.position = position;
            host.rotation = rotation;
        }
    }
}
