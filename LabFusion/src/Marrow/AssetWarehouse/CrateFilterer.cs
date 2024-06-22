using Il2CppSLZ.Marrow.Warehouse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Marrow
{
    public static class CrateFilterer
    {
        public static TCrate[] FilterByTags<TCrate>(params string[] tags) where TCrate : Crate
        {
            var crates = AssetWarehouse.Instance.GetCrates<TCrate>();
            List<TCrate> filtered = new();

            foreach (var crate in crates) 
            {
                bool isValid = true;

                foreach (var tag in tags)
                {
                    if (!crate.Tags.Contains(tag))
                    {
                        isValid = false;
                        break;
                    }
                }

                if (!isValid)
                {
                    continue;
                }

                filtered.Add(crate);
            }

            return filtered.ToArray();
        }
    }
}
