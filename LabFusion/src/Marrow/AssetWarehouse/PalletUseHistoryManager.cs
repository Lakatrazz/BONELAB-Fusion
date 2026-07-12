using Il2CppSLZ.Marrow.Warehouse;

using System.Text.Json.Serialization;

namespace LabFusion.Marrow;

[Serializable]
public class PalletUseEntry
{
    [JsonPropertyName("barcode")]
    public string Barcode { get; set; } = string.Empty;

    [JsonPropertyName("lastUseTime")]
    public DateTime LastUseTime { get; set; } = DateTime.MinValue;
}

[Serializable]
public class PalletUseHistory
{
    [JsonPropertyName("entries")]
    public List<PalletUseEntry> Entries { get; set; } = new();
}

public static class PalletUseHistoryManager
{
    public static PalletUseHistory History { get; private set; } = new();

    public static HashSet<string> ActivelyUsedPallets { get; } = new();

    public static bool IsPalletActivelyUsed(string barcode) => ActivelyUsedPallets.Contains(barcode);

    public static void ClearActivelyUsedPallets()
    {
        ActivelyUsedPallets.Clear();
    }

    public static void MarkPalletUsed(string barcode)
    {
        var entry = GetOrAddEntry(barcode);

        entry.LastUseTime = DateTime.UtcNow;

        ActivelyUsedPallets.Add(barcode);
    }

    public static void MarkPalletUsed(Pallet pallet) 
    { 
        if (pallet == null)
        {
            return;
        }

        MarkPalletUsed(pallet.Barcode.ID); 
    }

    public static void MarkCrateUsed(Crate crate) 
    { 
        if (crate == null)
        {
            return;
        }

        MarkPalletUsed(crate.Pallet); 
    }

    public static PalletUseEntry GetOrAddEntry(string barcode)
    {
        var existingEntry = GetEntry(barcode);

        if (existingEntry != null)
        {
            return existingEntry;
        }

        var newEntry = new PalletUseEntry() { Barcode = barcode };
        History.Entries.Add(newEntry);

        return newEntry;
    }

    public static PalletUseEntry GetEntry(string barcode)
    {
        return History.Entries.FirstOrDefault(e => e.Barcode == barcode);
    }

    public static DateTime GetLastUseTime(string barcode)
    {
        var entry = GetEntry(barcode);

        if (entry != null)
        {
            return entry.LastUseTime;
        }

        return DateTime.MinValue;
    }
}