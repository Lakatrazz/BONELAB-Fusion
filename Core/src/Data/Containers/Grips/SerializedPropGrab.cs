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
        public new const int Size = SerializedGrab.Size + sizeof(ushort) * 2 + SerializedTransform.Size;

        public string fullPath;
        public ushort index;
        public ushort id;
        public SerializedTransform relativeHand = null;

        public SerializedPropGrab() { }

        public SerializedPropGrab(string fullPath, ushort index, ushort id)
        {
            this.fullPath = fullPath;
            this.index = index;
            this.id = id;
        }

        public override void WriteDefaultGrip(Hand hand, Grip grip)
        {
            base.WriteDefaultGrip(hand, grip);

            relativeHand = gripPair.GetRelativeHand();
        }

        public override void Serialize(FusionWriter writer)
        {
            base.Serialize(writer);

            writer.Write(fullPath);
            writer.Write(index);
            writer.Write(id);
            writer.Write(relativeHand);
        }

        public override void Deserialize(FusionReader reader)
        {
            base.Deserialize(reader);

            fullPath = reader.ReadString();
            index = reader.ReadUInt16();
            id = reader.ReadUInt16();
            relativeHand = reader.ReadFusionSerializable<SerializedTransform>();
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

        public override void RequestGrab(PlayerRep rep, Handedness handedness, Grip grip)
        {
            // Don't do anything if this isn't grabbed anymore
            if (!isGrabbed)
                return;

            // Get the hand and its starting values
            Hand hand = rep.RigReferences.GetHand(handedness);

            Transform handTransform = hand.transform;
            Vector3 position = handTransform.position;
            Quaternion rotation = handTransform.rotation;

            // Move the hand into its relative position
            grip.SetRelativeHand(hand, relativeHand);

            // Apply the grab
            base.RequestGrab(rep, handedness, grip);

            // Reset the hand position
            handTransform.SetPositionAndRotation(position, rotation);
        }

    }
}
