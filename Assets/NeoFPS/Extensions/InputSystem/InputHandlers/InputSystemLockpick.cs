using UnityEngine;
using NeoFPS.Constants;
using System.Collections;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inputref-mb-inputsystemlockpick.html")]
    public class InputSystemLockpick : FpsInputSystemHandler
    {
#if ENABLE_INPUT_SYSTEM

        [SerializeField, Tooltip("The maximum turn rate of the pick object in degrees per second.")]
        private float m_AnalogueTurnRate = 90f;

        private IPickAngleLockpickPopup m_LockpickPopup = null;

        public override FpsInputContext inputContext
        {
            get { return FpsInputContext.Menu; }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            m_LockpickPopup = GetComponent<IPickAngleLockpickPopup>();
            if (m_LockpickPopup == null)
                Debug.LogError("InputLockpick is placed on a gameobject without a lockpick popup (ILockpickPopup)");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Capture mouse cursor
            NeoFpsInputManager.captureMouseCursor = true;
            StartCoroutine(MouseCapture());

            // Push escape handler
            NeoFpsInputManager.PushEscapeHandler(m_LockpickPopup.Cancel);
        }

        IEnumerator MouseCapture()
        {
            yield return null;
            NeoFpsInputManager.captureMouseCursor = true;
        }

        protected override void OnDisable()
        {
            // Pop escape handler
            NeoFpsInputManager.PopEscapeHandler(m_LockpickPopup.Cancel);

            // Capture mouse cursor
            NeoFpsInputManager.captureMouseCursor = false;

            base.OnDisable();
        }

        protected override void UpdateInput()
        {
            if (m_LockpickPopup != null)
            {
                // Main controls
                var controls = NeoFpsNewInputManager.controls;

                // Get rotation (sum of mouse and both analogues
                float rotatePick = GetVector2(controls.Movement.MouseLook).x;
                rotatePick += GetVector2(controls.Movement.AnalogueLook).x * m_AnalogueTurnRate * Time.deltaTime;
                rotatePick += GetVector2(controls.Movement.AnalogueMove).x * m_AnalogueTurnRate * Time.deltaTime;

                // Get primary mouse button
                bool tension = Mouse.current.leftButton.isPressed || GetButton(controls.Movement.StrafeRight);

                // Get A (Xbox) or Cross (PS) gamepad button
                if (Gamepad.current != null)
                    tension |= Gamepad.current.buttonSouth.isPressed;

                // Apply input
                m_LockpickPopup.ApplyInput(rotatePick, tension);
            }
        }

#endif
    }
}
