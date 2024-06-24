#if MELONLOADER
using MelonLoader;
#else
using SLZ.Marrow.Warehouse;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class AvatarCosmeticPoint : MonoBehaviour
    {
#if MELONLOADER
        public AvatarCosmeticPoint(IntPtr intPtr) : base(intPtr) { }

        public Il2CppValueField<int> cosmeticPoint;
#else
        public RigPoint cosmeticPoint = RigPoint.HEAD;
#endif

#if UNITY_EDITOR
        public SpawnableCrateReference previewCosmetic = new(Barcode.EMPTY);

        public void OnDrawGizmos()
        {
            if (previewCosmetic != null && previewCosmetic.TryGetCrate(out var crate) && crate.PreviewMesh.EditorAsset != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.matrix = transform.localToWorldMatrix;

                Gizmos.DrawMesh(crate.PreviewMesh.EditorAsset);
            }
            else
            {
                Gizmos.color = Color.cyan;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawSphere(Vector3.zero, 0.02f);
            }
        }
#endif
    }
}