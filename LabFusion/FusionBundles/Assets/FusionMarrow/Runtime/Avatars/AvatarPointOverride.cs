#if MELONLOADER
using Il2CppInterop.Runtime.InteropTypes.Fields;

using MelonLoader;
#else
using SLZ.Marrow.Warehouse;

using UnityEngine.Serialization;
#endif

using UnityEngine;

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class AvatarPointOverride : MonoBehaviour
    {
#if MELONLOADER
        public AvatarPointOverride(IntPtr intPtr) : base(intPtr) { }

        public Il2CppValueField<int> Point;

        public Il2CppValueField<int> Alignment;

        public Il2CppValueField<int> Side;

        public AvatarPoint GetPoint() => (AvatarPoint)Point.Get();

        public AvatarAlignment GetAlignment() => (AvatarAlignment)Alignment.Get();

        public AvatarSide GetSide() => (AvatarSide)Side.Get();

        public AvatarAnchor GetAnchor() => new(GetPoint(), GetAlignment(), GetSide());
#else
        [FormerlySerializedAs("cosmeticPoint")]
        public AvatarPoint Point = AvatarPoint.Head;

        public AvatarAlignment Alignment = AvatarAlignment.Center;

        public AvatarSide Side = AvatarSide.Center;
#endif

#if UNITY_EDITOR
        public SpawnableCrateReference PreviewCosmetic = new(Barcode.EMPTY);

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
            if (PreviewCosmetic != null && PreviewCosmetic.TryGetCrate(out var crate))
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