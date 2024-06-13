using LabFusion.Utilities;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Combat;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Rig;
using Il2CppInterop.Runtime;
using Il2CppSLZ.VFX;

using UnityEngine;

using Il2Type = Il2CppSystem.Type;

namespace LabFusion.Syncables
{
    public static class SyncBlacklist
    {
        private static readonly Il2Type[] BlacklistedChildComponents = new Il2Type[4] {
            Il2CppType.Of<GetVelocity>(),
            Il2CppType.Of<SpawnFragment>(),
            Il2CppType.Of<ProjectileBalloon>(),
            Il2CppType.Of<AmmoPickup>(),
        };

        private static readonly Il2Type[] BlacklistedParentComponents = new Il2Type[1] {
            Il2CppType.Of<RigManager>(),
        };

        public static bool HasBlacklistedComponents(this GameObject go)
        {
            // Check children components
            foreach (var type in BlacklistedChildComponents)
            {
                if (go.GetComponentInChildren(type, true) != null)
                {
                    return true;
                }
            }

            // Check parent components
            foreach (var type in BlacklistedParentComponents)
            {
                if (go.GetComponentInParent(type, true) != null)
                {
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

            bool isValid = hasRigidbody && hasGunProperties && spawnableProperties;

            return isValid;
        }
    }
}
