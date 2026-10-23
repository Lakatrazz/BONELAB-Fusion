using System;

using UltEvents;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

namespace LabFusion.Marrow.Integration
{
    [CustomEditor(typeof(GamemodePlayerSettings))]
    public class GamemodePlayerSettingsEditor : Editor
    {
        private SerializedProperty _avatarOverrideProperty;
        private SerializedProperty _vitalityOverrideProperty;

        public void OnEnable()
        {
            _avatarOverrideProperty = serializedObject.FindProperty(nameof(GamemodePlayerSettings.avatarOverride));
            _vitalityOverrideProperty = serializedObject.FindProperty(nameof(GamemodePlayerSettings.vitalityOverride));
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var playerSettings = target as GamemodePlayerSettings;

            if (!playerSettings.TryGetComponent<LifeCycleEvents>(out var lifeCycleEvent))
            {
                var warnBox = new HelpBox("If you want to change the player settings, please add" +
                    " a LifeCycleEvents to this GameObject!", HelpBoxMessageType.Warning);
                root.Add(warnBox);

                var addLifeCycleEventsButton = new Button(() =>
                {
                    Undo.AddComponent<LifeCycleEvents>(playerSettings.gameObject);
                })
                {
                    text = "Add LifeCycleEvents"
                };
                root.Add(addLifeCycleEventsButton);

                return root;
            }

            var avatarOverride = new PropertyField(_avatarOverrideProperty);
            root.Add(avatarOverride);

            var vitalityOverride = new PropertyField(_vitalityOverrideProperty);
            root.Add(vitalityOverride);

            var infoBox = new HelpBox("The LifeCycleEvents on this GameObject is used to inject variables for these settings." +
    " Make sure nothing else is using the LifeCycleEvents on this same GameObject.", HelpBoxMessageType.Info);
            root.Add(infoBox);

            root.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                OverrideLifeCycleEvent(playerSettings, lifeCycleEvent);
            }, TrickleDown.TrickleDown);

            return root;
        }

        private void OverrideLifeCycleEvent(GamemodePlayerSettings playerSettings, LifeCycleEvents lifeCycleEvent)
        {
            var ultEvent = new UltEvent();

            ApplyToUltEvent(playerSettings, ultEvent);

            lifeCycleEvent.AwakeEvent = ultEvent;
        }

        private void ApplyToUltEvent(GamemodePlayerSettings playerSettings, UltEvent ultEvent)
        {
            ultEvent.Clear();

            Action<string> setAvatarAction = playerSettings.SetAvatar;
            Action<float> setVitalityAction = playerSettings.SetVitality;

            if (playerSettings.avatarOverride.IsValid())
            {
                var avatarCall = ultEvent.AddPersistentCall(setAvatarAction);
                avatarCall.PersistentArguments[0].String = playerSettings.avatarOverride.Barcode.ID;
            }

            if (playerSettings.vitalityOverride > 0f)
            {
                var vitalityCall = ultEvent.AddPersistentCall(setVitalityAction);
                vitalityCall.PersistentArguments[0].Float = playerSettings.vitalityOverride;
            }
        }
    }
}