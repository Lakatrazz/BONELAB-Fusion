using LabFusion.Player;

namespace LabFusion.SDK.Equippables;

public interface IEquippableItem
{
    string Barcode { get; }

    void OnLocalEquipped(bool equipped);

    void OnNetEquipped(PlayerID playerID, bool equipped);
}
