using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;
#endif

namespace NeoFPS
{
    [CreateAssetMenu(fileName = "FpsManager_Input", menuName = "NeoFPS/Managers/Input Manager (New System)", order = NeoFpsMenuPriorities.manager_input)]
    [HelpURL("https://docs.neofps.com/manual/inputref-so-inputmanager.html")]
    public class NeoFpsNewInputManager : NeoFpsInputManagerBase
    {
#if ENABLE_INPUT_SYSTEM

        private static NeoFpsNewInputManager s_CastInstance = null;
        private static RuntimeBehaviour s_ProxyBehaviour = null;
        private static NeoFpsInputActions s_Controls = null;

        public static NeoFpsInputActions controls
        {
            get
            {
                if (s_Controls == null)
                    s_Controls = new NeoFpsInputActions();
                return s_Controls;
            }
        }

        protected override void Initialise()
        {
            s_CastInstance = this;

            base.Initialise();

            LoadBindings();
            InitialiseEscapeHandlers();

            s_ProxyBehaviour = GetBehaviourProxy<RuntimeBehaviour>();
        }

        public override bool IsValid()
        {
            return true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (s_ProxyBehaviour != null)
            {
                Destroy(s_ProxyBehaviour);
                s_ProxyBehaviour = null;
            }

            if (s_CastInstance == this)
                s_CastInstance = null;
        }

        #region VIRTUAL INPUT

        private static IInputSystemVirtualInput s_VirtualInput = null;

        public static IInputSystemVirtualInput virtualInput
        {
            get { return s_VirtualInput; }
            set
            {
                if (s_VirtualInput != null && value != null)
                    Debug.LogError("Assigning virtual input component to NeoFpsNewInputManager, but one is already assigned.");
                s_VirtualInput = value;
            }
        }

        #endregion

        #region ACTION MAPS

        [SerializeField, Tooltip("The input system actions collection for UI navigation.")]
        private InputActionAsset m_UIActions = null;

        private InputAction m_CancelAction = null;

        void InitialiseEscapeHandlers()
        {
            if (m_UIActions != null)
            {
                m_CancelAction = m_UIActions.FindAction("UI/Cancel");
                m_UIActions.Enable();
            }
        }

        protected override bool CheckForEscapeInput()
        {
            return (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame) || controls.Miscellaneous.Menu.WasPressedThisFrame();
        }

        protected override bool CheckForCancelInput()
        {
            return (m_CancelAction != null && m_CancelAction.WasPressedThisFrame()) || controls.Miscellaneous.Menu.WasPressedThisFrame();
        }

        #endregion

        #region GAMEPADS

        [SerializeField]
        private NeoGamepadProfile[] m_GamepadProfiles = { };

        private int m_CurrentGamepadProfile = -1;
        private bool m_GamepadEnabled = false;
        private bool m_GamepadConnected = false;

        public static string gamepadBindingGroup
        {
            get
            {
                if (controls != null)
                    return controls.GamepadScheme.bindingGroup;
                else
                    return string.Empty;
            }
        }

        protected override bool GetIsGamepadConnected()
        {
            return Gamepad.current != null;
        }

        protected override string GetConnectedGamepad()
        {
            if (Gamepad.current != null)
                return Gamepad.current.displayName;
            else
                return string.Empty;
        }

        protected override int GetNumGamepadProfiles()
        {
            return m_GamepadProfiles.Length;
        }

        protected override string GetGamepadProfileNameInteral(int index)
        {
            if (index >= 0 && index < m_GamepadProfiles.Length)
                return m_GamepadProfiles[index].profileName;
            return "Default";
        }

        void CheckGamepads()
        {
            var gamepadConnected = Gamepad.current != null;
            if (m_GamepadConnected != gamepadConnected)
            {
                m_GamepadConnected = gamepadConnected;
                OnGamepadConnectedChanged(gamepadConnected);
            }
        }

        void InitialiseGamepad()
        {
            FpsSettings.gamepad.onSettingsChanged += OnGamepadSettingsChanged;
            OnGamepadSettingsChanged();
        }

        void OnGamepadSettingsChanged()
        {
            bool changed = false;

            if (m_CurrentGamepadProfile != FpsSettings.gamepad.profile)
            {
                m_CurrentGamepadProfile = FpsSettings.gamepad.profile;
                changed = true;
            }

            if (m_GamepadEnabled != FpsSettings.gamepad.useGamepad)
            {
                m_GamepadEnabled = FpsSettings.gamepad.useGamepad;
                changed = true;
            }

            if (changed)
                m_GamepadProfiles[m_CurrentGamepadProfile].ApplyBindings(controls, m_GamepadEnabled);
        }

        #endregion

        #region PROXY BEHAVIOUR

        class RuntimeBehaviour : MonoBehaviour
        {
            void Start()
            {
                s_CastInstance.InitialiseGamepad();
            }

            void Update()
            {
                s_CastInstance.CheckGamepads();
            }

            void OnEnable()
            {
                controls.Enable();
            }

            void OnDisable()
            {
                controls.Disable();
            }
        }

        #endregion

        #region KEY BINDINGS

        private struct PendingRebind
        {
            public InputAction action;
            public int bindingIndex;
            public string path;
        }

        private static class RebindingInfo
        {
            public static bool isRebinding = false;
            public static InputAction action = null;
            public static UnityAction onCompleted = null;
            public static int bindingIndex = -1;

            public static void Reset()
            {
                isRebinding = false;
                action = null;
                onCompleted = null;
                bindingIndex = -1;
            }
        }

        [Serializable]
        private class RebindingIntermediate
        {
            public List<RebindingIntermediateEntry> controls = new List<RebindingIntermediateEntry>();
        }

        [Serializable]
        private struct RebindingIntermediateEntry
        {
            public string id;
            public string path;
            public List<string> bindings;
        }

        private static List<PendingRebind> s_PendingRebinds = new List<PendingRebind>();
        public static bool s_DirtySettings = false;

        public static string keyboardAndMouseBindingGroup
        {
            get
            {
                if (controls != null)
                    return controls.KeyboardAndMouseScheme.bindingGroup;
                else
                    return string.Empty;
            }
        }

        private static string m_Filepath = string.Empty;
        protected static string filepath
        {
            get
            {
                if (m_Filepath == string.Empty)
                {
#if UNITY_EDITOR
                    m_Filepath = Application.dataPath + "/../" + "InputSystemBindings.settings";
#else
					m_Filepath = Path.Combine (Application.dataPath, "InputSystemBindings.settings");
#endif
                }
                return m_Filepath;
            }
        }

        public static void SaveBindings()
        {
            if (s_DirtySettings)
            {
                // Create intermediate
                var intermediate = new RebindingIntermediate();

                // Populate intermediate
                foreach (var actionMap in controls.asset.actionMaps)
                {
                    foreach (var action in actionMap.actions)
                    {
                        // Only buttons
                        if (action.type == InputActionType.Button)
                        {
                            // Create entry for action
                            var entry = new RebindingIntermediateEntry();
                            entry.id = action.id.ToString();
                            entry.path = actionMap.name + "/" + action.name;
                            entry.bindings = new List<string>();

                            // Get bindings
                            for (int i = 0; i < action.bindings.Count; ++i)
                            {
                                var binding = action.bindings[i];
                                if (binding.groups != null && binding.groups.Contains(keyboardAndMouseBindingGroup))
                                    entry.bindings.Add(binding.effectivePath);
                                //else
                                //    Debug.LogErrorFormat("Binding groups not found for action {0}", action.name);
                            }

                            // Add
                            intermediate.controls.Add(entry);
                        }
                    }
                }

                // Write intermediate to disk
                var json = JsonUtility.ToJson(intermediate, true);
                File.WriteAllText(filepath, json);

                s_DirtySettings = false;
            }
        }

        public static void LoadBindings()
        {
            if (File.Exists(filepath))
            {
                // Disable controls temporarily
                controls.Disable();

                try
                {
                    // Load the intermediate
                    string json = File.ReadAllText(filepath);
                    var intermediate = JsonUtility.FromJson<RebindingIntermediate>(json);

                    // Iterate through entries
                    foreach (var entry in intermediate.controls)
                    {
                        // Get the action
                        var action = controls.FindAction(entry.id);
                        if (action == null)                            
                            action = controls.FindAction(entry.path);

                        if (action != null)
                        {
                            // Iterate through bindings
                            int bindingIndex = 0;
                            for (int i = 0; i < action.bindings.Count && bindingIndex < entry.bindings.Count; ++i)
                            {
                                if (action.bindings[i].groups.Contains(keyboardAndMouseBindingGroup))
                                {
                                    action.ApplyBindingOverride(i, entry.bindings[bindingIndex++]);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to load input system bindings due to error: " + e.Message);
                }

                // Re-enable controls
                controls.Enable();
            }
        }

        public static void ResetKeyBindings()
        {
            // Remove input system overrides
            controls.RemoveAllBindingOverrides();

            // Re-apply gamepad profile
            s_CastInstance.m_GamepadProfiles[s_CastInstance.m_CurrentGamepadProfile].ApplyBindings(controls, s_CastInstance.m_GamepadEnabled);

            // Save
            s_DirtySettings = true;
            SaveBindings();
        }

        public static void RemapKeyBinding(InputAction actionToRebind, int bindingIndex, UnityAction onCompleted)
        {
            if (!RebindingInfo.isRebinding)
            {
                RebindingInfo.isRebinding = true;

                // Disable the action (required for rebinding)
                actionToRebind.Disable();
                //controlMap.Disable();

                // Start the rebind and store the info
                RebindingInfo.bindingIndex = bindingIndex;
                RebindingInfo.action = actionToRebind;
                RebindingInfo.onCompleted = onCompleted;
                actionToRebind.PerformInteractiveRebinding(bindingIndex)
                    .WithBindingGroup(keyboardAndMouseBindingGroup)
                    .OnComplete(OnCompleteRebinding)
                    .OnCancel(OnCompleteRebinding)
                    .OnPotentialMatch(OnPotentialPrimaryBinding)
                    .Start();
            }
            else
                Debug.LogError("Attempting to rebind a key while a rebinding operation is already underway");
        }

        static void OnPotentialPrimaryBinding(InputActionRebindingExtensions.RebindingOperation op)
        {
            if (op.candidates.Count > 0)
            {
                bool consumed = false;

                var key = op.candidates[0] as KeyControl;
                if (key != null)
                {
                    // Cancel rebinding
                    if (key.keyCode == Key.Escape)
                        consumed = true;

                    // Clear this binding (deferred to end)
                    if (key.keyCode == Key.Delete)
                    {
                        consumed = true;

                        // Add a pending rebind for the current control to empty
                        s_PendingRebinds.Add(new PendingRebind()
                        {
                            action = RebindingInfo.action,
                            bindingIndex = RebindingInfo.bindingIndex,
                            path = string.Empty
                        });

                        s_DirtySettings = true;
                    }
                }

                if (!consumed)
                {
                    var potential = op.candidates[0];

                    // Get control path and adapt to generic
                    var path = potential.path;
                    var altPath = potential.path;
                    altPath = altPath.Replace("/Keyboard", "<Keyboard>");
                    altPath = altPath.Replace("/Mouse", "<Mouse>");

                    // Check for clashes and reset
                    foreach (var existing in controls.bindings)
                    {
                        if (existing.effectivePath == path || existing.effectivePath == altPath)
                        {
                            var action = controls.FindAction(existing.action);
                            var bindingIndex = action.bindings.IndexOf((b) => { return b.id == existing.id; });

                            // Check if not current action / binding
                            s_PendingRebinds.Add(new PendingRebind()
                            {
                                action = action,
                                bindingIndex = bindingIndex,
                                path = string.Empty
                            });
                        }
                    }

                    // Add rebind
                    s_PendingRebinds.Add(new PendingRebind()
                    {
                        action = RebindingInfo.action,
                        bindingIndex = RebindingInfo.bindingIndex,
                        path = path
                    });

                    s_DirtySettings = true;
                }
            }
            else
                Debug.LogError("Potential keybind, but no candidates!!!");

            // Cancel the rebind. Even if it was valid, we handle it through the pending
            // rebinds system as that allows for preventing clashes, etc
            op.Cancel();
        }

        static void OnCompleteRebinding(InputActionRebindingExtensions.RebindingOperation op)
        {
            // Dispose (doing it later has caused errors for some reason)
            op.Dispose();

            // Apply the binding override if required
            for (int i = 0; i < s_PendingRebinds.Count; ++i)
                s_PendingRebinds[i].action.ApplyBindingOverride(s_PendingRebinds[i].bindingIndex, s_PendingRebinds[i].path);
            s_PendingRebinds.Clear();

            // Re-enable the action
            RebindingInfo.action.Enable();

            // Save the settings if dirty
            SaveBindings();

            // Fire the completed event
            RebindingInfo.onCompleted?.Invoke();

            // Re-enable input
            controls.Enable();

            // Cleanup
            RebindingInfo.Reset();
        }

        #endregion


#else

        protected override bool GetIsGamepadConnected() { return false; }

        protected override string GetConnectedGamepad() { return string.Empty; }

        protected override int GetNumGamepadProfiles() { return 0; }

        protected override string GetGamepadProfileNameInteral(int index) { return string.Empty; }

        protected override bool CheckForEscapeInput() { return false; }

        protected override bool CheckForCancelInput() { return false; }

#endif
    }
}