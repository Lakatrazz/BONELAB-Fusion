#if MELONLOADER
using Il2CppSLZ.Bonelab;

using MelonLoader;
#endif

using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class ScrollButtonProxy : MonoBehaviour
    {
#if MELONLOADER
        public ScrollButtonProxy(IntPtr intPtr) : base(intPtr) { }

        private void Awake()
        {
            var scrollButton = gameObject.AddComponent<ScrollButton>();

            if (transform.name.ToLower().Contains("down"))
            {
                scrollButton.direction = ScrollButton.ScrollButtonDirection.DOWN;
            }
            else
            {
                scrollButton.direction = ScrollButton.ScrollButtonDirection.UP;
            }

            scrollButton.button = GetComponent<Button>();

            scrollButton.incremental = true; // Make sure this is incremental so its constant steps

            scrollButton.scrollFrequency = 1f;
            scrollButton.signedStepSize = 1f;
            scrollButton.stepSize = 1f;

            var grid = GetComponentInParent<ScrollRect>().transform.Find("Viewport/Content");
            var elementsContainer = grid.GetComponent<ScrollElementsContainer>();

            if (elementsContainer == null)
            {
                elementsContainer = grid.gameObject.AddComponent<ScrollElementsContainer>();
            }

            scrollButton.scrollElementsContainer = elementsContainer;
            scrollButton.scrollbar = transform.parent.GetComponentInChildren<Scrollbar>();
        }
#endif
    }
}