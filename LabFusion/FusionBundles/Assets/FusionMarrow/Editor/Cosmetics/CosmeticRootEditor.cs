using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace LabFusion.Marrow.Integration
{
    [CustomEditor(typeof(CosmeticRoot))]
    public class CosmeticRootEditor : Editor
    {
        private SerializedProperty _pointProperty = null;
        private SerializedProperty _alignmentProperty = null;
        private SerializedProperty _sideProperty = null;

        private SerializedProperty _hiddenInViewProperty = null;
        private SerializedProperty _hiddenInShopProperty = null;
        private SerializedProperty _rawPriceProperty = null;
        private SerializedProperty _previewIconProperty = null;

        private void OnEnable()
        {
            _pointProperty = serializedObject.FindProperty(nameof(CosmeticRoot.Point));
            _alignmentProperty = serializedObject.FindProperty(nameof(CosmeticRoot.Alignment));
            _sideProperty = serializedObject.FindProperty(nameof(CosmeticRoot.Side));

            _hiddenInViewProperty = serializedObject.FindProperty(nameof(CosmeticRoot.HiddenInView));
            _hiddenInShopProperty = serializedObject.FindProperty(nameof(CosmeticRoot.HiddenInShop));
            _rawPriceProperty = serializedObject.FindProperty(nameof(CosmeticRoot.RawPrice));
            _previewIconProperty = serializedObject.FindProperty(nameof(CosmeticRoot.PreviewIcon));
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var anchorHeader = new Label("Anchor");
            anchorHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(anchorHeader);

            var point = new PropertyField(_pointProperty);
            root.Add(point);

            var avatarPoint = (AvatarPoint)_pointProperty.enumValueFlag;

            var alignment = new PropertyField(_alignmentProperty);
            root.Add(alignment);

            var side = new PropertyField(_sideProperty);
            root.Add(side);

            alignment.style.display = AvatarPointSupport.CheckAlignmentSupported(avatarPoint) ? DisplayStyle.Flex : DisplayStyle.None;
            side.style.display = AvatarPointSupport.CheckSideSupported(avatarPoint) ? DisplayStyle.Flex : DisplayStyle.None;

            point.RegisterValueChangeCallback(evt =>
            {
                var newAvatarPoint = (AvatarPoint)evt.changedProperty.enumValueFlag;

                alignment.style.display = AvatarPointSupport.CheckAlignmentSupported(newAvatarPoint) ? DisplayStyle.Flex : DisplayStyle.None;
                side.style.display = AvatarPointSupport.CheckSideSupported(newAvatarPoint) ? DisplayStyle.Flex : DisplayStyle.None;
            });

            var shopHeader = new Label("Shop");
            shopHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(shopHeader);

            var hiddenInView = new PropertyField(_hiddenInViewProperty);
            root.Add(hiddenInView);

            var hiddenInShop = new PropertyField(_hiddenInShopProperty);
            root.Add(hiddenInShop);

            var rawPrice = new PropertyField(_rawPriceProperty);
            root.Add(rawPrice);

            var previewIcon = new PropertyField(_previewIconProperty);
            root.Add(previewIcon);

            return root;
        }
    }
}