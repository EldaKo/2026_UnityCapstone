using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using UnityEngine.InputSystem;

namespace NeoFPS
{
	public abstract class FpsInputSystemHandler : FpsInputBase
    {
#if ENABLE_INPUT_SYSTEM

        protected override bool isInputActive
        {
            get { return (NeoFpsNewInputManager.instance as NeoFpsNewInputManager != null); }
        }

        public float GetAxis(InputAction action)
        {
			if (!hasFocus || gainedFocusThisFrame)
				return 0f;

            // Get result
            float result = action.ReadValue<float>();

            // Add virtual input
            if (NeoFpsNewInputManager.virtualInput != null)
                result += NeoFpsNewInputManager.virtualInput.GetVirtualAxis(action.id);

            return result;
        }

        public Vector2 GetVector2(InputAction action)
        {
			if (!hasFocus || gainedFocusThisFrame)
				return Vector2.zero;

            // Get result
            Vector2 result = action.ReadValue<Vector2>();

            // Add virtual input
            if (NeoFpsNewInputManager.virtualInput != null)
                result += NeoFpsNewInputManager.virtualInput.GetVirtualVector2D(action.id);

            return result;
        }

        public bool GetButton(InputAction action)
        {
			if (!hasFocus || gainedFocusThisFrame)
				return false;

            // Get result
            bool result = action.IsPressed();

            // Add virtual input
            if (NeoFpsNewInputManager.virtualInput != null)
                result |= NeoFpsNewInputManager.virtualInput.GetVirtualButton(action.id);

            return result;
        }

        public bool GetButtonDown(InputAction action)
        {
			if (!hasFocus || gainedFocusThisFrame)
				return false;

            // Get result
            bool result = action.WasPressedThisFrame();

            // Add virtual input
            if (NeoFpsNewInputManager.virtualInput != null)
                result |= NeoFpsNewInputManager.virtualInput.GetVirtualButtonDown(action.id);

            return result;
        }

        public bool GetButtonUp(InputAction action)
        {
			if (!hasFocus || gainedFocusThisFrame)
				return false;

            // Get result
            bool result = action.WasReleasedThisFrame();

            // Add virtual input
            if (NeoFpsNewInputManager.virtualInput != null)
                result |= NeoFpsNewInputManager.virtualInput.GetVirtualButtonUp(action.id);

            return result;
        }

#else

        protected override bool isInputActive
        {
            get { return (NeoFpsNewInputManager.instance as NeoFpsNewInputManager != null); }
        }

        protected override void UpdateInput()
        { }

#endif
    }
}
