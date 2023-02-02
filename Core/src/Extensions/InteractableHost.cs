using LabFusion.Patching;

using SLZ.Interaction;

namespace LabFusion.Extensions
{
    public static class InteractableHostExtensions
    {
        public static void TryDetach(this InteractableHost host) {
            InteractableHostPatches.IgnorePatches = true;

            foreach (var hand in host._hands.ToArray()) {
                hand.TryDetach();
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
