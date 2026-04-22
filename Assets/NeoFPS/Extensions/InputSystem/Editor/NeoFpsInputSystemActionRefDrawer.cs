#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM

using UnityEngine;
using UnityEditor;
using NeoFPS;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace NeoFPSEditor
{
    [CustomPropertyDrawer(typeof(NeoFpsInputSystemActionRefAttribute))]
    public class NeoFpsInputSystemActionRefDrawer : PropertyDrawer
    {
        static SerializedProperty m_CurrentProperty = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var controls = NeoFpsNewInputManager.controls;

            string current = "<Not Set>";
            if (property.stringValue != string.Empty)
            {
                var action = controls.FindAction(property.stringValue);
                if (action == null)
                    current = "<Broken Reference>";
                else
                    current = action.name;
            }

            position = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName, property.tooltip));

            if (EditorGUI.DropdownButton(position, new GUIContent(current), FocusType.Passive))
            {
                m_CurrentProperty = property;

                var menu = new GenericMenu();

                foreach (var map in controls.asset.actionMaps)
                {
                    string path = map.name + "/";
                    foreach (var action in map.actions)
                        menu.AddItem(new GUIContent(path + action.name), false, SelectAction, action);
                }

                menu.ShowAsContext();
            }

            EditorGUI.EndProperty();
        }

        void SelectAction(object o)
        {
            var action = (InputAction)o;
            m_CurrentProperty.stringValue = action.id.ToString();
            m_CurrentProperty.serializedObject.ApplyModifiedProperties();
            m_CurrentProperty = null;
        }
    }
}

#endif