using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NeoFPS
{
#if ENABLE_INPUT_SYSTEM

    using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

    public class NeoFpsInputSystemTouchButton : BaseInputSystemTouchControl
    {
        [Header ("Input")]
        [SerializeField, NeoFpsInputSystemActionRef, Tooltip("The action the control should mimic.")]
        private string m_ActionID = string.Empty;

        [Header("Events")]

        [SerializeField, Tooltip("An event fired when the player first touches this control.")]
        private UnityEvent m_OnTouchStarted = null;
        [SerializeField, Tooltip("An event fired when a touch that started on this control is released.")]
        private UnityEvent m_OnTouchEnded = null;

        private InputAction m_Action = null;

        protected override void Awake()
        {
            var controls = NeoFpsNewInputManager.controls;
            if (!string.IsNullOrEmpty(m_ActionID))
                m_Action = controls.FindAction(m_ActionID);

            base.Awake();

            if (m_Action == null)
                enabled = false;
        }

        public override bool HandleTouch(ETouch touch)
        {
            if (m_Action != null)
                controller.SetVirtualButton(m_Action.id, true);

            return consume;
        }

        protected override void OnTouchStarted()
        {
            m_OnTouchStarted.Invoke();
        }

        protected override void OnTouchEnded()
        {
            m_OnTouchEnded.Invoke();
        }

#endif
    }
}