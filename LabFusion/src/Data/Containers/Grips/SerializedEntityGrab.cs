using LabFusion.Network;
using LabFusion.Grabbables;
using LabFusion.Extensions;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Interaction;

using UnityEngine;

using Il2CppSLZ.Marrow;

namespace LabFusion.Data;

public class EntityGrabGroupHandler : GrabGroupHandler<SerializedEntityGrab>
{
    public override GrabGroup? Group => GrabGroup.ENTITY;
}

public class SerializedEntityGrab : SerializedGrab
{
    public new const int Size = SerializedGrab.Size + sizeof(ushort) * 2 + SerializedTransform.Size;

    public ushort index;
    public ushort id;
    public SerializedTransform relativeHand = default;

    public SerializedEntityGrab() { }

    public SerializedEntityGrab(ushort index, ushort id)
    {
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

        writer.Write(index);
        writer.Write(id);
        writer.Write(relativeHand);
    }

    public override void Deserialize(FusionReader reader)
    {
        base.Deserialize(reader);

        index = reader.ReadUInt16();
        id = reader.ReadUInt16();
        relativeHand = reader.ReadFusionSerializable<SerializedTransform>();
    }

    public Grip GetGrip(out NetworkEntity entity)
    {
        entity = null;

        var foundEntity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(id);

        if (foundEntity != null)
        {
            entity = foundEntity;
            var gripExtender = foundEntity.GetExtender<GripExtender>();

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
        {
            return;
        }

        // Get the hand and its starting values
        Hand hand = player.RigRefs.GetHand(handedness);

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