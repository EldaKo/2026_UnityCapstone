using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
	[RequireComponent (typeof (IMeleeWeapon))]
    [HelpURL("https://docs.neofps.com/manual/inputref-mb-inputsystemmeleeweapon.html")]
    public class InputSystemMeleeWeapon : FpsInputSystemHandler
    {
#if ENABLE_INPUT_SYSTEM

        private IMeleeWeapon m_MeleeWeapon = null;
        private ICharacter m_Character = null;
		private AnimatedWeaponInspect m_Inspect = null;
		private bool m_IsPlayer = false;
		private bool m_IsAlive = false;

        public override FpsInputContext inputContext
        {
            get { return FpsInputContext.Character; }
        }

        protected override void OnAwake()
        {
            m_MeleeWeapon = GetComponent<IMeleeWeapon>();
			m_Inspect = GetComponentInChildren<AnimatedWeaponInspect>(true);
		}

        protected override void OnEnable()
        {
			// Get the wielding character
			IInventoryItem invItem = GetComponent<IInventoryItem>();
			if (invItem != null)
				m_Character = invItem.owner;
			else
				m_Character = null;

			// Check character found
			if (m_Character == null)
				return;

			// Attach event handlers
			m_Character.onControllerChanged += OnControllerChanged;
			m_Character.onIsAliveChanged += OnIsAliveChanged;
			OnControllerChanged (m_Character, m_Character.controller);
			OnIsAliveChanged (m_Character, m_Character.isAlive);
		}

		protected override void OnDisable ()
		{
			base.OnDisable();

			if (m_Character != null)
			{
				m_Character.onControllerChanged -= OnControllerChanged;
				m_Character.onIsAliveChanged -= OnIsAliveChanged;
			}
		}

		void OnControllerChanged (ICharacter character, IController controller)
		{
			m_IsPlayer = (controller != null && controller.isPlayer);
			if (m_IsPlayer && m_IsAlive)
				PushContext();
			else
				PopContext();
		}

		void OnIsAliveChanged (ICharacter character, bool alive)
		{
			m_IsAlive = alive;
			if (m_IsPlayer && m_IsAlive)
				PushContext();
			else
				PopContext();
		}

        protected override void OnLoseFocus()
		{
			m_MeleeWeapon.PrimaryRelease();
			m_MeleeWeapon.SecondaryRelease();

			// Inspect
			if (m_Inspect != null)
				m_Inspect.inspecting = false;
		}

        protected override void UpdateInput()
		{
			if (m_Character != null && !m_Character.allowWeaponInput)
				return;

			// Main controls
			var controls = NeoFpsNewInputManager.controls;

			if (GetButtonDown(controls.Combat.PrimaryFire))
				m_MeleeWeapon.PrimaryPress();
			if (GetButtonUp(controls.Combat.PrimaryFire))
				m_MeleeWeapon.PrimaryRelease();

			if (GetButtonDown(controls.Combat.SecondaryFire))
				m_MeleeWeapon.SecondaryPress();
			if (GetButtonUp(controls.Combat.SecondaryFire))
				m_MeleeWeapon.SecondaryRelease();

			// Inspect
			if (m_Inspect != null)
				m_Inspect.inspecting = GetButton(controls.Inventory.InspectWeapon);
        }

#endif
    }
}