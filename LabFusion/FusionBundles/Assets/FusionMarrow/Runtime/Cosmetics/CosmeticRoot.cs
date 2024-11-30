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

        public Il2CppValueField<bool> hiddenInShop;

        public Il2CppValueField<int> rawPrice;

        public Il2CppReferenceField<Texture2D> previewIcon;
#else
        public RigPoint cosmeticPoint = RigPoint.HEAD;

        public bool hiddenInView = false;

        public bool hiddenInShop = false;

        public int rawPrice = 100;

        public Texture2D previewIcon = null;

        private void OnDrawGizmos()
        {
            var mesh = Resources.Load<Mesh>($"Meshes/{cosmeticPoint}");

            if (mesh == null)
            {
                return;
            }

            Gizmos.color = new Color(0f, 1f, 1f, 0.5f);

            Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
        }
#endif
    }
}