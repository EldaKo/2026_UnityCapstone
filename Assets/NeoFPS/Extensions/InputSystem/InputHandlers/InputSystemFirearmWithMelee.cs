using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPS.ModularFirearms;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NeoFPS
{
	[RequireComponent(typeof(IMeleeWeapon))]
    [HelpURL("https://docs.neofps.com/manual/inputref-mb-inputsystemfirearmwithmelee.html")]
    public class InputSystemFirearmWithMelee : InputSystemFirearm
    {
#if ENABLE_INPUT_SYSTEM

        [Header("Input")]
        [SerializeField, NeoFpsInputSystemActionRef, Tooltip("The action to trigger the melee attack.")]
        private string m_ActionID = string.Empty;

        private InputAction m_Action = null;
        protected IMeleeWeapon m_MeleeWeapon = null;
		protected IWieldable m_MeleeWieldable = null;
		protected IWieldable m_FirearmWieldable = null;
		private bool m_Pressed = false;

		protected override void OnAwake()
		{
			base.OnAwake();

            var controls = NeoFpsNewInputManager.controls;
            if (!string.IsNullOrEmpty(m_ActionID))
                m_Action = controls.FindAction(m_ActionID);

            m_MeleeWeapon = GetComponent<IMeleeWeapon>();

			if (m_MeleeWeapon != null)
			{
				m_MeleeWeapon.onAttackingChange += OnMeleeAttackingChanged;
				m_MeleeWieldable = m_MeleeWeapon as IWieldable;
			}

			if (m_Firearm != null)
			{
				if (m_Firearm.reloader != null)
				{
					m_Firearm.reloader.onReloadStart += OnReloadStart;
					m_Firearm.reloader.onReloadComplete += OnReloadComplete;
				}
				m_FirearmWieldable = m_Firearm as IWieldable;
			}
		}

        protected override void OnDisable()
        {
            base.OnDisable();

            m_Pressed = false;
        }

        private void OnMeleeAttackingChanged(bool attacking)
        {
            if (attacking)
                m_FirearmWieldable.AddBlocker(this);
            else
                m_FirearmWieldable.RemoveBlocker(this);
        }

        private void OnReloadStart(IModularFirearm firearm)
        {
            m_MeleeWieldable.AddBlocker(this);
        }

        private void OnReloadComplete(IModularFirearm firearm)
        {
            m_MeleeWieldable.RemoveBlocker(this);
        }

        protected override void AdditionalFirearmInput()
        {
            if (m_Pressed)
                m_Pressed = false;

            if (m_MeleeWeapon != null && m_Action.WasPressedThisFrame())
            {
                m_MeleeWeapon.PrimaryPress();
                m_Pressed = true;
            }
        }

#endif
    }
}