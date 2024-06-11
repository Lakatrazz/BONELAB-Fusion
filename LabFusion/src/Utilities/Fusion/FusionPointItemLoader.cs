using LabFusion.Data;
using LabFusion.SDK.Points;

using UnityEngine;

namespace LabFusion.Utilities
{
    public class ItemPair
    {
        public WeakAssetReference<GameObject> GameObject { get; private set; } = new();
        public WeakAssetReference<Texture2D> Preview { get; private set; } = new();

        public static ItemPair LoadFromBundle(AssetBundle bundle, string name)
        {
            var itemPair = new ItemPair();

            bundle.LoadPersistentAssetAsync<GameObject>(ResourcePaths.ItemPrefix + name, (v) => { itemPair.GameObject.SetAsset(v); });
            bundle.LoadPersistentAssetAsync<Texture2D>(ResourcePaths.PreviewPrefix + name, (v) => { itemPair.Preview.SetAsset(v); });

            return itemPair;
        }
    }

    public static class FusionPointItemLoader
    {
        public static WeakAssetReference<AssetBundle> ItemBundle { get; private set; } = new();

        private static readonly ItemPair _nullItemPair = new();

        private static readonly string[] _itemNames = new string[] {
            nameof(BitMiner),

            // Special
            nameof(VictoryTrophy),
        };

        private static readonly Dictionary<string, ItemPair> _itemPairs = new();

        private static AssetBundleCreateRequest _itemBundleRequest = null;

        private static void OnBundleCompleted(AsyncOperation operation)
        {
            var bundle = _itemBundleRequest.assetBundle;
            ItemBundle.SetAsset(bundle);

            foreach (var item in _itemNames)
            {
                _itemPairs.Add(item, ItemPair.LoadFromBundle(bundle, item));
            }
        }

        public static void OnBundleLoad()
        {
            _itemBundleRequest = FusionBundleLoader.LoadAssetBundleAsync(ResourcePaths.ItemBundle);

            if (_itemBundleRequest != null)
            {
                _itemBundleRequest.add_completed((Il2CppSystem.Action<AsyncOperation>)OnBundleCompleted);
            }
            else
                FusionLogger.Error("Item bundle failed to load!");
        }

        public static void OnBundleUnloaded()
        {
            // Unload item bundle
            if (ItemBundle.HasAsset)
            {
                ItemBundle.Asset.Unload(true);
                ItemBundle.UnloadAsset();
            }
        }

        public static ItemPair GetPair(string name)
        {
            if (_itemPairs.TryGetValue(name, out var pair))
            {
                return pair;
            }
            else
            {
                return _nullItemPair;
            }
        }
    }
}