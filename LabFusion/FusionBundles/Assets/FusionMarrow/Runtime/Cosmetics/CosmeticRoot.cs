using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppInterop.Runtime.InteropTypes.Fields;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class CosmeticRoot : MonoBehaviour
    {
#if MELONLOADER
        public CosmeticRoot(IntPtr intPtr) : base(intPtr) { }

        public Il2CppValueField<int> cosmeticPoint;

        public Il2CppValueField<bool> hiddenInView;

        public Il2CppValueField<int> rawPrice;

        public Il2CppReferenceField<Texture2D> previewIcon;
#else
        public RigPoint cosmeticPoint = RigPoint.HEAD;

        public bool hiddenInView = false;

        public int rawPrice = 100;

        public Texture2D previewIcon = null;
#endif
    }
}