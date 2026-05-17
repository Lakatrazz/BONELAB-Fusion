using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.UI.Popups;

using System.Reflection;

using UnityEngine;

using Il2CppSLZ.Marrow;

namespace LabFusion.SDK.Points;

using System;

// Terraria rarity levels
public enum RarityLevel
{
    Gray = -1,
    White = 0,
    Blue = 1,
    Green = 2,
    Orange = 3,
    LightRed = 4,
    Pink = 5,
    LightPurple = 6,
    Lime = 7,
    Yellow = 8,
    Cyan = 9,
    Red = 10,
    Purple = 11,
}

public enum SortMode
{
    PRICE,
    NAME,
    TAG,
    EQUIPPED,
    UNEQUIPPED,
    LAST_SORT,
}

public static class PointItemManager
{
    public static event Action OnBitCountChanged = null;
    public static event Action<IPointItem> OnItemUnlocked = null;

    public static RarityLevel CalculateLevel(int price)
    {
        if (price >= 5000)
        {
            return RarityLevel.Purple;
        }
        else if (price >= 4000)
        {
            return RarityLevel.Red;
        }
        else if (price >= 3000)
        {
            return RarityLevel.Cyan;
        }
        else if (price >= 2500)
        {
            return RarityLevel.Yellow;
        }
        else if (price >= 2000)
        {
            return RarityLevel.Lime;
        }
        else if (price >= 1500)
        {
            return RarityLevel.LightPurple;
        }
        else if (price >= 1200)
        {
            return RarityLevel.Pink;
        }
        else if (price >= 1000)
        {
            return RarityLevel.LightRed;
        }
        else if (price >= 800)
        {
            return RarityLevel.Orange;
        }
        else if (price >= 300)
        {
            return RarityLevel.Green;
        }
        else if (price >= 200)
        {
            return RarityLevel.Blue;
        }
        else if (price >= 120)
        {
            return RarityLevel.White;
        }

        return RarityLevel.Gray;
    }

    public static Color ParseColor(RarityLevel level)
    {
        return level switch
        {
            RarityLevel.Gray => Color.gray,
            RarityLevel.Blue => Color.blue,
            RarityLevel.Green => Color.green,
            RarityLevel.Orange => new Color(1f, 0.647f, 0f),
            RarityLevel.LightRed => new Color(1f, 0.447f, 0.462f),
            RarityLevel.Pink => new Color(1f, 0.411f, 0.7f),
            RarityLevel.LightPurple => new Color(0.796f, 0.764f, 0.89f),
            RarityLevel.Lime => new Color(0.749f, 1f, 0f),
            RarityLevel.Yellow => Color.yellow,
            RarityLevel.Cyan => Color.cyan,
            RarityLevel.Red => Color.red,
            RarityLevel.Purple => new Color(0.5f, 0f, 0.5f),
            _ => Color.white,
        };
    }

    public static string ParsePrice(int price, bool unlocked = false)
    {
        if (unlocked)
        {
            return "Bought";
        }

        if (price <= 0)
        {
            return "Free";
        }

        if (price > BitEconomy.PricelessValue)
        {
            return "Priceless";
        }

        return $"{price} Bits";
    }

    internal static void HookEvents()
    {
    }

    internal static void UnhookEvents()
    {
    }

    public static void LoadItems(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new NullReferenceException("Tried loading point items from a null assembly!");
        }

        AssemblyUtilities.LoadAllValid<IPointItem>(assembly, RegisterPointItem);
    }

    public static void RegisterPointItem<T>() where T : IPointItem => RegisterPointItem(typeof(T));

    private static void RegisterPointItem(Type type)
    {
        // Only register compiled point items
        if (type.GetCustomAttribute<CompiledPointItemAttribute>() == null)
        {
            return;
        }

        var item = Activator.CreateInstance(type) as IPointItem;

        RegisterPointItem(item);
    }

    public static void RegisterPointItem(IPointItem item)
    {
        if (PointItemLookup.ContainsKey(item.Barcode))
        {
            FusionLogger.Error($"Tried registering PointItem with barcode {item.Barcode}, but that barcode was already registered!");
            return;
        }

        PointItems.Add(item);
        PointItemLookup.Add(item.Barcode, item);

        item.OnRegistered();

        if (item.IsEquipped)
        {
            OnEquipChanged(PlayerIDManager.LocalID, item.Barcode, true);
        }
    }

    public static bool TryGetPointItem(string barcode, out IPointItem item)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            item = null;
            return false;
        }

        return PointItemLookup.TryGetValue(barcode, out item);
    }

    public static int GetBitCount()
    {
        return PointSaveManager.GetBitCount();
    }

    public static void RewardBits(int bits, bool popup = true)
    {
        bits = Math.Max(0, bits);

        // Make sure the amount isn't invalid
        if (bits.IsNaN())
        {
            FusionLogger.ErrorLine("Prevented attempt to give invalid bit reward. Please notify a Fusion developer and send them your log.");
            return;
        }

        var currentBits = GetBitCount();
        PointSaveManager.SetBitCount(currentBits + bits);

        if (popup)
        {
            BitPopup.Send(bits);
        }

        OnBitCountChanged.InvokeSafe("executing OnBitCountChanged");
    }

    public static void DecrementBits(int bits, bool popup = true)
    {
        bits = Math.Max(0, bits);

        // Make sure the amount isn't invalid
        if (bits.IsNaN())
        {
            FusionLogger.ErrorLine("Prevented attempt to remove an invalid bit amount. Please notify a Fusion developer and send them your log.");
            return;
        }

        var currentBits = GetBitCount();
        PointSaveManager.SetBitCount(currentBits - bits);

        if (popup)
        {
            BitPopup.Send(-bits);
        }

        OnBitCountChanged.InvokeSafe("executing OnBitCountChanged");
    }

    public static bool TryUpgradeItem(IPointItem item)
    {
        var unlockedItems = GetUnlockedItems();

        if (!unlockedItems.Contains(item))
        {
            return false;
        }

        if (item.IsMaxUpgrade)
        {
            return false;
        }

        int price = item.CurrentPrice;
        int bits = GetBitCount();

        if (price < 0)
            return false;

        if (price > bits)
            return false;

        PointSaveManager.UpgradeItem(item.Barcode);

        DecrementBits(price);

        return true;
    }

    public static bool TryBuyItem(IPointItem item)
    {
        var unlockedItems = GetUnlockedItems();

        if (unlockedItems.Contains(item))
        {
            return false;
        }

        int price = item.Price;
        int bits = GetBitCount();

        if (price < 0)
        {
            return false;
        }

        if (price > bits)
        {
            return false;
        }

        PointSaveManager.UnlockItem(item.Barcode);

        DecrementBits(price);

        OnItemUnlocked?.Invoke(item);

        return true;
    }

    internal static void OnEquipChanged(PlayerID id, string barcode, bool isEquipped)
    {
        if (!TryGetPointItem(barcode, out var item))
        {
            return;
        }

        item.OnEquipChanged(id, isEquipped);
    }

    internal static void Internal_OnTriggerItem(PlayerID id, string barcode, string value = null)
    {
        // if (!TryGetPointItem(barcode, out var item))
        // {
        //     return;
        // }
        // 
        // // Get the rig info
        // RigManager manager = null;
        // PointItemPayloadType type = PointItemPayloadType.SELF;
        // 
        // if (id == null || id.IsMe)
        // {
        //     manager = RigData.Refs.RigManager;
        //     type = PointItemPayloadType.SELF;
        // }
        // else if (NetworkPlayerManager.TryGetPlayer(id, out var rep))
        // {
        //     manager = rep.RigRefs.RigManager;
        //     type = PointItemPayloadType.PLAYER_REP;
        // }
        // 
        // // Update equip
        // var payload = new PointItemPayload()
        // {
        //     type = type,
        //     playerId = id,
        //     rigManager = manager,
        // };
        // 
        // if (value != null)
        // {
        //     item.OnTrigger(payload, value);
        // }
        // else
        // {
        //     item.OnTrigger(payload);
        // }
    }

    public static void SetEquipped(IPointItem item, bool isEquipped)
    {
        if (item == null || (!item.IsUnlocked && !item.IsEquipped))
        {
            return;
        }

        OnEquipChanged(PlayerIDManager.LocalID, item.Barcode, isEquipped);
        PointSaveManager.SetEquipped(item.Barcode, isEquipped);
        PointItemSender.SendPointItemEquip(item.Barcode, isEquipped);
    }

    public static void UnequipAll()
    {
        foreach (var item in LoadedItems)
        {
            SetEquipped(item, false);
        }
    }

    public static IReadOnlyList<IPointItem> GetLockedItems(SortMode sort = SortMode.PRICE)
    {
        List<IPointItem> items = new(LoadedItems.Count);

        foreach (var item in LoadedItems)
        {
            if (item.Redacted)
            {
                continue;
            }

            if ((sort == SortMode.EQUIPPED && !item.IsEquipped) || (sort == SortMode.UNEQUIPPED && item.IsEquipped))
            {
                continue;
            }

            if (!item.IsUnlocked)
            {
                items.Add(item);
            }
        }

        SortBy(ref items, sort);

        return items;
    }

    public static IReadOnlyList<IPointItem> GetUnlockedItems(SortMode sort = SortMode.PRICE)
    {
        List<IPointItem> items = new(LoadedItems.Count);

        foreach (var item in LoadedItems)
        {
            if ((sort == SortMode.EQUIPPED && !item.IsEquipped) || (sort == SortMode.UNEQUIPPED && item.IsEquipped))
            {
                continue;
            }

            if (item.IsUnlocked)
            {
                items.Add(item);
            }
        }

        SortBy(ref items, sort);

        return items;
    }

    private static void SortBy(ref List<IPointItem> items, SortMode sort)
    {
        switch (sort)
        {
            case SortMode.PRICE:
                items.Sort((x, y) => x.Price - y.Price);
                break;
            case SortMode.TAG:
                items.Sort((x, y) => x.MainTag.CompareTo(y.MainTag));
                break;
            case SortMode.NAME:
                items.Sort((x, y) => x.Title.CompareTo(y.Title));
                break;
        }
    }

    public static IReadOnlyList<IPointItem> LoadedItems => PointItems;

    internal static readonly List<IPointItem> PointItems = new();
    internal static readonly Dictionary<string, IPointItem> PointItemLookup = new();
}