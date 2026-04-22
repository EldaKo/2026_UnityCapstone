#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using NeoFPS;
using UnityEngine.Events;

namespace NeoFPSEditor
{
    public static class NeoFpsInputSystemConverters
    {
        [MenuItem("Tools/NeoFPS/Input Handlers/Convert To Input System")]
        public static void ConvertToInputSystem()
        {
#if ENABLE_INPUT_SYSTEM

            if (!ShowConfirmationDialogue())
                return;

            int count = 0;
            int ignoredCount = 0;
            int interventionCount = 0;
            ProcessPrefabs<FpsInput>((inputHandler, offset) =>
            {
                if (inputHandler is InputCharacterMotion)
                    ProcessComponents<InputSystemCharacterMotion>(inputHandler, offset);
                else if (inputHandler is InputCharacterSlowMo)
                    ProcessComponents<InputSystemCharacterSlowMo>(inputHandler, offset);
                else if (inputHandler is InputFirearm)
                    ProcessComponents<InputSystemFirearm>(inputHandler, offset);
                else if (inputHandler is InputInventory)
                    ProcessComponents<InputSystemInventory>(inputHandler, offset);
                else if (inputHandler is InputLockpick)
                    ProcessComponents<InputSystemLockpick>(inputHandler, offset);
                else if (inputHandler is InputMenu)
                    ProcessComponents<InputSystemMenu>(inputHandler, offset);
                else if (inputHandler is UiInputToggle)
                    ProcessComponents<MenuInputSystemToggle>(inputHandler, offset);
                else if (inputHandler is InputMeleeWeapon)
                    ProcessComponents<InputSystemMeleeWeapon>(inputHandler, offset);
                else if (inputHandler is InputThrownWeapon)
                    ProcessComponents<InputSystemThrownWeapon>(inputHandler, offset);
                else if (inputHandler is InputWieldableTool)
                    ProcessComponents<InputSystemWieldableTool>(inputHandler, offset);
                else if (inputHandler is InputAbilityFirearm)
                    ProcessComponents<InputSystemAbilityFirearm>(inputHandler, offset);
                else if (inputHandler is InputGame)
                    ProcessComponents<InputSystemGame>(inputHandler, offset);
                else if (inputHandler is InputCarryObject)
                    ProcessComponents<InputSystemCarryObject>(inputHandler, offset);
                else if (inputHandler is InputFirearmWithSecondary)
                    ProcessComponents<InputSystemFirearmWithSecondary>(inputHandler, offset);
                else if (inputHandler is InputDualWield)
                    ProcessComponents<InputSystemDualWield>(inputHandler, offset);
                else if (inputHandler is InputFirearmWithMelee)
                {
                    Debug.Log("InputSystemFirearmWithMelee needs custom settings. Click this log message to ping the object", inputHandler.gameObject);
                    ProcessComponents<InputSystemFirearmWithMelee>(inputHandler, offset);
                    ++interventionCount;
                }
                else if (inputHandler is NeoFpsTouchScreenController)
                {
                    // Do nothing - just need to swap the prefab in the scene
                    ++ignoredCount;
                    --count;
                }
                else
                {
                    // The input handler isn't known
                    Debug.LogFormat("Swap method not found for type: {0} on object {1}", inputHandler.GetType().Name, inputHandler.gameObject);
                    ++ignoredCount;
                    --count;
                }

                ++count;
            });

            ProcessPrefabs<HudToggle>((hudToggle, offset) =>
            {
                // Add the replacement component
                var replacement = ObjectFactory.AddComponent<InputSystemHudToggle>(hudToggle.gameObject);
                //original.gameObject.AddComponent<T>();

                // Copy/paste values across
                ComponentUtility.CopyComponent(hudToggle);
                ComponentUtility.PasteComponentValues(replacement);

                // Destroy the original
                Object.DestroyImmediate(hudToggle, true);
            });

            if (interventionCount > 0)
                Debug.LogFormat("Replaced {0} components and skipped {1}. {2} components need user attention (see logs)", count, ignoredCount, interventionCount);
            else
                Debug.LogFormat("Replaced {0} components and skipped {1}", count, ignoredCount);

#else
            Debug.LogError("The input system is not enabled for this project. Please set the active input handler in your player settings to \"Both\" (this will require a restart of the editor)");
#endif
        }

        [MenuItem("Tools/NeoFPS/Input Handlers/Convert To Input Manager (Default)")]
        public static void ConvertToInputManager()
        {
#if ENABLE_LEGACY_INPUT_MANAGER

            if (!ShowConfirmationDialogue())
                return;

            int count = 0;
            int ignoredCount = 0;
            int interventionCount = 0;
            ProcessPrefabs<FpsInputSystemHandler>((inputHandler, path) =>
            {
                if (inputHandler is InputSystemCharacterMotion)
                    ProcessComponents<InputCharacterMotion>(inputHandler, path);
                else if (inputHandler is InputSystemCharacterSlowMo)
                    ProcessComponents<InputCharacterSlowMo>(inputHandler, path);
                else if (inputHandler is InputSystemFirearm)
                    ProcessComponents<InputFirearm>(inputHandler, path);
                else if (inputHandler is InputSystemInventory)
                    ProcessComponents<InputInventory>(inputHandler, path);
                else if (inputHandler is InputSystemLockpick)
                    ProcessComponents<InputLockpick>(inputHandler, path);
                else if (inputHandler is InputSystemMenu)
                    ProcessComponents<InputMenu>(inputHandler, path);
                else if (inputHandler is MenuInputSystemToggle)
                    ProcessComponents<UiInputToggle>(inputHandler, path);
                else if (inputHandler is InputSystemMeleeWeapon)
                    ProcessComponents<InputMeleeWeapon>(inputHandler, path);
                else if (inputHandler is InputSystemThrownWeapon)
                    ProcessComponents<InputThrownWeapon>(inputHandler, path);
                else if (inputHandler is InputSystemWieldableTool)
                    ProcessComponents<InputWieldableTool>(inputHandler, path);
                else if (inputHandler is InputSystemAbilityFirearm)
                    ProcessComponents<InputAbilityFirearm>(inputHandler, path);
                else if (inputHandler is InputSystemGame)
                    ProcessComponents<InputGame>(inputHandler, path);
                else if (inputHandler is InputSystemCarryObject)
                    ProcessComponents<InputCarryObject>(inputHandler, path);
                else if (inputHandler is InputSystemFirearmWithMelee)
                {
                    Debug.Log("InputSystemFirearmWithMelee needs custom settings. Click this log message to ping the object", inputHandler.gameObject);
                    ProcessComponents<InputFirearmWithMelee>(inputHandler, path);
                    ++interventionCount;
                }
                else if (inputHandler is NeoFpsInputSystemTouchScreenController)
                {
                    // Do nothing - just need to swap the prefab in the scene
                    ++ignoredCount;
                    --count;
                }
                else
                {
                    // The input handler isn't known
                    Debug.LogFormat("Swap method not found for type: {0} on object {1}", inputHandler.GetType().Name, inputHandler.gameObject);
                    ++ignoredCount;
                    --count;
                }

                ++count;
            });


            if (interventionCount > 0)
                Debug.LogFormat("Replaced {0} components and skipped {1}. {2} components need user attention (see logs)", count, ignoredCount, interventionCount);
            else
                Debug.LogFormat("Replaced {0} components and skipped {1}", count, ignoredCount);

#else
            Debug.LogError("The legacy input manager is not enabled for this project. Please set the active input handler in your player settings to \"Both\" (this will require a restart of the editor)");
#endif
        }

        static void ProcessPrefabs<T>(UnityAction<T, string> callback) where T : MonoBehaviour
        {
            List<T> inputHandlers = new List<T>();
            List<Component> components = new List<Component>();

            var guids = AssetDatabase.FindAssets("t:GameObject");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                prefab.GetComponentsInChildren(true, inputHandlers);

                if (inputHandlers.Count > 0)
                    prefab.GetComponentsInChildren(true, components);

                foreach (var inputHandler in inputHandlers)
                {
                    // Skip variants or nested prefabs
                    if (PrefabUtility.IsPartOfPrefabInstance(inputHandler) || PrefabUtility.IsPartOfVariantPrefab(inputHandler))
                        continue;

                    callback?.Invoke(inputHandler, path);
                }

                inputHandlers.Clear();
                components.Clear();
            }

            Undo.ClearAll();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void ProcessComponents<T>(Component original, string path) where T : FpsInputBase
        {
            // Add the replacement component
            var replacement = ObjectFactory.AddComponent<T>(original.gameObject);
            //original.gameObject.AddComponent<T>();

            // Copy/paste values across
            ComponentUtility.CopyComponent(original);
            ComponentUtility.PasteComponentValues(replacement);

            // Destroy the original
            Object.DestroyImmediate(original, true);
        }

        static bool ShowConfirmationDialogue()
        {
            return EditorUtility.DisplayDialog("Convert Input Handlers", "This operation will modify multiple prefabs in your project and can not be undone. Make sure you have a backup before proceeding.\n\nDo you want to continue?", "Continue", "Cancel");
        }
    }
}

#endif