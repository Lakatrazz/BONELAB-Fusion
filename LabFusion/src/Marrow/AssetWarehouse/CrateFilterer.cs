using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow;

public static class CrateFilterer
{
    public static TCrate[] FilterByTags<TCrate>(Pallet pallet, params string[] tags) where TCrate : Crate
    {
        List<TCrate> filtered = new();

        foreach (var crate in pallet.Crates)
        {
            var genericCrate = crate.TryCast<TCrate>();

            if (!genericCrate)
            {
                continue;
            }

            if (!HasTags(crate, tags))
            {
                continue;
            }

            filtered.Add(genericCrate);
        }

        return filtered.ToArray();
    }

    public static TCrate[] FilterByTags<TCrate>(params string[] tags) where TCrate : Crate
    {
        var crates = AssetWarehouse.Instance.GetCrates<TCrate>();
        List<TCrate> filtered = new();

        foreach (var crate in crates) 
        {
            if (!HasTags(crate, tags))
            {
                continue;
            }

            filtered.Add(crate);
        }

        return filtered.ToArray();
    }

    public static bool HasTags(Crate crate, params string[] tags)
    {
        foreach (var tag in tags)
        {
            if (!crate.Tags.Contains(tag))
            {
                return false;
            }
        }

        return true;
    }
}