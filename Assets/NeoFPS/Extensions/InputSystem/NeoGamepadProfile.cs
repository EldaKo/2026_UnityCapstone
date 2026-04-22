using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace NeoFPS
{
    [Serializable]
    public class NeoGamepadProfile
    {
#if ENABLE_INPUT_SYSTEM

        [SerializeField]
        private string m_ProfileName = "Gamepad";

#if UNITY_EDITOR
        public bool expanded = true;
#endif

        [SerializeField]
        private NeoGamepadAxes m_Axes = NeoGamepadAxes.LeftMoveRightLook;

        [SerializeField]
        private string[] m_ActionIDs = new string[(int)NeoGamepadControl.count];

        public string profileName
        {
            get { return m_ProfileName; }
        }

        void OnValidate()
        {
            // Check for correct Action ID count
            if (m_ActionIDs.Length != (int)NeoGamepadControl.count)
            {
                var newActionIDs = new string[(int)NeoGamepadControl.count];
                for (int i = 0; i < newActionIDs.Length && i < m_ActionIDs.Length; ++i)
                    newActionIDs[i] = m_ActionIDs[i];
                m_ActionIDs = newActionIDs;
            }
        }

        public void ApplyBindings(NeoFpsInputActions inputActions, bool enabled)
        {
            // Iterate through bindings
            foreach (var binding in inputActions.bindings)
            {
                // Check for gamepad group
                if (binding.groups != null && binding.groups.Contains(inputActions.GamepadScheme.bindingGroup))
                {
                    // Get action and check it's button type
                    var action = inputActions.FindAction(binding.action);

                    // Enable / disable the controller
                    if (enabled)
                    {
                        // Filter button types
                        if (action.type == InputActionType.Button)
                        {
                            // Get the ID as a string (because GUIDs suck - not serializable)
                            var stringID = action.id.ToString();

                            // Iterate through profiles' bindings
                            bool found = false;
                            for (int i = 0; i < m_ActionIDs.Length; ++i)
                            {
                                if (!string.IsNullOrEmpty(m_ActionIDs[i]) && stringID == m_ActionIDs[i])
                                {
                                    var index = action.bindings.IndexOf((b) => { return b.id == binding.id; });
                                    if (index >= 0)
                                    {
                                        // Rebind
                                        switch ((NeoGamepadControl)i)
                                        {
                                            case NeoGamepadControl.ButtonNorth:
                                                action.ApplyBindingOverride(index, "<Gamepad>/buttonNorth");
                                                break;
                                            case NeoGamepadControl.ButtonSouth:
                                                action.ApplyBindingOverride(index, "<Gamepad>/buttonSouth");
                                                break;
                                            case NeoGamepadControl.ButtonEast:
                                                action.ApplyBindingOverride(index, "<Gamepad>/buttonEast");
                                                break;
                                            case NeoGamepadControl.ButtonWest:
                                                action.ApplyBindingOverride(index, "<Gamepad>/buttonWest");
                                                break;
                                            case NeoGamepadControl.DPadUp:
                                                action.ApplyBindingOverride(index, "<Gamepad>/dpad/up");
                                                break;
                                            case NeoGamepadControl.DPadDown:
                                                action.ApplyBindingOverride(index, "<Gamepad>/dpad/down");
                                                break;
                                            case NeoGamepadControl.DPadLeft:
                                                action.ApplyBindingOverride(index, "<Gamepad>/dpad/left");
                                                break;
                                            case NeoGamepadControl.DPadRight:
                                                action.ApplyBindingOverride(index, "<Gamepad>/dpad/right");
                                                break;
                                            case NeoGamepadControl.LeftTrigger:
                                                action.ApplyBindingOverride(index, "<Gamepad>/leftTrigger");
                                                break;
                                            case NeoGamepadControl.LeftBumper:
                                                action.ApplyBindingOverride(index, "<Gamepad>/leftShoulder");
                                                break;
                                            case NeoGamepadControl.RightTrigger:
                                                action.ApplyBindingOverride(index, "<Gamepad>/rightTrigger");
                                                break;
                                            case NeoGamepadControl.RightBumper:
                                                action.ApplyBindingOverride(index, "<Gamepad>/rightShoulder");
                                                break;
                                            case NeoGamepadControl.LeftStickPress:
                                                action.ApplyBindingOverride(index, "<Gamepad>/leftStickPress");
                                                break;
                                            case NeoGamepadControl.RightStickPress:
                                                action.ApplyBindingOverride(index, "<Gamepad>/rightStickPress");
                                                break;
                                        }

                                        found = true;
                                        break;
                                    }
                                }
                            }

                            // Not found. Bind to nothing
                            if (!found)
                            {
                                var index = action.bindings.IndexOf((b) => { return b.id == binding.id; });
                                if (index >= 0)
                                    action.ApplyBindingOverride(index, string.Empty);
                            }
                        }
                    }
                    else
                    {
                        // Disable the binding
                        var index = action.bindings.IndexOf((b) => { return b.id == binding.id; });
                        action.ApplyBindingOverride(index, string.Empty);
                    }
                }
            }

            // Sort stick axes
            if (enabled)
            {
                var move = inputActions.Movement.AnalogueMove;
                for (int i = 0; i < move.bindings.Count; ++i)
                {
                    // Find gamepad binding
                    if (move.bindings[i].groups != null && move.bindings[i].groups.Contains(inputActions.GamepadScheme.bindingGroup))
                    {
                        if (m_Axes == NeoGamepadAxes.LeftMoveRightLook)
                            move.ApplyBindingOverride(i, "<Gamepad>/leftStick");
                        else
                            move.ApplyBindingOverride(i, "<Gamepad>/rightStick");

                        break;
                    }
                }
                var look = inputActions.Movement.AnalogueLook;
                for (int i = 0; i < look.bindings.Count; ++i)
                {
                    // Find gamepad binding
                    if (look.bindings[i].groups != null && look.bindings[i].groups.Contains(inputActions.GamepadScheme.bindingGroup))
                    {
                        if (m_Axes == NeoGamepadAxes.LeftMoveRightLook)
                            look.ApplyBindingOverride(i, "<Gamepad>/rightStick");
                        else
                            look.ApplyBindingOverride(i, "<Gamepad>/leftStick");

                        break;
                    }
                }
            }
        }

#endif

    }

    public enum NeoGamepadControl : int
    {
        ButtonNorth,
        ButtonSouth,
        ButtonEast,
        ButtonWest,
        DPadUp,
        DPadDown,
        DPadLeft,
        DPadRight,
        LeftTrigger,
        LeftBumper,
        RightTrigger,
        RightBumper,
        LeftStickPress,
        RightStickPress,
        count
    }

    public enum NeoGamepadAxes
    {
        LeftMoveRightLook,
        RightMoveLeftLook
    }
}
