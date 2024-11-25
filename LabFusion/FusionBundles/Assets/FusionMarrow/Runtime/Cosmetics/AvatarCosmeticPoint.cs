#if MELONLOADER
using Il2CppInterop.Runtime.InteropTypes.Fields;

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
            var previewMesh = GetPreviewMesh();

            if (previewMesh != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.matrix = transform.localToWorldMatrix;

                Gizmos.DrawMesh(previewMesh);
            }
            else
            {
                Gizmos.color = Color.cyan;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawSphere(Vector3.zero, 0.02f);
            }
        }

        private Mesh GetPreviewMesh()
        {
            if (previewCosmetic != null && previewCosmetic.TryGetCrate(out var crate))
            {
                var mesh = crate.PreviewMesh.Asset != null ? crate.PreviewMesh.Asset : crate.PreviewMesh.EditorAsset;

                if (mesh == null)
                {
                    return null;
                }

                if (mesh.vertices.Length <= 0 || mesh.normals.Length <= 0)
                {
                    return null;
                }

                return mesh;
            }

            return null;
        }
#endif
    }
}