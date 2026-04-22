using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NeoFPS
{
#if ENABLE_INPUT_SYSTEM
    using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
#endif

    public class NeoFpsInputSystemTouchVirtualAnalog : BaseInputSystemTouchControl
    {
#if ENABLE_INPUT_SYSTEM

        [Header("Input")]
        [SerializeField, NeoFpsInputSystemActionRef, Tooltip("The action the control should affect.")]
        private string m_ActionID = string.Empty;
        [SerializeField, Tooltip("A falloff curve for the input strength. The horizontal axis is the normalised distance of the touch from the center of the control. The vertical axis is the output strength.")]
        private AnimationCurve m_InputCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Visualisation")]

        [SerializeField, Tooltip("A visual marker for the analog position that appears while touch is held.")]
        private RectTransform m_TouchMarkerTransform = null;
        [SerializeField, Tooltip("The distance the marker should move from the center of the analog control when at full strength.")]
        private float m_MaxMarkerDistance = 120f;

        [Header("Events")]

        [SerializeField, Tooltip("An event fired when the player first touches this control.")]
        private UnityEvent m_OnTouchStarted = null;
        [SerializeField, Tooltip("An event fired when a touch that started on this control is released.")]
        private UnityEvent m_OnTouchEnded = null;

        private InputAction m_Action = null;

        protected void OnValidate()
        {
            m_InputCurve.preWrapMode = WrapMode.Clamp;
            m_InputCurve.postWrapMode = WrapMode.Clamp;
        }

        protected override void Awake()
        {
            var controls = NeoFpsNewInputManager.controls;
            if (!string.IsNullOrEmpty(m_ActionID))
                m_Action = controls.FindAction(m_ActionID);

            base.Awake();

            if (m_Action == null)
                enabled = false;

            if (m_TouchMarkerTransform != null)
                m_TouchMarkerTransform.gameObject.SetActive(false);
        }

        public override bool HandleTouch(ETouch touch)
        {
            if (m_Action != null)
            {
                // Get the relative offset from the center
                var rect = rectTransform.rect;
                Vector2 world = rectTransform.TransformPoint(rect.center);
                Vector2 offset = touch.screenPosition - world;
                Vector2 normalised = offset / (rect.width * 0.5f);

                // Apply the input curve
                float magnitude = normalised.magnitude;
                if (magnitude > 0.01f)
                {
                    normalised *= m_InputCurve.Evaluate(magnitude) / magnitude;

                    controller.SetVirtualVector2D(m_Action.id, new Vector2(
                        normalised.x, normalised.y
                        ));
                }

                if (m_TouchMarkerTransform != null)
                {
                    m_TouchMarkerTransform.anchoredPosition = normalised * m_MaxMarkerDistance;
                }
            }

            return consume;
        }

        protected override void OnTouchStarted()
        {
            m_OnTouchStarted.Invoke();

            if (m_TouchMarkerTransform != null)
                m_TouchMarkerTransform.gameObject.SetActive(true);
        }

        protected override void OnTouchEnded()
        {
            m_OnTouchEnded.Invoke();

            if (m_TouchMarkerTransform != null)
                m_TouchMarkerTransform.gameObject.SetActive(false);
        }

#endif
    }
}