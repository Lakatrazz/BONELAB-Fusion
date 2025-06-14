using Il2CppSLZ.Marrow;

namespace LabFusion.Player;

public static class LocalInventory
{
    /// <summary>
    /// Sets the local player's ammo count for all types.
    /// </summary>
    /// <param name="count">The amount of light, medium, and heavy ammo.</param>
    public static void SetAmmo(int count)
    {
        var ammoInventory = AmmoInventory.Instance;

        if (ammoInventory == null)
        {
            return;
        }

        ammoInventory.ClearAmmo();

        ammoInventory.AddCartridge(ammoInventory.lightAmmoGroup, count);
        ammoInventory.AddCartridge(ammoInventory.heavyAmmoGroup, count);
        ammoInventory.AddCartridge(ammoInventory.mediumAmmoGroup, count);
    }

    /// <summary>
    /// Adds to the local player's ammo count for all types.
    /// </summary>
    /// <param name="count"></param>
    public static void AddAmmo(int count)
    {
        var ammoInventory = AmmoInventory.Instance;

        if (ammoInventory == null)
        {
            return;
        }

        ammoInventory.AddCartridge(ammoInventory.lightAmmoGroup, count);
        ammoInventory.AddCartridge(ammoInventory.heavyAmmoGroup, count);
        ammoInventory.AddCartridge(ammoInventory.mediumAmmoGroup, count);
    }

    /// <summary>
    /// Clears the local player's ammo.
    /// </summary>
    public static void ClearAmmo()
    {
        var ammoInventory = AmmoInventory.Instance;

        if (ammoInventory == null)
        {
            return;
        }

        ammoInventory.ClearAmmo();
    }
}
