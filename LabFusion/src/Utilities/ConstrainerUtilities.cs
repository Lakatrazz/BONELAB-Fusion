using UnityEngine;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Marrow;

namespace LabFusion.Utilities;

public static class ConstrainerUtilities
{
    public static bool PlayerConstraintsEnabled { get { return LobbyInfoManager.LobbyInfo.PlayerConstraints; } }

    public static bool HasConstrainer { get { return GlobalConstrainer != null; } }
    public static Constrainer GlobalConstrainer { get; private set; }

    public static void OnMainSceneInitialized()
    {
        // Get the constrainer crate so we can create a global constrainer
        var crate = CrateFilterer.GetCrate<SpawnableCrate>(new Barcode(CommonBarcodes.CONSTRAINER_BARCODE));

        if (crate == null)
        {
            return;
        }

        // If this was replaced, fix the GameObject GUID
        if (!crate.Pallet.IsInMarrowGame())
        {
            crate.MainAsset = new MarrowAsset(CommonBarcodes.CONSTRAINER_ASSET_GUID);
        }

        // Load the asset so we can create it
        crate.LoadAsset((Il2CppSystem.Action<GameObject>)((go) =>
        {
            // Make sure the GameObject exists
            if (go == null)
            {
                return;
            }

            var constrainer = GameObject.Instantiate(go, new Vector3(1000f, 1000f, 1000f), QuaternionExtensions.identity);
            GlobalConstrainer = constrainer.GetComponent<Constrainer>();
            constrainer.SetActive(false);
        }));
    }
}