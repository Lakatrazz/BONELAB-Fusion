using LabFusion.Player;

namespace LabFusion.SDK.Equippables;

public interface IEquippableItem
{
    string Barcode { get; }

    void OnLocalEquipChanged(bool equipped);

    void OnNetEquipChanged(PlayerID playerID, bool equipped);
}
