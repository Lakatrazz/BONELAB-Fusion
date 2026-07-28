using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Utilities;

namespace LabFusion.Marrow.Interaction;

public class GrabSnapshot
{
    public GripReference GripReference { get; } = GripReference.None;

    public SimpleTransform? TargetInBase { get; } = null;

    public Grip Grip { get; set; } = null;

    public GrabSnapshot(SerializedGrab grab)
    {
        GripReference = grab.GripReference;
        TargetInBase = SimpleTransform.Create(grab.TargetInBase.position, grab.TargetInBase.rotation);
    }

    public bool TryGetGrip(out Grip grip)
    {
        if (Grip != null)
        {
            grip = Grip;
            return true;
        }

        if (GripReference.TryGetGrip(out grip))
        {
            Grip = grip;
            return true;
        }

        grip = null;
        return false;
    }
}
