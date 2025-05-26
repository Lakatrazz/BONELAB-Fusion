#if MELONLOADER
using Il2CppInterop.Runtime.Attributes;

using LabFusion.Utilities;

using MelonLoader;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class UIPanel : MonoBehaviour
    {
#if MELONLOADER
        public UIPanel(IntPtr intPtr) : base(intPtr) { }

        private void Awake()
        {
            UIMachineUtilities.CreateLaserCursor(transform.parent, transform, transform.localScale);
        }
#endif
    }
}