#if !MELONLOADER
using UnityEditor;
using UnityEngine;

namespace LabFusion.MarrowIntegration {
    [CustomEditor(typeof(FusionMarrowBehaviour), editorForChildClasses: true)]
    public class FusionMarrowBehaviourEditor : Editor {
        public override void OnInspectorGUI()
        {
            var behaviour = target as FusionMarrowBehaviour;
            if (behaviour.Comment != null) {
                EditorGUILayout.HelpBox(behaviour.Comment, MessageType.Info);
            }

            base.OnInspectorGUI();
        }
    }
}
#endif