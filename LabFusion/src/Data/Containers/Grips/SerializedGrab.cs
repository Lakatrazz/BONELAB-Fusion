using LabFusion.Entities;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Network.Serialization;

namespace LabFusion.Data;

public abstract class SerializedGrab : INetSerializable
{
    public const int Size = sizeof(byte) + SerializedTransform.Size;

    public bool IsGrabbed;
    public SerializedTransform TargetInBase;
    public GripPair GripPair;

    public void WriteDefaultGrip(Hand hand, Grip grip)
    {
        // Check if this is actually grabbed
        IsGrabbed = hand.m_CurrentAttachedGO == grip.gameObject;

        // Store the target
        var target = grip.GetTargetInBase(hand);
        TargetInBase = new SerializedTransform(target.position, target.rotation);

        GripPair = new GripPair(hand, grip);
    }

    public virtual int GetSize()
    {
        return Size;
    }

    public virtual void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref IsGrabbed);
        serializer.SerializeValue(ref TargetInBase);
    }

    public abstract Grip GetGrip();

    public void RequestGrab(NetworkPlayer player, Handedness handedness, Grip grip)
    {
        // Make sure the grip exists
        if (grip == null)
        {
            return;
        }

        // Don't do anything if this isn't grabbed anymore
        if (!IsGrabbed)
        {
            return;
        }

        // Don't grab if the player rig doesn't exist
        if (!player.HasRig)
        {
            return;
        }

        player.Grabber.Attach(handedness, grip, SimpleTransform.Create(TargetInBase.position, TargetInBase.rotation));
    }
}
