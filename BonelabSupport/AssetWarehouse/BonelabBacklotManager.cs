using LabFusion.Marrow;

namespace MarrowFusion.Bonelab;

public static class BonelabBacklotManager
{
    public static void Initialize()
    {
        BacklotReferences.ConstrainerReference = BonelabSpawnableReferences.ConstrainerReference;
    }
}
