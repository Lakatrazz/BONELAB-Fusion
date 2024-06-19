using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Syncables;
using LabFusion.Grabbables;
using LabFusion.Extensions;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Interaction;

using UnityEngine;
using LabFusion.Entities;

namespace LabFusion.Data
{
    public class PropGrabGroupHandler : GrabGroupHandler<SerializedPropGrab>
    {
        public override GrabGroup? Group => GrabGroup.PROP;
    }

    public class SerializedPropGrab : SerializedGrab
    {
        public new const int Size = SerializedGrab.Size + sizeof(ushort) * 2 + SerializedTransform.Size;

        public string fullPath;
        public ushort index;
        public ushort id;
        public SerializedTransform relativeHand = default;

        public SerializedPropGrab() { }

        public SerializedPropGrab(string fullPath, ushort index, ushort id)
        {
            this.fullPath = fullPath;
            this.index = index;
            this.id = id;
        }

        public override int GetSize()
        {
            return Size + fullPath.GetSize();
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

        public Grip GetGrip(out NetworkProp prop)
        {
            GameObject go;
            InteractableHost host;
            prop = null;

            var foundEntity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(id);
            var foundProp = foundEntity?.GetExtender<NetworkProp>();

            if (foundProp != null)
            {
                prop = foundProp;
                var gripExtender = foundEntity.GetExtender<Entities.GripExtender>();

                if (gripExtender != null)
                {
                    return gripExtender.GetComponent(index);
                }
            }
            else if (fullPath != "_" && (go = GameObjectUtilities.GetGameObject(fullPath)) && (host = InteractableHost.Cache.Get(go)))
            {
                NetworkEntity entity = new();
                prop = new NetworkProp(entity, host.GetComponentInParent<MarrowEntity>());

                NetworkEntityManager.IdManager.RegisterEntity(id, entity);

                var gripExtender = entity.GetExtender<Entities.GripExtender>();

                if (gripExtender != null)
                {
                    return gripExtender.GetComponent(index);
                }
            }

            return null;
        }

        public override Grip GetGrip()
        {
            return GetGrip(out _);
        }

        public override void RequestGrab(NetworkPlayer player, Handedness handedness, Grip grip)
        {
            // Don't do anything if this isn't grabbed anymore
            if (!isGrabbed)
                return;

            // Get the hand and its starting values
            Hand hand = player.RigReferences.GetHand(handedness);

            Transform handTransform = hand.transform;
            Vector3 position = handTransform.position;
            Quaternion rotation = handTransform.rotation;

            // Move the hand into its relative position
            grip.SetRelativeHand(hand, relativeHand);

            // Apply the grab
            base.RequestGrab(player, handedness, grip);

            // Reset the hand position
            handTransform.SetPositionAndRotation(position, rotation);
        }

    }
}
