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

        public static void SortItems() {
            PointItems.Sort((x, y) => x.Price - y.Price);
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

        public static IReadOnlyList<PointItem> GetLockedItems() {
            List<PointItem> items = new List<PointItem>(LoadedItems.Count);

            foreach (var item in LoadedItems) {
                if (!item.IsUnlocked)
                    items.Add(item);
            }

            return items;
        }

        public static IReadOnlyList<PointItem> GetUnlockedItems() {
            List<PointItem> items = new List<PointItem>(LoadedItems.Count);

            foreach (var item in LoadedItems) {
                if (item.IsUnlocked)
                    items.Add(item);
            }

            return items;
        }

        public static IReadOnlyList<PointItem> LoadedItems => PointItems;

        internal static readonly List<PointItem> PointItems = new List<PointItem>();
        internal static readonly Dictionary<string, PointItem> PointItemLookup = new Dictionary<string, PointItem>();
    }
}
