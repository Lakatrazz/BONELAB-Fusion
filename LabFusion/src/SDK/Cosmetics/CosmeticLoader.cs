using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow;

namespace LabFusion.SDK.Cosmetics
{
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
        }
    }
}
