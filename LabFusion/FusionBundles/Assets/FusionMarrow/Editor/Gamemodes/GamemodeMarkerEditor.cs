using System;

using UltEvents;

using UnityEditor;
using UnityEngine;

namespace LabFusion.Marrow.Integration
{
    [CustomEditor(typeof(GamemodeMarker))]
    public class GamemodeMarkerEditor : Editor
    {
        public const string AddTeamMethodName = nameof(GamemodeMarker.AddTeam);

        public const string LavaGangBarcode = "Lakatrazz.FusionContent.BoneTag.TeamLavaGang";

        public const string SabrelakeBarcode = "Lakatrazz.FusionContent.BoneTag.TeamSabrelake";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var gamemodeMarker = target as GamemodeMarker;

            var lifeCycleEvent = gamemodeMarker.GetComponent<LifeCycleEvents>();

            if (lifeCycleEvent != null)
            {
                OverrideLifeCycleEvent(gamemodeMarker, lifeCycleEvent);

                EditorGUILayout.HelpBox("The LifeCycleEvents on this GameObject is used to inject variables for this marker." +
                    " Make sure nothing else is using the LifeCycleEvents on this same GameObject.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("If you want to set a specific Team for this Gamemode Marker, please add" +
                    " a LifeCycleEvents to this GameObject!", MessageType.Warning);

                if (GUILayout.Button("Add LifeCycleEvents"))
                {
                    Undo.AddComponent<LifeCycleEvents>(gamemodeMarker.gameObject);
                }
            }
        }

        private void OverrideLifeCycleEvent(GamemodeMarker gamemodeMarker, LifeCycleEvents lifeCycleEvent)
        {
            // Make sure the awake event is properly set up
            if (lifeCycleEvent.AwakeEvent == null)
            {
                lifeCycleEvent.AwakeEvent = new UltEvent();

                EditorUtility.SetDirty(lifeCycleEvent);
            }

            var awakeEvent = lifeCycleEvent.AwakeEvent;

            Action<string> addTeamAction = gamemodeMarker.AddTeam;

            // Add a persistent call
            if (awakeEvent.PersistentCallsList == null || awakeEvent.PersistentCallsList.Count != 1)
            {
                awakeEvent.Clear();

                awakeEvent.AddPersistentCall(addTeamAction);

                EditorUtility.SetDirty(lifeCycleEvent);
            }

            var firstCall = awakeEvent.PersistentCallsList[0];

            // First call isn't AddTeam, change it
            if (firstCall.MethodName != AddTeamMethodName)
            {
                firstCall.SetMethod(addTeamAction);

                EditorUtility.SetDirty(lifeCycleEvent);
            }

            var barcode = firstCall.PersistentArguments[0].String;

            EditorGUI.BeginChangeCheck();

            barcode = EditorGUILayout.TextField("Team Tag", barcode);

            if (barcode != LavaGangBarcode && GUILayout.Button("Set Team LavaGang"))
            {
                barcode = LavaGangBarcode;
            }

            if (barcode != SabrelakeBarcode && GUILayout.Button("Set Team Sabrelake"))
            {
                barcode = SabrelakeBarcode;
            }

            if (!string.IsNullOrWhiteSpace(barcode) && GUILayout.Button("Clear Team"))
            {
                barcode = null;
            }

            // Override the life cycle event value
            if (EditorGUI.EndChangeCheck())
            {
                firstCall.SetArguments(barcode);

                EditorUtility.SetDirty(lifeCycleEvent);
            }
        }
    }
}