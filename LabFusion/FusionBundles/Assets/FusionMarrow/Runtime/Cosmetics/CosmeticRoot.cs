using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppInterop.Runtime.InteropTypes.Fields;
#else
using UnityEngine.Serialization;
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

        public Il2CppValueField<int> Point;

        public Il2CppValueField<int> Alignment;

        public Il2CppValueField<int> Side;

        public Il2CppValueField<bool> HiddenInView;

        public Il2CppValueField<bool> HiddenInShop;

        public Il2CppValueField<int> RawPrice;

        public Il2CppReferenceField<Texture2D> PreviewIcon;

        public AvatarPoint GetPoint() => (AvatarPoint)Point.Get();

        public AvatarAlignment GetAlignment() => (AvatarAlignment)Alignment.Get();

        public AvatarSide GetSide() => AvatarPointSupport.ValidateSideAndFallback(GetPoint(), (AvatarSide)Side.Get());

        public AvatarAnchor GetAnchor() => new(GetPoint(), GetAlignment(), GetSide());

        public bool GetHiddenInView() => HiddenInView.Get();

        public bool GetHiddenInShop() => HiddenInShop.Get();

        public int GetRawPrice() => RawPrice.Get();

        public Texture2D GetPreviewIcon() => PreviewIcon.Get();

        public string GetAuthorOverride()
        {
            var authorOverride = transform.Find("AuthorOverride");

            if (authorOverride == null || authorOverride.childCount <= 0)
            {
                return null;
            }

            var author = authorOverride.GetChild(0).name;

            return author;
        }
#else
        [FormerlySerializedAs("cosmeticPoint")]
        public AvatarPoint Point = AvatarPoint.Head;

        public AvatarAlignment Alignment = AvatarAlignment.Center;

        public AvatarSide Side = AvatarSide.Center;

        [FormerlySerializedAs("hiddenInView")]
        public bool HiddenInView = false;

        [FormerlySerializedAs("hiddenInShop")]
        public bool HiddenInShop = false;

        [FormerlySerializedAs("rawPrice")]
        public int RawPrice = 100;

        [FormerlySerializedAs("previewIcon")]
        public Texture2D PreviewIcon = null;

        private void OnDrawGizmos()
        {
            var mesh = Resources.Load<Mesh>($"Meshes/{GetMeshName()}");

            if (mesh == null)
            {
                return;
            }

            Gizmos.color = new Color(0f, 1f, 1f, 0.5f);

            Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
        }

        private string GetMeshName()
        {
            string meshName = Point.ToString();

            if (AvatarPointSupport.CheckAlignmentSupported(Point))
            {
                meshName += $"_a{Alignment}";
            }

            if (AvatarPointSupport.CheckSideSupported(Point))
            {
                var validatedSide = AvatarPointSupport.ValidateSideAndFallback(Point, Side);

                meshName += $"_s{validatedSide}";
            }

            return meshName;
        }
#endif
    }
}