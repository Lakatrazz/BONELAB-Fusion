using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SLZ.Marrow.Warehouse;
using SLZ.Props;

using LabFusion.Extensions;
using LabFusion.Preferences;

namespace LabFusion.Utilities {
    public static class ConstrainerUtilities {
        public static bool PlayerConstraintsEnabled { get { return FusionPreferences.ActiveServerSettings.PlayerConstraintsEnabled.GetValue(); } }

        public static bool HasConstrainer { get { return GlobalConstrainer != null; } }
        public static Constrainer GlobalConstrainer { get; private set; }

        public static void OnMainSceneInitialized() {
            // Get the constrainer crate so we can create a global constrainer
            var crate = AssetWarehouse.Instance.GetCrate<SpawnableCrate>(CommonBarcodes.CONSTRAINER_BARCODE);
            if (crate != null) {
                // If this was replaced, fix the GameObject GUID
                if (!crate.Pallet.Internal) {
                    crate.MainAsset = new MarrowAsset(CommonBarcodes.CONSTRAINER_ASSET_GUID);
                }

                // Load the asset so we can create it
                crate.LoadAsset((Il2CppSystem.Action<GameObject>)((go) => {
                    // Make sure the GameObject exists
                    if (go == null) {
                        return;
                    }

                    var constrainer = GameObject.Instantiate(go, new Vector3(1000f, 1000f, 1000f), QuaternionExtensions.identity);
                    GlobalConstrainer = constrainer.GetComponent<Constrainer>();
                    constrainer.SetActive(false);
                }));
            }
        }
    }
}
