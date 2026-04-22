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
#endif

    public class NeoFpsInputSystemTouchInventoryButton : BaseInputSystemTouchControl
    {
#if ENABLE_INPUT_SYSTEM

        [Header("Input")]
        [SerializeField, NeoFpsInputSystemActionRef, Tooltip("The quick slot selection actions corresponding to the slot buttons. Tapping a slot on the screen will pick the action with the correct index from this list.")]
        private string[] m_ActionIDs = { };

        HudInventoryItemBase m_HudInventoryItem = null;
        private InputAction m_Action = null;

        protected override void Awake()
        {
            base.Awake();

            m_HudInventoryItem = GetComponent<HudInventoryItemBase>();
            if (m_HudInventoryItem == null)
                gameObject.SetActive(false);
            else
            {
                var controls = NeoFpsNewInputManager.controls;
                int index = m_HudInventoryItem.slotIndex;
                if (index < m_ActionIDs.Length && !string.IsNullOrEmpty(m_ActionIDs[index]))
                    m_Action = controls.FindAction(m_ActionIDs[index]);
                if (m_Action == null)
                    gameObject.SetActive(false);
            }
        }

        public override bool HandleTouch(ETouch touch)
        {
            if (m_Action != null)
                controller.SetVirtualButton(m_Action.id, true);

            return consume;
        }

        protected override void OnTouchStarted() { }
        protected override void OnTouchEnded() { }

#endif
    }
}