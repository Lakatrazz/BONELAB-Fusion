using System;

using UltEvents;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

namespace LabFusion.Marrow.Integration
{
    [CustomEditor(typeof(GamemodeMusicSettings))]
    public class GamemodeMusicSettingsEditor : Editor
    {
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

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var musicSettings = target as GamemodeMusicSettings;

            if (!musicSettings.TryGetComponent<LifeCycleEvents>(out var lifeCycleEvent))
            {
                var warnBox = new HelpBox("If you want to change the music settings, please add" +
                    " a LifeCycleEvents to this GameObject!", HelpBoxMessageType.Warning);
                root.Add(warnBox);

                var addLifeCycleEventsButton = new Button(() =>
                {
                    Undo.AddComponent<LifeCycleEvents>(musicSettings.gameObject);
                })
                {
                    text = "Add LifeCycleEvents"
                };
                root.Add(addLifeCycleEventsButton);

                return root;
            }

            var teamOverrides = new PropertyField(_teamOverridesProperty);
            root.Add(teamOverrides);

            var songOverrides = new PropertyField(_songOverridesProperty);
            root.Add(songOverrides);

            var victorySongOverride = new PropertyField(_victorySongOverrideProperty);
            root.Add(victorySongOverride);

            var failureSongOverride = new PropertyField(_failureSongOverrideProperty);
            root.Add(failureSongOverride);

            var tieSongOverride = new PropertyField(_tieSongOverrideProperty);
            root.Add(tieSongOverride);

            var infoBox = new HelpBox("The LifeCycleEvents on this GameObject is used to inject variables for these settings." +
    " Make sure nothing else is using the LifeCycleEvents on this same GameObject.", HelpBoxMessageType.Info);
            root.Add(infoBox);

            root.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                OverrideLifeCycleEvent(musicSettings, lifeCycleEvent);
            }, TrickleDown.TrickleDown);

            return root;
        }

        private void OverrideLifeCycleEvent(GamemodeMusicSettings musicSettings, LifeCycleEvents lifeCycleEvent)
        {
            var ultEvent = new UltEvent();

            ApplyToUltEvent(musicSettings, ultEvent);

            lifeCycleEvent.AwakeEvent = ultEvent;
        }

        private void ApplyToUltEvent(GamemodeMusicSettings musicSettings, UltEvent ultEvent)
        {
            ultEvent.Clear();

            ApplyTeamOverrides(musicSettings, ultEvent);
            ApplySongOverrides(musicSettings, ultEvent);
            ApplyIndividualSongs(musicSettings, ultEvent);
        }

        private void ApplyTeamOverrides(GamemodeMusicSettings musicSettings, UltEvent ultEvent)
        {
            Action<string, string> setVictorySongAction = musicSettings.SetVictorySong;
            Action<string, string> setFailureSongAction = musicSettings.SetFailureSong;

            foreach (var teamOverride in musicSettings.teamOverrides)
            {
                if (teamOverride.victorySongOverride.IsValid())
                {
                    var nameCall = ultEvent.AddPersistentCall(setVictorySongAction);

                    nameCall.PersistentArguments[0].String = teamOverride.teamTag.Barcode.ID;
                    nameCall.PersistentArguments[1].String = teamOverride.victorySongOverride.Barcode.ID;
                }

                if (teamOverride.failureSongOverride.IsValid())
                {
                    var logoCall = ultEvent.AddPersistentCall(setFailureSongAction);

                    logoCall.PersistentArguments[0].String = teamOverride.teamTag.Barcode.ID;
                    logoCall.PersistentArguments[1].String = teamOverride.failureSongOverride.Barcode.ID;
                }
            }
        }

        private void ApplySongOverrides(GamemodeMusicSettings musicSettings, UltEvent ultEvent)
        {
            Action<string> addSongAction = musicSettings.AddSong;

            foreach (var songOverride in musicSettings.songOverrides)
            {
                if (songOverride.IsValid())
                {
                    var songCall = ultEvent.AddPersistentCall(addSongAction);

                    songCall.PersistentArguments[0].String = songOverride.Barcode.ID;
                }
            }
        }

        private void ApplyIndividualSongs(GamemodeMusicSettings musicSettings, UltEvent ultEvent)
        {
            Action<string> setVictorySongAction = musicSettings.SetVictorySong;
            Action<string> setFailureSongAction = musicSettings.SetFailureSong;
            Action<string> setTieSongAction = musicSettings.SetTieSong;

            if (musicSettings.victorySongOverride.IsValid())
            {
                var songCall = ultEvent.AddPersistentCall(setVictorySongAction);

                songCall.PersistentArguments[0].String = musicSettings.victorySongOverride.Barcode.ID;
            }

            if (musicSettings.failureSongOverride.IsValid())
            {
                var songCall = ultEvent.AddPersistentCall(setFailureSongAction);

                songCall.PersistentArguments[0].String = musicSettings.failureSongOverride.Barcode.ID;
            }

            if (musicSettings.tieSongOverride.IsValid())
            {
                var songCall = ultEvent.AddPersistentCall(setTieSongAction);

                songCall.PersistentArguments[0].String = musicSettings.tieSongOverride.Barcode.ID;
            }
        }
    }
}