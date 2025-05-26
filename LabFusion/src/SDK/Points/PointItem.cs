using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;

using UnityEngine;

using Il2CppSLZ.Marrow;

namespace LabFusion.SDK.Points;

using System;

[AttributeUsage(AttributeTargets.Class)]
public sealed class CompiledPointItemAttribute : Attribute { }

public enum PointItemPayloadType
{
    SELF = 0,
    MIRROR = 1,
    PLAYER_REP = 2,
}

public struct PointItemPayload
{
    public PointItemPayloadType type;
    public RigManager rigManager;
    public Mirror mirror;
    public PlayerID playerId;
}

public sealed class PointItemUpgrade
{
    public string Description { get; }

    public int Price { get; }

    public string PurchasedDescription { get; }

    public PointItemUpgrade(string description, int price, string purchasedDescription = null)
    {
        Description = description;

        Price = price;

        if (purchasedDescription == null)
        {
            PurchasedDescription = description;
        }
        else
        {
            PurchasedDescription = purchasedDescription;
        }
    }
}

public abstract class PointItem
{
    /// <summary>
    /// The display title of this item.
    /// </summary>
    public abstract string Title { get; }

    /// <summary>
    /// The author of this item.
    /// </summary>
    public abstract string Author { get; }

    /// <summary>
    /// The category that this item is contained in.
    /// </summary>
    public virtual string Category => "Fusion Content";

    /// <summary>
    /// The tags of the item. The first tag is shown in the shop after the price. (Optional)
    /// </summary>
    public virtual string[] Tags => null;

    /// <summary>
    /// The upgrades of the item. UpgradeLevel of -1 is no upgrades, 0 is first upgrade. (Optional)
    /// </summary>
    public virtual PointItemUpgrade[] Upgrades => null;

    /// <summary>
    /// The version of the item.
    /// </summary>
    public virtual string Version => "1.0.0";

    /// <summary>
    /// The description of the item.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// The barcode pointing to the item. This is also used for save data, so it should never change.
    /// </summary>
    public virtual string Barcode => $"{Author}.{Title}.Item";

    /// <summary>
    /// Should this item be hidden in the point shop?
    /// </summary>
    public virtual bool Redacted => false;

    /// <summary>
    /// The amount of points required to purchase this item.
    /// </summary>
    public abstract int Price { get; }

    /// <summary>
    /// The current price. If not purchased, this is the regular price. Otherwise, it is the next upgrade's price.
    /// </summary>
    public int CurrentPrice
    {
        get
        {
            if (IsUnlocked)
            {
                if (NextUpgrade != null)
                {
                    return NextUpgrade.Price;
                }

                if (CurrentUpgrade != null)
                {
                    return CurrentUpgrade.Price;
                }
            }

            return Price;
        }
    }

    /// <summary>
    /// The current description. If not purchased, this is the regular description. Otherwise, it is the next upgrade's description.
    /// </summary>
    public string CurrentDescription
    {
        get
        {
            if (IsUnlocked)
            {
                if (NextUpgrade != null)
                    return NextUpgrade.Description;

                if (CurrentUpgrade != null)
                    return CurrentUpgrade.PurchasedDescription;
            }

            return Description;
        }
    }

    /// <summary>
    /// Can this item be equipped?
    /// </summary>
    public virtual bool Equippable => true;

    // Hook implementations
    public virtual bool ImplementUpdate => false;
    public virtual bool ImplementFixedUpdate => false;
    public virtual bool ImplementLateUpdate => false;

    public PointItemUpgrade CurrentUpgrade
    {
        get
        {
            if (Upgrades == null || CurrentUpgradeIndex <= -1)
                return null;

            return Upgrades[CurrentUpgradeIndex];
        }
    }

    public PointItemUpgrade NextUpgrade
    {
        get
        {
            if (IsMaxUpgrade)
                return null;

            return Upgrades[CurrentUpgradeIndex + 1];
        }
    }

    public bool IsMaxUpgrade => CurrentUpgradeIndex >= UpgradeCount - 1;

    public bool HasUpgrades => UpgradeCount > 0;

    public int CurrentUpgradeIndex
    {
        get
        {
            if (Upgrades == null || Upgrades.Length <= 0)
                return -1;

            return Math.Min(PointSaveManager.GetUpgradeLevel(Barcode), Upgrades.Length - 1);
        }
    }

    public int UpgradeCount
    {
        get
        {
            if (Upgrades == null || Upgrades.Length <= 0)
                return -1;

            return Upgrades.Length;
        }
    }

    public virtual bool IsUnlocked => PointSaveManager.IsUnlocked(Barcode);

    public bool IsEquipped => PointSaveManager.IsEquipped(Barcode);
    
    /// <summary>
    /// The main tag of this item. Either the first tag from <see cref="Tags"/>, or Misc if there are no tags.
    /// </summary>
    public string MainTag => Tags == null || Tags.Length <= 0 ? "Misc" : Tags[0];

    public RarityLevel Rarity => PointItemManager.CalculateLevel(CurrentPrice);

    public abstract void LoadPreviewIcon(Action<Texture2D> onLoaded);

    internal void Register()
    {
        if (ImplementFixedUpdate)
            MultiplayerHooking.OnFixedUpdate += OnFixedUpdate;

        if (ImplementUpdate)
            MultiplayerHooking.OnUpdate += OnUpdate;

        if (ImplementLateUpdate)
            MultiplayerHooking.OnLateUpdate += OnLateUpdate;

        OnRegistered();
    }

    public virtual void OnRegistered() { }

    internal void Unregister()
    {
        if (ImplementFixedUpdate)
            MultiplayerHooking.OnFixedUpdate -= OnFixedUpdate;

        if (ImplementUpdate)
            MultiplayerHooking.OnUpdate -= OnUpdate;

        if (ImplementLateUpdate)
            MultiplayerHooking.OnLateUpdate -= OnLateUpdate;

        OnUnregistered();
    }

    public virtual void OnUnregistered() { }

    public virtual void OnFixedUpdate() { }

    public virtual void OnUpdate() { }

    public virtual void OnLateUpdate() { }

    public virtual void OnEquipChanged(PointItemPayload payload, bool isEquipped) { }

    public virtual void OnUpdateObjects(PointItemPayload payload, bool isVisible) { }

    public void Trigger()
    {
        PointItemManager.Internal_OnTriggerItem(PlayerIDManager.LocalID, Barcode);
        PointItemSender.SendPointItemTrigger(Barcode);
    }

    public void Trigger(string value)
    {
        PointItemManager.Internal_OnTriggerItem(PlayerIDManager.LocalID, Barcode, value);
        PointItemSender.SendPointItemTrigger(Barcode, value);
    }

    public virtual void OnTrigger(PointItemPayload payload) { }

    public virtual void OnTrigger(PointItemPayload payload, string value) { }
}