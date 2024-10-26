using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Entities;

using System.Reflection;

using UnityEngine;

using Il2CppSLZ.Marrow;

namespace LabFusion.SDK.Points;

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
    AUTHOR,
    RARITY,
    EQUIPPED,
    UNEQUIPPED,
    LAST_SORT,
}

public static class PointItemManager
{
    public static event Action OnBitCountChanged = null;
    public static event Action<PointItem> OnItemUnlocked = null;

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

    internal static void HookEvents()
    {
        LocalPlayer.OnLocalRigCreated += OnLocalRigCreated;
        NetworkPlayer.OnNetworkRigCreated += OnNetworkRigCreated;
    }

    internal static void UnhookEvents()
    {
        LocalPlayer.OnLocalRigCreated -= OnLocalRigCreated;
        NetworkPlayer.OnNetworkRigCreated -= OnNetworkRigCreated;
    }

    private static void OnLocalRigCreated(RigManager rigManager)
    {
        foreach (var item in LoadedItems)
        {
            if (item.IsEquipped)
            {
                item.OnUpdateObjects(new PointItemPayload()
                {
                    type = PointItemPayloadType.SELF,
                    rigManager = rigManager,
                    playerId = PlayerIdManager.LocalId,
                }, true);
            }
        }
    }

    private static void OnNetworkRigCreated(NetworkPlayer player, RigManager rigManager)
    {
        if (player.NetworkEntity.IsOwner)
        {
            return;
        }

        foreach (var item in LoadedItems)
        {
            if (player.PlayerId.EquippedItems.Contains(item.Barcode))
            {
                item.OnUpdateObjects(new PointItemPayload()
                {
                    type = PointItemPayloadType.PLAYER_REP,
                    rigManager = rigManager,
                    playerId = player.PlayerId,
                }, true);
            }
        }
    }

    public static void LoadItems(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new NullReferenceException("Tried loading point items from a null assembly!");
        }

        AssemblyUtilities.LoadAllValid<PointItem>(assembly, RegisterPointItem);
    }

    public static void RegisterPointItem<T>() where T : PointItem => RegisterPointItem(typeof(T));

    private static void RegisterPointItem(Type type)
    {
        // Only register compiled point items
        if (type.GetCustomAttribute<CompiledPointItemAttribute>() == null)
        {
            return;
        }

        var item = Activator.CreateInstance(type) as PointItem;

        RegisterPointItem(item);
    }

    public static void RegisterPointItem(PointItem item)
    {
        if (PointItemLookup.ContainsKey(item.Barcode))
        {
            FusionLogger.Error($"Tried registering PointItem with barcode {item.Barcode}, but that barcode was already registered!");
            return;
        }

        PointItems.Add(item);
        PointItemLookup.Add(item.Barcode, item);

        item.Register();

        if (item.IsEquipped)
        {
            Internal_OnEquipChange(PlayerIdManager.LocalId, item.Barcode, true);
        }
    }

    public static bool TryGetPointItem(string barcode, out PointItem item)
    {
        if (barcode == null)
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
            FusionBitPopup.Send(bits);
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
            FusionBitPopup.Send(-bits);
        }

        OnBitCountChanged.InvokeSafe("executing OnBitCountChanged");
    }

    public static bool TryUpgradeItem(PointItem item)
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

        int price = item.ActivePrice;
        int bits = GetBitCount();

        if (price < 0)
            return false;

        if (price > bits)
            return false;

        PointSaveManager.UpgradeItem(item.Barcode);

        DecrementBits(price);

        return true;
    }

    public static bool TryBuyItem(PointItem item)
    {
        var unlockedItems = GetUnlockedItems();

        if (unlockedItems.Contains(item))
        {
            return false;
        }

        int price = item.AdjustedPrice;
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

    internal static void Internal_OnEquipChange(PlayerId id, string barcode, bool isEquipped)
    {
        if (!TryGetPointItem(barcode, out var item))
        {
            return;
        }

        // Get the rig info
        RigManager manager = null;
        PointItemPayloadType type = PointItemPayloadType.SELF;

        if (id == null || id.IsMe)
        {
            manager = RigData.Refs.RigManager;
            type = PointItemPayloadType.SELF;
        }
        else if (NetworkPlayerManager.TryGetPlayer(id, out var rep))
        {
            manager = rep.RigRefs.RigManager;
            type = PointItemPayloadType.PLAYER_REP;
        }

        // Update equip
        var payload = new PointItemPayload()
        {
            type = type,
            playerId = id,
            rigManager = manager,
        };

        item.OnEquipChanged(payload, isEquipped);

        // Update visibility
        if (manager != null)
        {
            item.OnUpdateObjects(payload, isEquipped);
        }
    }

    internal static void Internal_OnTriggerItem(PlayerId id, string barcode, string value = null)
    {
        if (!TryGetPointItem(barcode, out var item))
        {
            return;
        }

        // Get the rig info
        RigManager manager = null;
        PointItemPayloadType type = PointItemPayloadType.SELF;

        if (id == null || id.IsMe)
        {
            manager = RigData.Refs.RigManager;
            type = PointItemPayloadType.SELF;
        }
        else if (NetworkPlayerManager.TryGetPlayer(id, out var rep))
        {
            manager = rep.RigRefs.RigManager;
            type = PointItemPayloadType.PLAYER_REP;
        }

        // Update equip
        var payload = new PointItemPayload()
        {
            type = type,
            playerId = id,
            rigManager = manager,
        };

        if (value != null)
        {
            item.OnTrigger(payload, value);
        }
        else
        {
            item.OnTrigger(payload);
        }
    }

    public static void SetEquipped(PointItem item, bool isEquipped)
    {
        if (item == null || (!item.IsUnlocked && !item.IsEquipped))
        {
            return;
        }

        Internal_OnEquipChange(PlayerIdManager.LocalId, item.Barcode, isEquipped);
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

    public static IReadOnlyList<PointItem> GetLockedItems(SortMode sort = SortMode.PRICE)
    {
        List<PointItem> items = new(LoadedItems.Count);

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

    public static IReadOnlyList<PointItem> GetUnlockedItems(SortMode sort = SortMode.PRICE)
    {
        List<PointItem> items = new(LoadedItems.Count);

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

    private static void SortBy(ref List<PointItem> items, SortMode sort)
    {
        switch (sort)
        {
            case SortMode.PRICE:
                items.Sort((x, y) => x.AdjustedPrice - y.AdjustedPrice);
                break;
            case SortMode.TAG:
                items.Sort((x, y) => x.MainTag.CompareTo(y.MainTag));
                break;
            case SortMode.NAME:
                items.Sort((x, y) => x.Title.CompareTo(y.Title));
                break;
            case SortMode.AUTHOR:
                items.Sort((x, y) => x.Author.CompareTo(y.Author));
                break;
        }
    }

    public static IReadOnlyList<PointItem> LoadedItems => PointItems;

    internal static readonly List<PointItem> PointItems = new();
    internal static readonly FusionDictionary<string, PointItem> PointItemLookup = new();
}