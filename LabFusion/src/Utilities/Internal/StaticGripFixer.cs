using SLZ.Bonelab;
using SLZ.Interaction;
using UnityEngine;

namespace LabFusion.Utilities
{
    internal static class StaticGripFixer
    {
        internal static void OnMainSceneInitialized()
        {
            // Ammo dispenser
            var ammoDispensers = GameObject.FindObjectsOfType<AmmoDispenser>();

            foreach (var dispenser in ammoDispensers)
            {
                dispenser.gameObject.AddComponent<InteractableHost>();
            }
        }
    }
}
