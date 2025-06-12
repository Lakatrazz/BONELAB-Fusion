namespace LabFusion.Marrow.Pool;

public static class SpawnableBlacklist
{
    /// <summary>
    /// A list of spawnable barcodes that will have networked spawns but not become a networked entity.
    /// </summary>
    public static readonly List<string> ClientSideBarcodes = new();

    /// <summary>
    /// Returns if a spawnable with a certain barcode can still be spawned, but will not create a NetworkEntity.
    /// </summary>
    /// <param name="barcode"></param>
    /// <returns></returns>
    public static bool IsClientSide(string barcode)
    {
        if (ClientSideBarcodes.Contains(barcode))
        {
            return true;
        }

        return false;
    }
}
