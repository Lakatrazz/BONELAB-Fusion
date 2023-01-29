using LabFusion.Patching;

using SLZ.Interaction;

namespace LabFusion.Extensions
{
    public static class InteractableHostExtensions
    {
        public static void TryDetach(this InteractableHost host) {
            InteractableHostPatches.IgnorePatches = true;
            host.ForceDetach();
            InteractableHostPatches.IgnorePatches = false;
        }

        public static void TryDetach(this IGrippable host) {
            InteractableHostPatches.IgnorePatches = true;
            host.ForceDetach();
            InteractableHostPatches.IgnorePatches = false;
        }
    }
}
