#if MELONLOADER
using MelonLoader;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class MenuToolbar : MonoBehaviour
    {
#if MELONLOADER
        public MenuToolbar(IntPtr intPtr) : base(intPtr) { }
#endif
    }
}