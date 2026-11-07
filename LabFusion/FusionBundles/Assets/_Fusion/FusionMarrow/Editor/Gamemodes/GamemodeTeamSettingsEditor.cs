using System;

using UltEvents;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

namespace LabFusion.Marrow.Integration
{
    [CustomEditor(typeof(GamemodeTeamSettings))]
    public class GamemodeTeamSettingsEditor : Editor
    {
        private SerializedProperty _teamOverridesProperty;

        public void OnEnable()
        {
            _teamOverridesProperty = serializedObject.FindProperty(nameof(GamemodeTeamSettings.teamOverrides));
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var teamSettings = target as GamemodeTeamSettings;

            if (!teamSettings.TryGetComponent<LifeCycleEvents>(out var lifeCycleEvent))
            {
                var warnBox = new HelpBox("If you want to change the team settings, please add" +
                    " a LifeCycleEvents to this GameObject!", HelpBoxMessageType.Warning);
                root.Add(warnBox);

                var addLifeCycleEventsButton = new Button(() =>
                {
                    Undo.AddComponent<LifeCycleEvents>(teamSettings.gameObject);
                })
                {
                    text = "Add LifeCycleEvents"
                };
                root.Add(addLifeCycleEventsButton);

                return root;
            }

            var teamOverrides = new PropertyField(_teamOverridesProperty);
            root.Add(teamOverrides);

            var infoBox = new HelpBox("The LifeCycleEvents on this GameObject is used to inject variables for these settings." +
                    " Make sure nothing else is using the LifeCycleEvents on this same GameObject.", HelpBoxMessageType.Info);
            root.Add(infoBox);

            root.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                OverrideLifeCycleEvent(teamSettings, lifeCycleEvent);
            }, TrickleDown.TrickleDown);

            return root;
        }

        private void OverrideLifeCycleEvent(GamemodeTeamSettings teamSettings, LifeCycleEvents lifeCycleEvent)
        {
            var ultEvent = new UltEvent();

            ApplyToUltEvent(teamSettings, ultEvent);

            lifeCycleEvent.AwakeEvent = ultEvent;
        }

        private void ApplyToUltEvent(GamemodeTeamSettings teamSettings, UltEvent ultEvent)
        {
            Action<string, string> setTeamNameAction = teamSettings.SetTeamName;
            Action<string, Texture2D> setTeamLogoAction = teamSettings.SetTeamLogo;

            ultEvent.Clear();

            foreach (var teamOverride in teamSettings.teamOverrides)
            {
                if (!string.IsNullOrWhiteSpace(teamOverride.overrideName))
                {
                    var nameCall = ultEvent.AddPersistentCall(setTeamNameAction);

                    nameCall.PersistentArguments[0].String = teamOverride.teamTag.Barcode.ID;
                    nameCall.PersistentArguments[1].String = teamOverride.overrideName;
                }

                if (teamOverride.overrideLogo != null)
                {
                    var logoCall = ultEvent.AddPersistentCall(setTeamLogoAction);

                    logoCall.PersistentArguments[0].String = teamOverride.teamTag.Barcode.ID;
                    logoCall.PersistentArguments[1].Object = teamOverride.overrideLogo;
                }
            }
        }
    }
}