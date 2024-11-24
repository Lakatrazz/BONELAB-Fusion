using System;

using UltEvents;

using UnityEditor;
using UnityEngine;

namespace LabFusion.Marrow.Integration
{
    [CustomEditor(typeof(GamemodeTeamSettings))]
    public class GamemodeTeamSettingsEditor : Editor
    {
        public const string LavaGangBarcode = "Lakatrazz.FusionContent.BoneTag.TeamLavaGang";

        public const string SabrelakeBarcode = "Lakatrazz.FusionContent.BoneTag.TeamSabrelake";

        private SerializedProperty _teamOverridesProperty;

        public void OnEnable()
        {
            _teamOverridesProperty = serializedObject.FindProperty(nameof(GamemodeTeamSettings.teamOverrides));
        }

        public override void OnInspectorGUI()
        {
            var teamSettings = target as GamemodeTeamSettings;

            var lifeCycleEvent = teamSettings.GetComponent<LifeCycleEvents>();

            if (lifeCycleEvent != null)
            {
                OverrideLifeCycleEvent(teamSettings, lifeCycleEvent);

                EditorGUILayout.HelpBox("The LifeCycleEvents on this GameObject is used to inject variables for these settings." +
                    " Make sure nothing else is using the LifeCycleEvents on this same GameObject.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("If you want to change the team settings, please add" +
                    " a LifeCycleEvents to this GameObject!", MessageType.Warning);

                if (GUILayout.Button("Add LifeCycleEvents"))
                {
                    Undo.AddComponent<LifeCycleEvents>(teamSettings.gameObject);
                }
            }
        }

        private void OverrideLifeCycleEvent(GamemodeTeamSettings teamSettings, LifeCycleEvents lifeCycleEvent)
        {
            // Make sure the awake event is properly set up
            if (lifeCycleEvent.AwakeEvent == null)
            {
                lifeCycleEvent.AwakeEvent = new UltEvent();

                EditorUtility.SetDirty(lifeCycleEvent);
            }

            var awakeEvent = lifeCycleEvent.AwakeEvent;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_teamOverridesProperty);

            if (!HasBarcode(teamSettings, LavaGangBarcode) && GUILayout.Button("Add LavaGang Override"))
            {
                teamSettings.teamOverrides.Add(new GamemodeTeamSettings.TeamOverride()
                {
                    teamBarcode = LavaGangBarcode,
                });

                EditorUtility.SetDirty(teamSettings);

                ApplyChanges();
            }

            if (!HasBarcode(teamSettings, SabrelakeBarcode) && GUILayout.Button("Add Sabrelake Override"))
            {
                teamSettings.teamOverrides.Add(new GamemodeTeamSettings.TeamOverride()
                {
                    teamBarcode = SabrelakeBarcode,
                });

                EditorUtility.SetDirty(teamSettings);

                ApplyChanges();
            }

            if (teamSettings.teamOverrides.Count > 0 && GUILayout.Button("Clear Overrides"))
            {
                teamSettings.teamOverrides.Clear();

                EditorUtility.SetDirty(teamSettings);

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
                ApplyToUltEvent(teamSettings, awakeEvent);

                EditorUtility.SetDirty(lifeCycleEvent);
            }
        }

        private bool HasBarcode(GamemodeTeamSettings teamSettings, string barcode)
        {
            foreach (var teamOverride in teamSettings.teamOverrides)
            {
                if (teamOverride.teamBarcode == barcode)
                {
                    return true;
                }
            }

            return false;
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

                    nameCall.PersistentArguments[0].String = teamOverride.teamBarcode;
                    nameCall.PersistentArguments[1].String = teamOverride.overrideName;
                }

                if (teamOverride.overrideLogo != null)
                {
                    var logoCall = ultEvent.AddPersistentCall(setTeamLogoAction);

                    logoCall.PersistentArguments[0].String = teamOverride.teamBarcode;
                    logoCall.PersistentArguments[1].Object = teamOverride.overrideLogo;
                }
            }
        }
    }
}