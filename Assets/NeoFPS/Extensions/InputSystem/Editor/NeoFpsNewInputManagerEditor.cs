#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM

using UnityEngine;
using UnityEditor;
using NeoFPS;
using System;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace NeoFPSEditor
{
    [CustomEditor(typeof(NeoFpsNewInputManager))]
    public class NeoFpsNewInputManagerEditor : NeoFpsInputManagerBaseEditor
    {
        protected override void InspectInternal()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Input Actions", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UIActions"));
            NeoFpsEditorGUI.Separator();

            InspectGamepadProfiles();
            EditorGUILayout.Space();

            NeoFpsEditorGUI.Separator();
            base.InspectInternal();
        }

        #region GAMEPAD PROFILES

        void InspectGamepadProfiles()
        {
            EditorGUILayout.LabelField("Gamepad Profiles", EditorStyles.boldLabel);

            var gamepadProfiles = serializedObject.FindProperty("m_GamepadProfiles");
            for (int i = 0; i < gamepadProfiles.arraySize; ++i)
                DrawGamepadProfile(gamepadProfiles, i);

            EditorGUILayout.Space();

            if (GUILayout.Button("Add New Profile"))
            {
                ++gamepadProfiles.arraySize;
                var prop = gamepadProfiles.GetArrayElementAtIndex(gamepadProfiles.arraySize - 1);
                prop.FindPropertyRelative("m_ProfileName").stringValue = "Gamepad";
                prop.FindPropertyRelative("m_Axes").enumValueIndex = 0;
                prop.FindPropertyRelative("m_ActionIDs").arraySize = 0;
                prop.FindPropertyRelative("m_ActionIDs").arraySize = (int)NeoGamepadControl.count;
            }
        }

        void DrawGamepadProfile(SerializedProperty gamepadProfiles, int index)
        {
            var prop = gamepadProfiles.GetArrayElementAtIndex(index);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            ++EditorGUI.indentLevel;
            var profileName = prop.FindPropertyRelative("m_ProfileName");
            var expanded = prop.FindPropertyRelative("expanded");

            expanded.boolValue = EditorGUILayout.Foldout(expanded.boolValue, profileName.stringValue, true);
            if (expanded.boolValue)
            {
                EditorGUILayout.PropertyField(profileName);
                GUILayout.Space(4);
                EditorGUILayout.PropertyField(prop.FindPropertyRelative("m_Axes"));
                GUILayout.Space(4);

                // Get actions
                var actions = prop.FindPropertyRelative("m_ActionIDs");

                // Check array size
                if (actions.arraySize != (int)NeoGamepadControl.count)
                    actions.arraySize = (int)NeoGamepadControl.count;

                // Draw actions
                DrawActionPicker("Button North", actions.GetArrayElementAtIndex((int)NeoGamepadControl.ButtonNorth));
                DrawActionPicker("Button South", actions.GetArrayElementAtIndex((int)NeoGamepadControl.ButtonSouth));
                DrawActionPicker("Button East", actions.GetArrayElementAtIndex((int)NeoGamepadControl.ButtonEast));
                DrawActionPicker("Button West", actions.GetArrayElementAtIndex((int)NeoGamepadControl.ButtonWest));
                GUILayout.Space(4);
                DrawActionPicker("D-Pad Up", actions.GetArrayElementAtIndex((int)NeoGamepadControl.DPadUp));
                DrawActionPicker("D-Pad Down", actions.GetArrayElementAtIndex((int)NeoGamepadControl.DPadDown));
                DrawActionPicker("D-Pad Left", actions.GetArrayElementAtIndex((int)NeoGamepadControl.DPadLeft));
                DrawActionPicker("D-Pad Right", actions.GetArrayElementAtIndex((int)NeoGamepadControl.DPadRight));
                GUILayout.Space(4);
                DrawActionPicker("Left Trigger", actions.GetArrayElementAtIndex((int)NeoGamepadControl.LeftTrigger));
                DrawActionPicker("Left Bumper", actions.GetArrayElementAtIndex((int)NeoGamepadControl.LeftBumper));
                DrawActionPicker("Right Trigger", actions.GetArrayElementAtIndex((int)NeoGamepadControl.RightTrigger));
                DrawActionPicker("Right Bumper", actions.GetArrayElementAtIndex((int)NeoGamepadControl.RightBumper));
                GUILayout.Space(4);
                DrawActionPicker("Left-Stick Press", actions.GetArrayElementAtIndex((int)NeoGamepadControl.LeftStickPress));
                DrawActionPicker("Right-Stick Press", actions.GetArrayElementAtIndex((int)NeoGamepadControl.RightStickPress));

                EditorGUILayout.Space();

                if (GUILayout.Button("Remove Profile"))
                {
                    SerializedArrayUtility.RemoveAt(gamepadProfiles, index);
                    serializedObject.ApplyModifiedProperties();
                    throw new ExitGUIException();
                }

            }
            --EditorGUI.indentLevel;
            EditorGUILayout.EndVertical();
        }

        private SerializedProperty m_ActionProp = null;

        void DrawActionPicker (string label, SerializedProperty prop)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(label);

            var entry = new GUIContent();
            if (string.IsNullOrEmpty(prop.stringValue))
                entry.text = "<None Set>";
            else
            {
                // Get action based on guid
                var action = NeoFpsNewInputManager.controls.FindAction(prop.stringValue);
                if (action != null)
                    entry.text = action.actionMap.name + "/" + action.name;
                else
                    entry.text = "<Invalid Action>";
            }

            var list = EditorGUILayout.DropdownButton(entry, FocusType.Passive);

            EditorGUILayout.EndHorizontal();

            if (list)
            {
                m_ActionProp = prop;

                var menu = new GenericMenu();

                var controls = NeoFpsNewInputManager.controls.asset;
                for (int i = 0; i < controls.actionMaps.Count; ++i)
                {
                    var actionMap = controls.actionMaps[i];
                    for (int j = 0; j < actionMap.actions.Count; ++j)
                    {
                        var action = actionMap.actions[j];
                        if (action.type == InputActionType.Button)
                            menu.AddItem(new GUIContent(actionMap.name + "/" + action.name), false, PickAction, action.id);
                    }
                }

                menu.ShowAsContext();
            }
        }

        void PickAction(object o)
        {
            var guid = (Guid)o;
            m_ActionProp.stringValue = guid.ToString();
            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}

#endif