#if UNITY_EDITOR

using UnityEditor;
using NeoFPS.CharacterMotion.Behaviours;

namespace NeoFPSEditor.CharacterMotion.Behaviours
{
    [MotionGraphBehaviourEditor(typeof(BlockInteractionBehaviour))]
    public class BlockInteractionBehaviourEditor : MotionGraphBehaviourEditor
    {
        protected override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnEnter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnExit"));
        }
    }
}

#endif
