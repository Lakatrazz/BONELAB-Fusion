using Il2CppSLZ.Marrow;

using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Marrow.Combat;

public static class NetworkGunManager
{
    private static AmmoInventory _networkAmmoInventory = null;
    public static AmmoInventory NetworkAmmoInventory => _networkAmmoInventory;

    public static void OnMainSceneInitialized()
    {
        var playerInventory = AmmoInventory.Instance;

        if (playerInventory == null)
        {
            FusionLogger.Warn("Not creating network AmmoInventory since the singleton AmmoInventory is null!");
            return;
        }

        var inventoryGameObject = new GameObject("Network AmmoInventory");
        inventoryGameObject.SetActive(false);

        _networkAmmoInventory = inventoryGameObject.AddComponent<AmmoInventory>();

        _networkAmmoInventory.lightAmmoGroup = playerInventory.lightAmmoGroup;
        _networkAmmoInventory.mediumAmmoGroup = playerInventory.mediumAmmoGroup;
        _networkAmmoInventory.heavyAmmoGroup = playerInventory.heavyAmmoGroup;

        var count = 10000000;

        _networkAmmoInventory.AddCartridge(_networkAmmoInventory.lightAmmoGroup, count);
        _networkAmmoInventory.AddCartridge(_networkAmmoInventory.heavyAmmoGroup, count);
        _networkAmmoInventory.AddCartridge(_networkAmmoInventory.mediumAmmoGroup, count);

        inventoryGameObject.SetActive(true);
    }
}
