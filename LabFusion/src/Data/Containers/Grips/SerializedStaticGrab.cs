using LabFusion.Network;
using LabFusion.Grabbables;
using LabFusion.Entities;
using LabFusion.Patching;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

using UnityEngine;

namespace LabFusion.Data;

public class StaticGrabGroupHandler : GrabGroupHandler<SerializedStaticGrab>
{
    public override GrabGroup? Group => GrabGroup.STATIC;
}

public class SerializedStaticGrab : SerializedGrab
{
    public new const int Size = SerializedGrab.Size + ComponentHashData.Size + SerializedTransform.Size;

    public ComponentHashData gripHash = null;
    public SerializedTransform worldHand = default;

    public SerializedStaticGrab() { }

    public SerializedStaticGrab(ComponentHashData gripHash)
    {
        this.gripHash = gripHash;
    }

    public override int GetSize()
    {
        return Size;
    }

    public override void WriteDefaultGrip(Hand hand, Grip grip)
    {
        base.WriteDefaultGrip(hand, grip);

        worldHand = new SerializedTransform(hand.transform);
    }

    public override void Serialize(FusionWriter writer)
    {
        base.Serialize(writer);

        writer.Write(gripHash);
        writer.Write(worldHand);
    }

    public override void Deserialize(FusionReader reader)
    {
        base.Deserialize(reader);

        gripHash = reader.ReadFusionSerializable<ComponentHashData>();
        worldHand = reader.ReadFusionSerializable<SerializedTransform>();
    }

    public override Grip GetGrip()
    {
        var grip = GripPatches.HashTable.GetComponentFromData(gripHash);

        return grip;
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

        // Move the hand into its world position
        handTransform.SetPositionAndRotation(worldHand.position, worldHand.rotation);

        // Apply the grab
        base.RequestGrab(player, handedness, grip);

        // Reset the hand position
        handTransform.SetPositionAndRotation(position, rotation);
    }
}