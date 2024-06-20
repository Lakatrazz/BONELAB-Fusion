using LabFusion.Patching;

using Il2CppSLZ.Interaction;

namespace LabFusion.Extensions
{
    public static class InteractableHostExtensions
    {
        public static void TryDetach(this InteractableHost host)
        {
            List<Hand> handsToDetach = null;
            for (var i = 0; i < host._hands.Count; i++)
            {
                if (handsToDetach == null)
                    handsToDetach = new List<Hand>();

                handsToDetach.Add(host._hands[i]);
            }

            if (handsToDetach != null)
            {
                for (var i = 0; i < handsToDetach.Count; i++)
                {
                    handsToDetach[i].TryDetach();
                }
            }
        }

        public static void TryDetach(this IGrippable host)
        {
            var interactable = host.TryCast<InteractableHost>();

            if (interactable != null)
                interactable.TryDetach();
        }
    }
}
