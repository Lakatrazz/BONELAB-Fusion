using LabFusion.BoneMenu;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace LabFusion.Points {
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
        }

        internal static void Internal_UnhookAssemblies()
        {
            AppDomain.CurrentDomain.AssemblyLoad -= Internal_AssemblyLoad;
        }

        private static void Internal_AssemblyLoad(object sender, AssemblyLoadEventArgs args) {
            LoadItems(args.LoadedAssembly);
        }

        public static void LoadItems(Assembly assembly)
        {
            if (assembly == null)
                throw new NullReferenceException("Tried loading point items from a null assembly!");

            assembly.GetTypes()
                .Where(type => typeof(PointItem).IsAssignableFrom(type) && !type.IsAbstract)
                .ForEach(type => {
                    try
                    {
                        RegisterPointItem(type);
                    }
                    catch (Exception e)
                    {
                        FusionLogger.LogException("loading point items", e);
                    }
                });
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
            var currentBits = GetBitCount();
            PointSaveManager.SetBitCount(currentBits + bits);
        }

        public static void DecrementBits(int bits) {
            var currentBits = GetBitCount();
            PointSaveManager.SetBitCount(currentBits - bits);
        }

        public static bool TryBuyItem(PointItem item) {
            var unlockedItems = GetUnlockedItems();

            if (unlockedItems.Contains(item))
                return false;

            int price = item.Price;
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

        public static void SetEquipped(PointItem item, bool isEquipped) {
            if (item == null || !item.IsUnlocked)
                return;

            PointSaveManager.SetEquipped(item.Barcode, isEquipped);
        }

        public static IReadOnlyList<PointItem> GetLockedItems(SortMode sort = SortMode.PRICE) {
            List<PointItem> items = new List<PointItem>(LoadedItems.Count);

            foreach (var item in LoadedItems) {
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
                    items.Sort((x, y) => x.Price - y.Price);
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
