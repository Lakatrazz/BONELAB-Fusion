using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace LabFusion.Marrow.Integration
{
    [CustomEditor(typeof(AvatarPointOverride))]
    public class AvatarPointOverrideEditor : Editor
    {
        private SerializedProperty _pointProperty = null;
        private SerializedProperty _alignmentProperty = null;
        private SerializedProperty _sideProperty = null;

        private SerializedProperty _previewCosmeticProperty = null;

        private void OnEnable()
        {
            _pointProperty = serializedObject.FindProperty(nameof(AvatarPointOverride.Point));
            _alignmentProperty = serializedObject.FindProperty(nameof(AvatarPointOverride.Alignment));
            _sideProperty = serializedObject.FindProperty(nameof(AvatarPointOverride.Side));

            _previewCosmeticProperty = serializedObject.FindProperty(nameof(AvatarPointOverride.PreviewCosmetic));
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

                UpdateInvalidSideWarning(invalidSideWarning, newAvatarPoint, avatarSide, newSideSupported);
            });

            side.RegisterValueChangeCallback(evt =>
            {
                var avatarPoint = (AvatarPoint)_pointProperty.enumValueFlag;
                var sideSupported = AvatarPointSupport.CheckSideSupported(avatarPoint);

                var newAvatarSide = (AvatarSide)evt.changedProperty.enumValueFlag;

                UpdateInvalidSideWarning(invalidSideWarning, avatarPoint, newAvatarSide, sideSupported);
            });

            var previewHeader = new Label("Preview");
            previewHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(previewHeader);

            var previewCosmetic = new PropertyField(_previewCosmeticProperty);
            root.Add(previewCosmetic);

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