using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if MELONLOADER
using MelonLoader;
using LabFusion.SDK.Points;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class BitMart : MonoBehaviour
    {
#if MELONLOADER
        public BitMart(IntPtr intPtr) : base(intPtr) { }

        private void Awake()
        {
            // Setup the bitmart logic
            PointShopHelper.CompleteBitMart(gameObject);
        }
#endif
    }
}