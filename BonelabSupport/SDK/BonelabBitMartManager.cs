using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Props;

using LabFusion.Marrow.Pool;
using LabFusion.Marrow.Proxies;
using LabFusion.SDK.Points;

using UnityEngine;

namespace MarrowFusion.Bonelab;

public static class BonelabBitMartManager
{
    public static void Initialize()
    {
        BitMart.ItemPurchased += OnItemPurchased;
    }

    public static void Uninitialize()
    {
        BitMart.ItemPurchased -= OnItemPurchased;
    }

    private static void OnItemPurchased(BitMart bitMart, PointItem item)
    {
        SpawnGacha(bitMart, item.Barcode);
    }

    private static void SpawnGacha(BitMart bitMart, string barcode)
    {
        var itemSpawnPoint = bitMart.ItemSpawnPoint;

        if (itemSpawnPoint == null)
        {
            return;
        }

        var gachaSpawnable = LocalAssetSpawner.CreateSpawnable(BonelabSpawnableReferences.GachaCapsuleReference);

        LocalAssetSpawner.Register(gachaSpawnable);

        LocalAssetSpawner.Spawn(gachaSpawnable, itemSpawnPoint.position, itemSpawnPoint.rotation, (poolee) =>
        {
            var gachaCapsule = poolee.GetComponent<GachaCapsule>();

            if (gachaCapsule == null)
            {
                return;
            }

            gachaCapsule.selectedCrate = new GenericCrateReference(barcode);
            gachaCapsule.SetPreviewMesh();

            // Add a bunch of torque to get the ball out
            var rigidbody = poolee.GetComponentInChildren<Rigidbody>();

            if (rigidbody == null)
            {
                return;
            }

            rigidbody.velocity = itemSpawnPoint.forward * 0.1f;
            rigidbody.angularVelocity = itemSpawnPoint.right * 150f;
        });
    }
}
