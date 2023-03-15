using LabFusion.Utilities;

using SLZ.Bonelab;
using SLZ.Combat;
using SLZ.Marrow.Pool;
using SLZ.Props.Weapons;
using SLZ.Props;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using UnhollowerRuntimeLib;

using Il2Type = Il2CppSystem.Type;

using SLZ.Rig;

namespace LabFusion.Syncables {
    public static class SyncBlacklist {
        private static readonly Il2Type[] BlacklistedChildComponents = new Il2Type[4] {
            Il2CppType.Of<GetVelocity>(),
            Il2CppType.Of<SpawnFragment>(),
            Il2CppType.Of<ProjectileBalloon>(),
            Il2CppType.Of<AmmoPickup>(),
        };

        private static readonly Il2Type[] BlacklistedParentComponents = new Il2Type[1] {
            Il2CppType.Of<RigManager>(),
        };

        public static bool HasBlacklistedComponents(this GameObject go) {
            // Check children components
            foreach (var type in BlacklistedChildComponents) {
                if (go.GetComponentInChildren(type, true) != null) {
                    return true;
                }
            }

            // Check parent components
            foreach (var type in BlacklistedParentComponents) {
                if (go.GetComponentInParent(type, true) != null) {
                    return true;
                }
            }

            return false;
        }

        public static bool IsSyncWhitelisted(this GameObject go)
        {
            if (HasBlacklistedComponents(go))
                return false;

            // Other hardcoded stuff (probably cleanup later)
            bool hasRigidbody = go.GetComponentInChildren<Rigidbody>(true) != null;

            bool hasGunProperties = go.GetComponentInChildren<FirearmCartridge>(true) == null || go.GetComponentInChildren<Gun>(true) != null;

            bool spawnableProperties = true;

            var assetPoolee = go.GetComponentInChildren<AssetPoolee>();
            if (assetPoolee)
                spawnableProperties = assetPoolee.spawnableCrate.Barcode != CommonBarcodes.BOARD_BARCODE;

            bool isValid = hasRigidbody && hasGunProperties && spawnableProperties;

            return isValid;
        }
    }
}
