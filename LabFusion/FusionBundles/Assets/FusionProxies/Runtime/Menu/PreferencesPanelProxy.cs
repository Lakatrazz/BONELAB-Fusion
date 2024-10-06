#if MELONLOADER
using Il2CppSLZ.Bonelab;

using MelonLoader;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class PreferencesPanelProxy : MonoBehaviour
    {
#if MELONLOADER
        public PreferencesPanelProxy(IntPtr intPtr) : base(intPtr) { }

        public void PAGESELECT(int i)
        {
            // Get references to the UI Rig
            var uiRig = UIRig.Instance;

            if (uiRig == null)
            {
                return;
            }

            // Open the page
            var panelView = uiRig.popUpMenu.preferencesPanelView;

            panelView.PAGESELECT(i);
        }
#else
        public void PAGESELECT(int i)
        {

        }
#endif
    }
}