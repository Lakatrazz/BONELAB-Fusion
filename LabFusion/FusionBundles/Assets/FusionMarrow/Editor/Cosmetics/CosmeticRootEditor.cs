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

            var avatarPoint = (AvatarPoint)_pointProperty.enumValueFlag;
            var avatarSide = (AvatarSide)_sideProperty.enumValueFlag;

            bool alignmentSupported = AvatarPointSupport.CheckAlignmentSupported(avatarPoint);
            bool sideSupported = AvatarPointSupport.CheckSideSupported(avatarPoint);

            var anchorHeader = new Label("Anchor");
            anchorHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(anchorHeader);

            var point = new PropertyField(_pointProperty);
            root.Add(point);

            var alignment = new PropertyField(_alignmentProperty);
            root.Add(alignment);

            var side = new PropertyField(_sideProperty);
            root.Add(side);

            alignment.style.display = alignmentSupported ? DisplayStyle.Flex : DisplayStyle.None;
            side.style.display = sideSupported ? DisplayStyle.Flex : DisplayStyle.None;

            var invalidSideWarning = new HelpBox(null, HelpBoxMessageType.Warning);
            UpdateInvalidSideWarning(invalidSideWarning, avatarPoint, avatarSide, sideSupported);
            root.Add(invalidSideWarning);

            point.RegisterValueChangeCallback(evt =>
            {
                var avatarSide = (AvatarSide)_sideProperty.enumValueFlag;

                var newAvatarPoint = (AvatarPoint)evt.changedProperty.enumValueFlag;
                var newAlignmentSupported = AvatarPointSupport.CheckAlignmentSupported(newAvatarPoint);
                var newSideSupported = AvatarPointSupport.CheckSideSupported(newAvatarPoint);

                alignment.style.display = newAlignmentSupported ? DisplayStyle.Flex : DisplayStyle.None;
                side.style.display = newSideSupported ? DisplayStyle.Flex : DisplayStyle.None;

                UpdateInvalidSideWarning(invalidSideWarning, newAvatarPoint, avatarSide, sideSupported);
            });

            side.RegisterValueChangeCallback(evt =>
            {
                var avatarPoint = (AvatarPoint)_pointProperty.enumValueFlag;
                var sideSupported = AvatarPointSupport.CheckSideSupported(avatarPoint);

                var newAvatarSide = (AvatarSide)evt.changedProperty.enumValueFlag;

                UpdateInvalidSideWarning(invalidSideWarning, avatarPoint, newAvatarSide, sideSupported);
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

        private void UpdateInvalidSideWarning(HelpBox helpBox, AvatarPoint point, AvatarSide side, bool sideSupported) 
        {
            helpBox.style.display = DisplayStyle.None;

            if (!sideSupported)
            {
                return;
            }

            var validatedSide = AvatarPointSupport.ValidateSideAndFallback(point, side);

            if (validatedSide == side)
            {
                return;
            }

            helpBox.style.display = DisplayStyle.Flex;
            helpBox.text = $"The {point} point does not support the side {side}! The side has defaulted to {validatedSide}!";
        }
    }
}