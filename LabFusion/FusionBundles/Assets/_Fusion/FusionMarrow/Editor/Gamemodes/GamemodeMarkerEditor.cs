using System;

using UltEvents;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

namespace LabFusion.Marrow.Integration
{
    [CustomEditor(typeof(GamemodeMarker))]
    public class GamemodeMarkerEditor : Editor
    {
        public const string AddTeamMethodName = nameof(GamemodeMarker.AddTeam);

        private SerializedProperty _teamTagsProperty;

        private void OnEnable()
        {
            _teamTagsProperty = serializedObject.FindProperty(nameof(GamemodeMarker.teamTags));
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var gamemodeMarker = target as GamemodeMarker;

            if (!gamemodeMarker.TryGetComponent<LifeCycleEvents>(out var lifeCycleEvent))
            {
                var warnBox = new HelpBox("If you want to set a specific Team for this Gamemode Marker, please add" +
                    " a LifeCycleEvents to this GameObject!", HelpBoxMessageType.Warning);
                root.Add(warnBox);

                var addLifeCycleEventsButton = new Button(() =>
                {
                    Undo.AddComponent<LifeCycleEvents>(gamemodeMarker.gameObject);
                })
                {
                    text = "Add LifeCycleEvents"
                };
                root.Add(addLifeCycleEventsButton);

                return root;
            }

            var tag = new PropertyField(_teamTagsProperty);
            root.Add(tag);

            var infoBox = new HelpBox("The LifeCycleEvents on this GameObject is used to inject variables for this marker." +
    " Make sure nothing else is using the LifeCycleEvents on this same GameObject.", HelpBoxMessageType.Info);
            root.Add(infoBox);

            root.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                OverrideLifeCycleEvent(gamemodeMarker, lifeCycleEvent);
            });

            return root;
        }

        private void OverrideLifeCycleEvent(GamemodeMarker gamemodeMarker, LifeCycleEvents lifeCycleEvent)
        {
            var ultEvent = new UltEvent();

            Action<string> addTeamAction = gamemodeMarker.AddTeam;

            ultEvent.Clear();

            foreach (var tag in gamemodeMarker.teamTags.Tags)
            {
                var addTeamCall = ultEvent.AddPersistentCall(addTeamAction);

                addTeamCall.PersistentArguments[0].String = tag.Barcode.ID;
            }

            lifeCycleEvent.AwakeEvent = ultEvent;
        }
    }
}