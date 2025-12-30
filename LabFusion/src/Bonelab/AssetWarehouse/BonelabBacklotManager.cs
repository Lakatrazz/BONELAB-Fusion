using LabFusion.Marrow;

namespace LabFusion.Bonelab;

public static class BonelabBacklotManager
{
    public static void Initialize()
    {
        BacklotReferences.ConstrainerReference = BonelabSpawnableReferences.ConstrainerReference;
    }
}
