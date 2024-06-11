using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        "Fusion",
        "Cosmetic",
    };

    public static void LoadAllCosmetics()
    {
        var cosmeticCrates = CrateFilterer.FilterByTags<SpawnableCrate>(RequiredTags);

        foreach (var crate in cosmeticCrates)
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

        var point = (RigPoint)cosmeticRoot.cosmeticPoint.Get();

        var price = cosmeticRoot.rawPrice.Get();

        var hiddenInView = cosmeticRoot.hiddenInView.Get();

        var variables = new CosmeticVariables()
        {
            title = title,
            description = description,
            author = author,
            tags = tags.ToArray(),
            barcode = barcode,
            cosmeticPoint = point,
            price = price,
            hiddenInView = hiddenInView,
        };

        var cosmeticItem = new CosmeticItem(variables);

        PointItemManager.RegisterPointItem(cosmeticItem);
    }
}
