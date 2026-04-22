using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.WieldableTools;

namespace NeoFPS
{
	[RequireComponent(typeof(IWieldableTool))]
    [HelpURL("https://docs.neofps.com/manual/inputref-mb-inputsystemwieldabletool.html")]
    public class InputSystemWieldableTool : FpsInputSystemHandler
    {
#if ENABLE_INPUT_SYSTEM

        private IWieldableTool m_Tool = null;
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
			m_Tool = GetComponent<IWieldableTool>();
			m_Inspect = GetComponentInChildren<AnimatedWeaponInspect>(true);
		}

		protected override void OnEnable()
		{
			// Attach event handlers
			m_Character = m_Tool.wielder;
			if (m_Character != null)
			{
				m_Character.onControllerChanged += OnControllerChanged;
				m_Character.onIsAliveChanged += OnIsAliveChanged;
				OnControllerChanged(m_Character, m_Character.controller);
				OnIsAliveChanged(m_Character, m_Character.isAlive);
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			if (m_Character != null)
			{
				m_Character.onControllerChanged -= OnControllerChanged;
				m_Character.onIsAliveChanged -= OnIsAliveChanged;
			}
		}

		void OnControllerChanged(ICharacter character, IController controller)
		{
			m_IsPlayer = (controller != null && controller.isPlayer);
			if (m_IsPlayer && m_IsAlive)
				PushContext();
			else
				PopContext();
		}

		void OnIsAliveChanged(ICharacter character, bool alive)
		{
			m_IsAlive = alive;
			if (m_IsPlayer && m_IsAlive)
				PushContext();
			else
				PopContext();
		}

		protected override void OnLoseFocus()
		{
			m_Tool.PrimaryRelease();

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

			// Primary
			if (GetButtonDown(controls.Combat.PrimaryFire))
				m_Tool.PrimaryPress();
			if (GetButtonUp(controls.Combat.PrimaryFire))
				m_Tool.PrimaryRelease();

			// Secondary
			if (GetButtonDown(controls.Combat.SecondaryFire))
				m_Tool.SecondaryPress();
			if (GetButtonUp(controls.Combat.SecondaryFire))
				m_Tool.SecondaryRelease();

			// Interrupt
			if (GetButtonDown(controls.Combat.Reload))
				m_Tool.Interrupt();

			// Flashlight
			if (GetButtonDown(controls.Combat.Flashlight))
			{
				var flashlight = GetComponentInChildren<IWieldableFlashlight>(false);
				if (flashlight != null)
					flashlight.Toggle();
			}

			// Inspect
			if (m_Inspect != null)
				m_Inspect.inspecting = GetButton(controls.Inventory.InspectWeapon);
        }

#endif
    }
}