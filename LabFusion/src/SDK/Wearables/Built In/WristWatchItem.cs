using LabFusion.Marrow;
using LabFusion.Marrow.Integration;

namespace LabFusion.SDK.Wearables;

public class WristWatchItem : WearableItem
{
    public override string Barcode => FusionSpawnableReferences.WristWatchReference.Barcode.ID;

    public override WearableInstance CreateInstance()
    {
        return new WearableInstance()
        {
            Point = WearablePoint.WristLeftTop,
            SpawnableCrateReference = FusionSpawnableReferences.WristWatchReference,
        };
    }
}
