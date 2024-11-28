using System;

using UltEvents;

using UnityEditor;

using UnityEngine;

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

        public override void OnInspectorGUI()
        {
            var playerSettings = target as GamemodePlayerSettings;

            var lifeCycleEvent = playerSettings.GetComponent<LifeCycleEvents>();

            if (lifeCycleEvent != null)
            {
                OverrideLifeCycleEvent(playerSettings, lifeCycleEvent);

                EditorGUILayout.HelpBox("The LifeCycleEvents on this GameObject is used to inject variables for these settings." +
                    " Make sure nothing else is using the LifeCycleEvents on this same GameObject.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("If you want to change the player settings, please add" +
                    " a LifeCycleEvents to this GameObject!", MessageType.Warning);

                if (GUILayout.Button("Add LifeCycleEvents"))
                {
                    Undo.AddComponent<LifeCycleEvents>(playerSettings.gameObject);
                }
            }
        }

        private void OverrideLifeCycleEvent(GamemodePlayerSettings playerSettings, LifeCycleEvents lifeCycleEvent)
        {
            // Make sure the awake event is properly set up
            if (lifeCycleEvent.AwakeEvent == null)
            {
                lifeCycleEvent.AwakeEvent = new UltEvent();

                EditorUtility.SetDirty(lifeCycleEvent);
            }

            var awakeEvent = lifeCycleEvent.AwakeEvent;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_avatarOverrideProperty);

            EditorGUILayout.PropertyField(_vitalityOverrideProperty);

            bool hasOverrides = !string.IsNullOrWhiteSpace(playerSettings.avatarOverride) || playerSettings.vitalityOverride > 0f;

            if (hasOverrides && GUILayout.Button("Clear Overrides"))
            {
                playerSettings.avatarOverride = null;
                playerSettings.vitalityOverride = 0f;

                EditorUtility.SetDirty(playerSettings);

                ApplyChanges();
            }

            // Override the life cycle event value
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                ApplyChanges();
            }

            void ApplyChanges()
            {
                ApplyToUltEvent(playerSettings, awakeEvent);

                EditorUtility.SetDirty(lifeCycleEvent);
            }
        }

        private void ApplyToUltEvent(GamemodePlayerSettings playerSettings, UltEvent ultEvent)
        {
            ultEvent.Clear();

            Action<string> setAvatarAction = playerSettings.SetAvatar;
            Action<float> setVitalityAction = playerSettings.SetVitality;

            if (!string.IsNullOrWhiteSpace(playerSettings.avatarOverride))
            {
                var avatarCall = ultEvent.AddPersistentCall(setAvatarAction);
                avatarCall.PersistentArguments[0].String = playerSettings.avatarOverride;
            }

            if (playerSettings.vitalityOverride > 0f)
            {
                var vitalityCall = ultEvent.AddPersistentCall(setVitalityAction);
                vitalityCall.PersistentArguments[0].Float = playerSettings.vitalityOverride;
            }
        }
    }
}