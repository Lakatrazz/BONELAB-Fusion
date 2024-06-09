using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.UI;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class InfoBoard : MonoBehaviour
    {
#if MELONLOADER
        public InfoBoard(IntPtr intPtr) : base(intPtr) { }

        private void Awake()
        {
            // Setup the info board logic
            InfoBoxHelper.CompleteInfoBoard(gameObject);
        }
#endif
    }
}