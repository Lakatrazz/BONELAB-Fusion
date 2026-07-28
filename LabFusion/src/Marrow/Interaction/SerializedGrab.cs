using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Data;
using LabFusion.Network.Serialization;

namespace LabFusion.Marrow.Interaction;

public class SerializedGrab : INetSerializable
{
    public GripReference GripReference;

    public SerializedTransform TargetInBase;

    public Handedness Handedness;

    public static SerializedGrab CreateFromHandGripPair(Hand hand, Grip grip)
    {
        var target = grip.GetTargetInBase(hand);

        return new SerializedGrab()
        {
            GripReference = GripReference.CreateFromGrip(grip),
            TargetInBase = new SerializedTransform(target.position, target.rotation),
            Handedness = hand.handedness,
        };
    }

    public int? GetSize() => GripReference.GetSize() + SerializedTransform.Size + sizeof(byte);

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref GripReference);
        serializer.SerializeValue(ref TargetInBase);
        serializer.SerializeValue(ref Handedness, Precision.OneByte);
    }
}
