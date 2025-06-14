using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow;
using LabFusion.Marrow.Integration;
using LabFusion.SDK.Points;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Cosmetics;

public static class CosmeticLoader
{
    public static readonly string[] RequiredTags = new[]
    {
        FusionTags.Fusion,
        FusionTags.Cosmetic,
    };

    public static void OnAssetWarehouseReady()
    {
        // Load initial cosmetics
        LoadAllCosmetics();

        // Hook pallet load event
        var onPalletAdded = OnPalletAdded;
        AssetWarehouse.Instance.OnPalletAdded += onPalletAdded;
    }

    private static void OnPalletAdded(Barcode barcode)
    {
        var hasPallet = AssetWarehouse.Instance.TryGetPallet(barcode, out var pallet);

        if (!hasPallet)
        {
            return;
        }

        var cosmeticCrates = CrateFilterer.FilterByTags<SpawnableCrate>(pallet, RequiredTags);

        LoadAllCosmetics(cosmeticCrates);
    }

    public static void LoadAllCosmetics()
    {
        var cosmeticCrates = CrateFilterer.FilterByTags<SpawnableCrate>(RequiredTags);

        LoadAllCosmetics(cosmeticCrates);
    }

    public static void LoadAllCosmetics(SpawnableCrate[] crates)
    {
        foreach (var crate in crates)
        {
            Action<GameObject> onLoaded = (go) =>
            {
                OnCrateAssetLoaded(go, crate);
            };

            crate.MainGameObject.LoadAsset(onLoaded);
        }
    }

    private static void OnCrateAssetLoaded(GameObject gameObject, SpawnableCrate crate)
    {
        // If an item with this barcode is already loaded, we can just skip it
        if (PointItemManager.TryGetPointItem(crate.Barcode.ID, out _))
        {
            return;
        }

        var cosmeticRoot = gameObject.GetComponent<CosmeticRoot>();

        if (cosmeticRoot == null)
        {
#if DEBUG
            FusionLogger.Warn($"Crate {crate.Title} has cosmetic tags, but no CosmeticRoot!");
#endif
            return;
        }

#if DEBUG
        FusionLogger.Log($"Loaded Cosmetic {crate.Title} from AssetWarehouse");
#endif

        var title = crate.Title;
        var description = crate.Description;
        var barcode = crate.Barcode;

        List<string> tags = crate.Tags.ToArray().ToList();
        tags.RemoveAll((t) => RequiredTags.Contains(t));

        var author = crate.Pallet.Author;

        var pallet = crate.Pallet.Title;

        var point = (RigPoint)cosmeticRoot.cosmeticPoint.Get();

        var price = cosmeticRoot.rawPrice.Get();

        var hiddenInView = cosmeticRoot.hiddenInView.Get();
        var hiddenInShop = cosmeticRoot.hiddenInShop.Get();

        var variables = new CosmeticVariables()
        {
            Title = title,
            Description = description,
            Author = author,
            Category = pallet,
            Tags = tags.ToArray(),
            Barcode = barcode.ID,
            CosmeticPoint = point,
            Price = price,
            HiddenInView = hiddenInView,
            HiddenInShop = hiddenInShop,
        };

        var cosmeticItem = new CosmeticItem(variables);

        PointItemManager.RegisterPointItem(cosmeticItem);
    }
}
