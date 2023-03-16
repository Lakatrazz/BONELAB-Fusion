using LabFusion.Patching;

using SLZ.Interaction;

using System.Collections.Generic;

namespace LabFusion.Extensions
{
    public static class InteractableHostExtensions
    {
        public static void TryDetach(this InteractableHost host) {
            InteractableHostPatches.IgnorePatches = true;

            List<Hand> handsToDetach = null;
            for (var i = 0; i < host._hands.Count; i++) {
                if (handsToDetach == null)
                    handsToDetach = new List<Hand>();

                handsToDetach.Add(host._hands[i]);
            }

            if (handsToDetach != null) {
                for (var i = 0; i < handsToDetach.Count; i++) {
                    handsToDetach[i].TryDetach();
                }
            }

            InteractableHostPatches.IgnorePatches = false;
        }

        public static void TryDetach(this IGrippable host) {
            var interactable = host.TryCast<InteractableHost>();

            if (interactable != null)
                interactable.TryDetach();
        }
    }
}
