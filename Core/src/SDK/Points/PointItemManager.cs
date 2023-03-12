using LabFusion.BoneMenu;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using SLZ.Rig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace LabFusion.SDK.Points {
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

    public enum SortMode {
        PRICE,
        NAME,
        TAG,
        AUTHOR,
        RARITY,
        EQUIPPED,
        UNEQUIPPED,
        LAST_SORT,
    }

    public static class PointItemManager {
        public static event Action OnBitCountChanged = null;

        public static Color ParseColor(RarityLevel level) {
            switch (level) {
                case RarityLevel.Gray:
                    return Color.gray;
                default:
                case RarityLevel.White:
                    return Color.white;
                case RarityLevel.Blue:
                    return Color.blue;
                case RarityLevel.Green:
                    return Color.green;
                case RarityLevel.Orange:
                    return new Color(1f, 0.647f, 0f);
                case RarityLevel.LightRed:
                    return new Color(1f, 0.447f, 0.462f);
                case RarityLevel.Pink:
                    return new Color(1f, 0.411f, 0.7f);
                case RarityLevel.LightPurple:
                    return new Color(0.796f, 0.764f, 0.89f);
                case RarityLevel.Lime:
                    return new Color(0.749f, 1f, 0f);
                case RarityLevel.Yellow:
                    return Color.yellow;
                case RarityLevel.Cyan:
                    return Color.cyan;
                case RarityLevel.Red:
                    return Color.red;
                case RarityLevel.Purple:
                    return new Color(0.5f, 0f, 0.5f);
            }
        }

        internal static void Internal_HookAssemblies()
        {
            AppDomain.CurrentDomain.AssemblyLoad += Internal_AssemblyLoad;

            MultiplayerHooking.OnLocalPlayerCreated += Internal_OnLocalPlayerCreated;
            MultiplayerHooking.OnPlayerRepCreated += Internal_OnPlayerRepCreated;
        }

        internal static void Internal_UnhookAssemblies()
        {
            AppDomain.CurrentDomain.AssemblyLoad -= Internal_AssemblyLoad;

            MultiplayerHooking.OnLocalPlayerCreated -= Internal_OnLocalPlayerCreated;
            MultiplayerHooking.OnPlayerRepCreated -= Internal_OnPlayerRepCreated;
        }

        private static void Internal_OnLocalPlayerCreated(RigManager rigManager) {
            foreach (var item in LoadedItems) {
                if (item.IsEquipped) {
                    item.OnUpdateObjects(new PointItemPayload()
                    {
                        type = PointItemPayloadType.SELF,
                        rigManager = rigManager,
                        playerId = PlayerIdManager.LocalId,
                    }, true);
                }
            }
        }

        private static void Internal_OnPlayerRepCreated(RigManager rigManager)
        {
            if (!PlayerRepManager.TryGetPlayerRep(rigManager, out var rep))
                return;

            foreach (var item in LoadedItems)
            {
                if (rep.PlayerId.EquippedItems.Contains(item.Barcode)) {
                    item.OnUpdateObjects(new PointItemPayload()
                    {
                        type = PointItemPayloadType.PLAYER_REP,
                        rigManager = rigManager,
                        playerId = rep.PlayerId,
                    }, true);
                }
            }
        }

        private static void Internal_AssemblyLoad(object sender, AssemblyLoadEventArgs args) {
            LoadItems(args.LoadedAssembly);
        }

        public static void LoadItems(Assembly assembly)
        {
            if (assembly == null)
                throw new NullReferenceException("Tried loading point items from a null assembly!");

            AssemblyUtilities.LoadAllValid<PointItem>(assembly, RegisterPointItem);
        }

        public static void RegisterPointItem<T>() where T : PointItem => RegisterPointItem(typeof(T));

        private static void RegisterPointItem(Type type)
        {
            var item = Activator.CreateInstance(type) as PointItem;

            if (PointItemLookup.ContainsKey(item.Barcode))
                throw new ArgumentException($"Point Item with barcode {item.Barcode} was already registered.");
            else {
                PointItems.Add(item);
                PointItemLookup.Add(item.Barcode, item);

                item.Register();

                if (item.IsEquipped)
                    Internal_OnEquipChange(PlayerIdManager.LocalId, item.Barcode, true);
            }
        }

        public static bool TryGetPointItem(string barcode, out PointItem item) {
            if (barcode == null) {
                item = null;
                return false;
            }

            return PointItemLookup.TryGetValue(barcode, out item);
        }

        public static int GetBitCount() {
            return PointSaveManager.GetBitCount();
        }

        public static void RewardBits(int bits) {
            // Make sure the amount isn't invalid
            if (bits.IsNaN()) {
                FusionLogger.ErrorLine("Prevented attempt to give invalid bit reward. Please notify a Fusion developer and send them your log.");
                return;
            }

            var currentBits = GetBitCount();
            PointSaveManager.SetBitCount(currentBits + bits);

            OnBitCountChanged.InvokeSafe("executing OnBitCountChanged");
        }

        public static void DecrementBits(int bits) {
            // Make sure the amount isn't invalid
            if (bits.IsNaN()) {
                FusionLogger.ErrorLine("Prevented attempt to remove an invalid bit amount. Please notify a Fusion developer and send them your log.");
                return;
            }

            var currentBits = GetBitCount();
            PointSaveManager.SetBitCount(currentBits - bits);

            OnBitCountChanged.InvokeSafe("executing OnBitCountChanged");
        }

        public static bool TryBuyItem(PointItem item) {
            var unlockedItems = GetUnlockedItems();

            if (unlockedItems.Contains(item))
                return false;

            int price = item.AdjustedPrice;
            int bits = GetBitCount();

            if (price < 0)
                return false;

            if (price > bits)
                return false;

            PointSaveManager.UnlockItem(item.Barcode);
            int newBits = bits - price;
            PointSaveManager.SetBitCount(newBits);

            return true;
        }

        internal static void Internal_OnEquipChange(PlayerId id, string barcode, bool isEquipped) {
            if (TryGetPointItem(barcode, out var item)) {
                // Get the rig info
                RigManager manager = null;
                PointItemPayloadType type = PointItemPayloadType.SELF;

                if (id == null || id.IsSelf) {
                    manager = RigData.RigReferences.RigManager;
                    type = PointItemPayloadType.SELF;
                }
                else if (PlayerRepManager.TryGetPlayerRep(id, out var rep)) {
                    manager = rep.RigReferences.RigManager;
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
                if (manager != null) {
                    item.OnUpdateObjects(payload, isEquipped);
                }
            }
        }

        internal static void Internal_OnTriggerItem(PlayerId id, string barcode, string value = null)
        {
            if (TryGetPointItem(barcode, out var item))
            {
                // Get the rig info
                RigManager manager = null;
                PointItemPayloadType type = PointItemPayloadType.SELF;

                if (id == null || id.IsSelf)
                {
                    manager = RigData.RigReferences.RigManager;
                    type = PointItemPayloadType.SELF;
                }
                else if (PlayerRepManager.TryGetPlayerRep(id, out var rep))
                {
                    manager = rep.RigReferences.RigManager;
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
                    item.OnTrigger(payload, value);
                else
                    item.OnTrigger(payload);
            }
        }

        public static void SetEquipped(PointItem item, bool isEquipped) {
            if (item == null || !item.IsUnlocked)
                return;

            Internal_OnEquipChange(PlayerIdManager.LocalId, item.Barcode, isEquipped);
            PointSaveManager.SetEquipped(item.Barcode, isEquipped);
            PointItemSender.SendPointItemEquip(item.Barcode, isEquipped);
        }

        public static void UnequipAll() {
            foreach (var item in LoadedItems) {
                SetEquipped(item, false);
            }
        }

        public static IReadOnlyList<PointItem> GetLockedItems(SortMode sort = SortMode.PRICE) {
            List<PointItem> items = new List<PointItem>(LoadedItems.Count);

            foreach (var item in LoadedItems) {
                if (item.Redacted)
                    continue;

                if ((sort == SortMode.EQUIPPED && !item.IsEquipped) || (sort == SortMode.UNEQUIPPED && item.IsEquipped))
                    continue;

                if (!item.IsUnlocked)
                    items.Add(item);
            }

            SortBy(ref items, sort);

            return items;
        }

        public static IReadOnlyList<PointItem> GetUnlockedItems(SortMode sort = SortMode.PRICE) {
            List<PointItem> items = new List<PointItem>(LoadedItems.Count);

            foreach (var item in LoadedItems) {
                if ((sort == SortMode.EQUIPPED && !item.IsEquipped) || (sort == SortMode.UNEQUIPPED && item.IsEquipped))
                    continue;

                if (item.IsUnlocked)
                    items.Add(item);
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
                case SortMode.RARITY:
                    items.Sort((x, y) => (int)x.Rarity - (int)y.Rarity);
                    break;
            }
        }

        public static IReadOnlyList<PointItem> LoadedItems => PointItems;

        internal static readonly List<PointItem> PointItems = new List<PointItem>();
        internal static readonly Dictionary<string, PointItem> PointItemLookup = new Dictionary<string, PointItem>();
    }
}
