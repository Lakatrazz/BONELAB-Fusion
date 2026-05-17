using LabFusion.Player;

using UnityEngine;

namespace LabFusion.SDK.Points;

using Math = System.Math;

public interface IPointItem
{
    string Title { get; }

    string Author { get; }

    string Category { get => "Fusion Content"; }

    string[] Tags { get => null; }

    PointItemUpgrade[] Upgrades { get => null; }

    string Version { get => "1.0.0"; }

    string Description { get; }

    string Barcode { get => $"{Author}.{Title}.Item"; }

    bool Redacted { get => false; }

    int Price { get; }

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

    bool Equippable { get => true; }

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

    public string MainTag => Tags == null || Tags.Length <= 0 ? "Misc" : Tags[0];

    public RarityLevel Rarity => PointItemManager.CalculateLevel(CurrentPrice);

    void LoadIcon(Action<Texture2D> loadCallback);

    void OnRegistered();

    void OnUnregistered();

    void OnEquipChanged(PlayerID playerID, bool equipped);
}
