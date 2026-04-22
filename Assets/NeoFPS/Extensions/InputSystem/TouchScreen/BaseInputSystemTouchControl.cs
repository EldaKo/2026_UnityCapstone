using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    public abstract class BaseInputSystemTouchControl : MonoBehaviour, INeoFpsInputSystemTouchControl
    {
#if ENABLE_INPUT_SYSTEM

        [Header ("Touch Control")]

        [SerializeField, Tooltip("The priority this control should have in terms of appearing over another. Higher numbers will cover controls with lower priority.")]
        private int m_Priority = 0;
        [SerializeField, Tooltip("Should the touch input be consumed or fall through to the control underneath this.")]
        private bool m_Consume = true;

        protected NeoFpsInputSystemTouchScreenController controller { get; private set; }

        public int priority { get { return m_Priority; } }

        public bool consume { get { return m_Consume; } }

        public RectTransform rectTransform { get; private set; }

        private int m_TouchCount = 0;

        protected virtual void Awake()
        {
            rectTransform = transform as RectTransform;
            controller = GetComponentInParent<NeoFpsInputSystemTouchScreenController>();
            if (controller == null)
                gameObject.SetActive(false);
        }

        protected void OnEnable()
        {
            controller.RegisterTouchControl(this);
        }

        protected void OnDisable()
        {
            controller.UnregisterTouchControl(this);
        }

        public void AddTouch()
        {
            if (++m_TouchCount == 1)
                OnTouchStarted();

            // Safety check
            Debug.Assert(m_TouchCount <= Input.touchCount);
        }

        public void RemoveTouch()
        {
            if (--m_TouchCount == 0)
                OnTouchEnded();

            // Safety check
            Debug.Assert(m_TouchCount >= 0);
        }

        protected abstract void OnTouchStarted();
        protected abstract void OnTouchEnded();
        public abstract bool HandleTouch(UnityEngine.InputSystem.EnhancedTouch.Touch touch);

#endif
    }
}
