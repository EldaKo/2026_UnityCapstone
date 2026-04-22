#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM

using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(InputSystemFirearmWithSecondary), true)]
    public class InputSystemFirearmWithSecondaryEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Firearm"));

            NeoFpsEditorGUI.Header("Secondary");
            InspectWeaponInfo(serializedObject.FindProperty("m_Secondary"));

            serializedObject.ApplyModifiedProperties();
        }

        void InspectWeaponInfo(SerializedProperty prop)
        {
            var typeProperty = prop.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(typeProperty);

            switch (typeProperty.enumValueIndex)
            {
                case 0: // Firearm
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("firearm"));
                    break;
                case 1: // WieldableTool
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("wieldableTool"));
                    break;
                case 2: // Melee
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("melee"));
                    break;
                case 3: // Thrown
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("thrown"));
                    break;
            }
        }
    }
}

#endif