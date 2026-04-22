using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inputref-mb-menuinputsystemtoggle.html")]
    public class MenuInputSystemToggle : InputSystemMenu
    {
#if ENABLE_INPUT_SYSTEM

        [SerializeField, Tooltip("An event fired once the menu context is pushed.")]
        private UnityEvent m_OnEnterMenu = new UnityEvent();
        [SerializeField, Tooltip("An event fired once the menu context is popped.")]
        private UnityEvent m_OnExitMenu = new UnityEvent();

        protected override void Start()
        {
            base.Start();

            NeoFpsInputManagerBase.PushEscapeHandler(ToggleContext);
        }

        private void OnDestroy()
        {
            NeoFpsInputManagerBase.PopEscapeHandler(ToggleContext);
        }

        public void ToggleContext()
        {
            if (hasFocus)
                PopContext();
            else
                PushContext();
        }

        protected override void OnGainFocus()
        {
            base.OnGainFocus();
            m_OnEnterMenu.Invoke();
        }

        protected override void OnLoseFocus()
        {
            base.OnLoseFocus();
            m_OnExitMenu.Invoke();
        }

        protected override void OnEnable()
        {
            // Empty to prevent PushContext
        }

        protected override void OnDisable()
        {
            // Empty to prevent PopContext
        }

#endif
    }
}
