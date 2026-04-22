#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM

using UnityEditor;
using NeoFPS;
using NeoFPS.Samples;
using UnityEditorInternal;
using UnityEngine;
using System;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace NeoFPSEditor.Samples
{
    [CustomEditor(typeof(OptionsMenuInputSystemBindings), true)]
    public class OptionsMenuInputSystemBindingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            // Base
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StartingSelection"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnBackButtonPressed"));
            EditorGUILayout.Space();

            // Main properties
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ContainerTransform"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ResetToDefaultsButton"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PrototypeDivider"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PrototypeBinding"));
            EditorGUILayout.Space();

            m_IgnoreList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        #region IGNORE ACTIONS

        private ReorderableList m_IgnoreList = null;

        void OnEnable()
        {
            m_IgnoreList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_IgnoreActions"), true, true, true, true);
            m_IgnoreList.drawHeaderCallback = DrawIgnoreListHeader;
            m_IgnoreList.drawElementCallback = DrawIgnoreListElement;
            m_IgnoreList.onAddDropdownCallback = OnIgnoreListAddPressed;
            m_IgnoreList.onRemoveCallback = OnIgnoreListRemovePressed;
        }

        void DrawIgnoreListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Actions To Ignore", EditorStyles.boldLabel);
        }

        void DrawIgnoreListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var guid = m_IgnoreList.serializedProperty.GetArrayElementAtIndex(index).stringValue;
            var action = NeoFpsNewInputManager.controls.FindAction(guid);

            if (action != null)
                EditorGUI.LabelField(rect, action.actionMap.name + "/" + action.name);
            else
                EditorGUI.LabelField(rect, "<Invalid Action>");
        }

        void OnIgnoreListAddPressed(Rect buttonRect, ReorderableList list)
        {
            var menu = new GenericMenu();

            // Iterate through all available actions
            var controls = NeoFpsNewInputManager.controls.asset;
            for (int i = 0; i < controls.actionMaps.Count; ++i)
            {
                var actionMap = controls.actionMaps[i];
                for (int j = 0; j < actionMap.actions.Count; ++j)
                {
                    var action = actionMap.actions[j];

                    // Filter button type actions only
                    if (action.type != InputActionType.Button)
                        continue;

                    // Remove actions already ignored
                    bool ignored = false;
                    var guid = action.id.ToString();
                    for (int k = 0; k < m_IgnoreList.serializedProperty.arraySize; ++k)
                    {
                        var prop = m_IgnoreList.serializedProperty.GetArrayElementAtIndex(k);
                        if (prop.stringValue == guid)
                        {
                            ignored = true;
                            break;
                        }
                    }

                    // Add menu entry
                    if (!ignored)
                        menu.AddItem(new GUIContent(actionMap.name + "/" + action.name), false, PickAction, guid);
                }
            }

            menu.ShowAsContext();
        }

        void PickAction(object o)
        {
            // Append to list and get property
            int index = m_IgnoreList.serializedProperty.arraySize++;
            var prop = m_IgnoreList.serializedProperty.GetArrayElementAtIndex(index);

            // Set to GUID of action
            prop.stringValue = (string)o;

            // Apply
            serializedObject.ApplyModifiedProperties();
        }

        void OnIgnoreListRemovePressed(ReorderableList list)
        {
            SerializedArrayUtility.RemoveAt(list.serializedProperty, list.index);
        }

        #endregion
    }
}

#endif