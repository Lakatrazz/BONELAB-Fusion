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
            Anchor = new AvatarAnchor(AvatarPoint.Wrist, AvatarAlignment.Back, AvatarSide.Left),
            SpawnableCrateReference = FusionSpawnableReferences.WristWatchReference,
            Components = new() { new WristWatchBehavior() },
        };
    }
}
