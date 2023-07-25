using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;
using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public enum PointItemPayloadType {
        SELF = 0,
        MIRROR = 1,
        PLAYER_REP = 2,
    }

    public struct PointItemPayload {
        public PointItemPayloadType type;
        public RigManager rigManager;
        public Mirror mirror;
        public PlayerId playerId;
    }

    public sealed class PointItemUpgrade {
        public string Description { get; }

        public int Price { get; }

        public string PurchasedDescription { get; }

        public PointItemUpgrade(string description, int price, string purchasedDescription = null) {
            Description = description;

            Price = price;

            if (purchasedDescription == null)
                PurchasedDescription = description;
            else
                PurchasedDescription = purchasedDescription;
        }
    }

    public abstract class PointItem {
        // The title of the item
        public abstract string Title { get; }

        // The author of the item
        public abstract string Author { get; }

        // The tags of the item. The first tag is shown in the shop after the price. (Optional)
        public virtual string[] Tags => null;

        // The upgrades of the item. UpgradeLevel of -1 is no upgrades, 0 is first upgrade. (Optional)
        public virtual PointItemUpgrade[] Upgrades => null;

        // The version of the item
        public virtual string Version => "1.0.0";

        // The description of the item
        public abstract string Description { get; }

        // The barcode pointing to the item
        public virtual string Barcode => $"{Author}.{Title}.Item";

        // Should this item be hidden in the point shop?
        public virtual bool Redacted => false;

        // The price of the item in bits (currency)
        public abstract int Price { get; }

        // The adjusted price based on the economy. This cannot be overriden.
        public int AdjustedPrice => BitEconomy.ConvertPrice(Price);

        // The active target price, whether it be for the next upgrade or for the regular purchase.
        public int ActivePrice { 
            get {
                if (IsUnlocked)
                {
                    if (NextUpgrade != null)
                        return BitEconomy.ConvertPrice(NextUpgrade.Price);

                    if (CurrentUpgrade != null)
                        return BitEconomy.ConvertPrice(CurrentUpgrade.Price);
                }

                return AdjustedPrice;
            }
        }

        // The active target description, whether it be for the next upgrade or for the regular purchase.
        public string ActiveDescription { 
            get {
                if (IsUnlocked) {
                    if (NextUpgrade != null)
                        return NextUpgrade.Description;

                    if (CurrentUpgrade != null)
                        return CurrentUpgrade.PurchasedDescription;
                }

                return Description;
            }
        }

        // The rarity level of the item.
        public virtual RarityLevel Rarity => RarityLevel.White;

        // The preview image of the item in the menu. (Optional)
        public virtual Texture2D PreviewImage => null;

        // Can the item be equipped?
        public virtual bool CanEquip => true;

        // Hook implementations
        public virtual bool ImplementUpdate => false;
        public virtual bool ImplementFixedUpdate => false;
        public virtual bool ImplementLateUpdate => false;

        public PointItemUpgrade CurrentUpgrade { 
            get {
                if (Upgrades == null || UpgradeLevel <= -1)
                    return null;

                return Upgrades[UpgradeLevel];
            } 
        }

        public PointItemUpgrade NextUpgrade {
            get {
                if (IsMaxUpgrade)
                    return null;

                return Upgrades[UpgradeLevel + 1];
            }
        }

        public bool IsMaxUpgrade => UpgradeLevel >= UpgradeCount - 1;

        public int UpgradeLevel 
        { 
            get {
                if (Upgrades == null || Upgrades.Length <= 0)
                    return -1;

                return Mathf.Min(PointSaveManager.GetUpgradeLevel(Barcode), Upgrades.Length - 1);
            } 
        }

        public int UpgradeCount {
            get {
                if (Upgrades == null || Upgrades.Length <= 0)
                    return -1;

                return Upgrades.Length;
            }
        }

        public bool IsUnlocked => PointSaveManager.IsUnlocked(Barcode);

        public bool IsEquipped => PointSaveManager.IsEquipped(Barcode);

        public string MainTag => Tags == null || Tags.Length <= 0 ? "Misc" : Tags[0];

        protected List<ulong> _shownPlayers = new();
        public IReadOnlyList<ulong> ShownPlayers => _shownPlayers;

        internal void Register() {
            if (ImplementFixedUpdate)
                MultiplayerHooking.OnFixedUpdate += OnFixedUpdate;

            if (ImplementUpdate)
                MultiplayerHooking.OnUpdate += OnUpdate;

            if (ImplementLateUpdate)
                MultiplayerHooking.OnLateUpdate += OnLateUpdate;

            OnRegistered();
        }

        public virtual void OnRegistered() { }

        internal void Unregister() {
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

        public void Trigger() {
            PointItemManager.Internal_OnTriggerItem(PlayerIdManager.LocalId, Barcode);
            PointItemSender.SendPointItemTrigger(Barcode);
        }

        public void Trigger(string value)
        {
            PointItemManager.Internal_OnTriggerItem(PlayerIdManager.LocalId, Barcode, value);
            PointItemSender.SendPointItemTrigger(Barcode, value);
        }

        public virtual void OnTrigger(PointItemPayload payload) { }

        public virtual void OnTrigger(PointItemPayload payload, string value) { }
    }
}
