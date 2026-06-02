using LabFusion.Player;
using LabFusion.SDK.Equippables;
using LabFusion.Extensions;

namespace LabFusion.SDK.Wearables;

public delegate void LocalWearableEquippedDelegate(WearableItem item, bool equipped);

public delegate void NetWearableEquippedDelegate(WearableItem item, PlayerID playerID, bool equipped);

public abstract class WearableItem : IEquippableItem
{
    public static event LocalWearableEquippedDelegate LocalEquipped;

    public static event NetWearableEquippedDelegate NetEquipped;

    public abstract string Barcode { get; }

    public abstract WearableInstance CreateInstance();

    public void OnLocalEquipChanged(bool equipped)
    {
        LocalEquipped?.InvokeSafe(this, equipped, "executing WearableItem.LocalEquipped event");
    }

    public void OnNetEquipChanged(PlayerID playerID, bool equipped)
    {
        NetEquipped?.InvokeSafe(this, playerID, equipped, "executing WearableItem.NetEquipped event");
    }
}
