using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS
{
    public class HudWeaponName : WieldableHudBase
    {
        [SerializeField, Tooltip("The text component to write the weapon name to")]
        Text m_Text = null;

        private IDisplayName m_DisplayName = null;

        protected override void AttachToSelection(IQuickSlotItem wieldable)
        {
            m_DisplayName = wieldable.GetComponent<IDisplayName>();
        }

        protected override void DetachFromSelection(IQuickSlotItem wieldable)
        {
            m_DisplayName = null;
        }

        protected override void ResetUI()
        {
            if (m_Text != null)
            {
                if (m_DisplayName as Component != null)
                {
                    m_Text.text = m_DisplayName.displayName;
                    gameObject.SetActive(true);
                }
                else
                {
                    m_Text.text = string.Empty;
                    gameObject.SetActive(false);
                }
            }
        }
    }
}