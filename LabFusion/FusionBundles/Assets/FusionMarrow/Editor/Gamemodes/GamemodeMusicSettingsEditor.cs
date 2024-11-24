using System;

using UltEvents;

using UnityEditor;

using UnityEngine;
using UnityEngine.TextCore.Text;

namespace LabFusion.Marrow.Integration
{
    [CustomEditor(typeof(GamemodeMusicSettings))]
    public class GamemodeMusicSettingsEditor : Editor
    {
        public const string LavaGangBarcode = "Lakatrazz.FusionContent.BoneTag.TeamLavaGang";

        public const string SabrelakeBarcode = "Lakatrazz.FusionContent.BoneTag.TeamSabrelake";

        private SerializedProperty _teamOverridesProperty;
        private SerializedProperty _songOverridesProperty;

        private SerializedProperty _victorySongOverrideProperty;
        private SerializedProperty _failureSongOverrideProperty;
        private SerializedProperty _tieSongOverrideProperty;

        public void OnEnable()
        {
            _teamOverridesProperty = serializedObject.FindProperty(nameof(GamemodeMusicSettings.teamOverrides));
            _songOverridesProperty = serializedObject.FindProperty(nameof(GamemodeMusicSettings.songOverrides));

            _victorySongOverrideProperty = serializedObject.FindProperty(nameof(GamemodeMusicSettings.victorySongOverride));
            _failureSongOverrideProperty = serializedObject.FindProperty(nameof(GamemodeMusicSettings.failureSongOverride));
            _tieSongOverrideProperty = serializedObject.FindProperty(nameof(GamemodeMusicSettings.tieSongOverride));
        }

        public override void OnInspectorGUI()
        {
            var musicSettings = target as GamemodeMusicSettings;

            var lifeCycleEvent = musicSettings.GetComponent<LifeCycleEvents>();

            if (lifeCycleEvent != null)
            {
                OverrideLifeCycleEvent(musicSettings, lifeCycleEvent);

                EditorGUILayout.HelpBox("The LifeCycleEvents on this GameObject is used to inject variables for these settings." +
                    " Make sure nothing else is using the LifeCycleEvents on this same GameObject.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("If you want to change the music settings, please add" +
                    " a LifeCycleEvents to this GameObject!", MessageType.Warning);

                if (GUILayout.Button("Add LifeCycleEvents"))
                {
                    Undo.AddComponent<LifeCycleEvents>(musicSettings.gameObject);
                }
            }
        }

        private void OverrideLifeCycleEvent(GamemodeMusicSettings musicSettings, LifeCycleEvents lifeCycleEvent)
        {
            // Make sure the awake event is properly set up
            if (lifeCycleEvent.AwakeEvent == null)
            {
                lifeCycleEvent.AwakeEvent = new UltEvent();

                EditorUtility.SetDirty(lifeCycleEvent);
            }

            var awakeEvent = lifeCycleEvent.AwakeEvent;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Teams", EditorStyles.whiteLargeLabel);

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(_teamOverridesProperty);

            if (!HasBarcode(musicSettings, LavaGangBarcode) && GUILayout.Button("Add LavaGang Override"))
            {
                musicSettings.teamOverrides.Add(new GamemodeMusicSettings.TeamOverride()
                {
                    teamBarcode = LavaGangBarcode,
                });

                EditorUtility.SetDirty(musicSettings);

                ApplyChanges();
            }

            if (!HasBarcode(musicSettings, SabrelakeBarcode) && GUILayout.Button("Add Sabrelake Override"))
            {
                musicSettings.teamOverrides.Add(new GamemodeMusicSettings.TeamOverride()
                {
                    teamBarcode = SabrelakeBarcode,
                });

                EditorUtility.SetDirty(musicSettings);

                ApplyChanges();
            }

            if (musicSettings.teamOverrides.Count > 0 && GUILayout.Button("Clear Overrides"))
            {
                musicSettings.teamOverrides.Clear();

                EditorUtility.SetDirty(musicSettings);

                ApplyChanges();
            }

            GUILayout.Space(5);

            EditorGUILayout.LabelField("General", EditorStyles.whiteLargeLabel);

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(_songOverridesProperty);

            EditorGUILayout.PropertyField(_victorySongOverrideProperty);
            EditorGUILayout.PropertyField(_failureSongOverrideProperty);
            EditorGUILayout.PropertyField(_tieSongOverrideProperty);

            // Override the life cycle event value
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                ApplyChanges();
            }

            void ApplyChanges()
            {
                ApplyToUltEvent(musicSettings, awakeEvent);

                EditorUtility.SetDirty(lifeCycleEvent);
            }
        }

        private void ApplyToUltEvent(GamemodeMusicSettings musicSettings, UltEvent ultEvent)
        {
            ultEvent.Clear();

            ApplyTeamOverrides(musicSettings, ultEvent);
            ApplySongOverrides(musicSettings, ultEvent);
            ApplyIndividualSongs(musicSettings, ultEvent);
        }

        private bool HasBarcode(GamemodeMusicSettings musicSettings, string barcode)
        {
            foreach (var teamOverride in musicSettings.teamOverrides)
            {
                if (teamOverride.teamBarcode == barcode)
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyTeamOverrides(GamemodeMusicSettings musicSettings, UltEvent ultEvent)
        {
            Action<string, string> setVictorySongAction = musicSettings.SetVictorySong;
            Action<string, string> setFailureSongAction = musicSettings.SetFailureSong;

            foreach (var teamOverride in musicSettings.teamOverrides)
            {
                if (!string.IsNullOrWhiteSpace(teamOverride.victorySongOverride))
                {
                    var nameCall = ultEvent.AddPersistentCall(setVictorySongAction);

                    nameCall.PersistentArguments[0].String = teamOverride.teamBarcode;
                    nameCall.PersistentArguments[1].String = teamOverride.victorySongOverride;
                }

                if (!string.IsNullOrWhiteSpace(teamOverride.failureSongOverride))
                {
                    var logoCall = ultEvent.AddPersistentCall(setFailureSongAction);

                    logoCall.PersistentArguments[0].String = teamOverride.teamBarcode;
                    logoCall.PersistentArguments[1].String = teamOverride.failureSongOverride;
                }
            }
        }

        private void ApplySongOverrides(GamemodeMusicSettings musicSettings, UltEvent ultEvent)
        {
            Action<string> addSongAction = musicSettings.AddSong;

            foreach (var songOverride in musicSettings.songOverrides)
            {
                if (!string.IsNullOrWhiteSpace(songOverride))
                {
                    var songCall = ultEvent.AddPersistentCall(addSongAction);

                    songCall.PersistentArguments[0].String = songOverride;
                }
            }
        }

        private void ApplyIndividualSongs(GamemodeMusicSettings musicSettings, UltEvent ultEvent)
        {
            Action<string> setVictorySongAction = musicSettings.SetVictorySong;
            Action<string> setFailureSongAction = musicSettings.SetFailureSong;
            Action<string> setTieSongAction = musicSettings.SetTieSong;

            if (!string.IsNullOrWhiteSpace(musicSettings.victorySongOverride))
            {
                var songCall = ultEvent.AddPersistentCall(setVictorySongAction);

                songCall.PersistentArguments[0].String = musicSettings.victorySongOverride;
            }

            if (!string.IsNullOrWhiteSpace(musicSettings.failureSongOverride))
            {
                var songCall = ultEvent.AddPersistentCall(setFailureSongAction);

                songCall.PersistentArguments[0].String = musicSettings.failureSongOverride;
            }

            if (!string.IsNullOrWhiteSpace(musicSettings.tieSongOverride))
            {
                var songCall = ultEvent.AddPersistentCall(setTieSongAction);

                songCall.PersistentArguments[0].String = musicSettings.tieSongOverride;
            }
        }
    }
}