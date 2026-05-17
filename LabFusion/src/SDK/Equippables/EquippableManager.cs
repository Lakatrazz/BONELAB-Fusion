using LabFusion.Player;

namespace LabFusion.SDK.Equippables;

public static class EquippableManager
{
    public static List<IEquippableItem> Equippables { get; } = new();

    public static Dictionary<string, IEquippableItem> BarcodeToEquippableLookup { get; } = new();

    public static HashSet<string> LocalEquippedItems { get; } = new();

    public static Dictionary<byte, HashSet<string>> NetEquippedItems { get; } = new();

    public static bool IsLocalEquipped(string barcode)
    {
        return LocalEquippedItems.Contains(barcode);
    }

    public static bool IsNetEquipped(PlayerID playerID, string barcode)
    {
        if (playerID.IsMe)
        {
            return IsLocalEquipped(barcode);
        }

        if (!NetEquippedItems.TryGetValue(playerID.SmallID, out var equippedItems))
        {
            return false;
        }

        return equippedItems.Contains(barcode);
    }

    public static IEquippableItem GetEquippable(string barcode) 
    {
        TryGetEquippable(barcode, out var equippable);
        return equippable;
    }

    public static void EquipEquippable(string barcode, bool equip = true) => ProcessLocalEquip(barcode, equip);

    public static bool TryGetEquippable(string barcode, out IEquippableItem equippable)
    {
        if (BarcodeToEquippableLookup.TryGetValue(barcode, out equippable))
        {
            return true;
        }

        equippable = null;
        return false;
    }

    public static void RegisterEquippable(IEquippableItem equippable)
    {
        Equippables.Add(equippable);

        BarcodeToEquippableLookup[equippable.Barcode] = equippable;
    }

    internal static void ProcessLocalEquip(string barcode, bool equipped)
    {
        var equippedItems = LocalEquippedItems;

        if (equippedItems.Contains(barcode) == equipped)
        {
            return;
        }

        if (equipped)
        {
            equippedItems.Add(barcode);
        }
        else
        {
            equippedItems.Remove(barcode);
        }

        if (TryGetEquippable(barcode, out var equippable))
        {
            equippable.OnLocalEquipped(equipped);
        }
    }

    internal static void ProcessNetEquip(PlayerID playerID, string barcode, bool equipped)
    {
        var smallID = playerID.SmallID;

        if (!NetEquippedItems.ContainsKey(smallID))
        {
            NetEquippedItems[smallID] = new();
        }

        var equippedItems = NetEquippedItems[smallID];

        if (equippedItems.Contains(barcode) == equipped)
        {
            return;
        }

        if (equipped)
        {
            equippedItems.Add(barcode);
        }
        else
        {
            equippedItems.Remove(barcode);
        }

        if (TryGetEquippable(barcode, out var equippable))
        {
            equippable.OnNetEquipped(playerID, equipped);
        }
    }

    internal static void ClearNetEquippedItems(PlayerID playerID)
    {
        var smallID = playerID.SmallID;

        if (!NetEquippedItems.TryGetValue(smallID, out var equippedItems))
        {
            return;
        }

        foreach (var barcode in equippedItems)
        {
            if (!TryGetEquippable(barcode, out var equippable))
            {
                continue;
            }

            equippable.OnNetEquipped(playerID, false);
        }

        NetEquippedItems.Remove(smallID);
    }
}
