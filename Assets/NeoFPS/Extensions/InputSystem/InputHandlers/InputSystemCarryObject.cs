using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using System.Collections;

namespace NeoFPS
{
	[RequireComponent(typeof(ICarrySystem))]
    [HelpURL("https://docs.neofps.com/manual/inputref-mb-inputsystemcarryobject.html")]
    public class InputSystemCarryObject : CharacterInputHandlerBase
    {
#if ENABLE_INPUT_SYSTEM

        private ICarrySystem m_CarrySystem = null;
		private bool m_LockCamera = false;

		void BlockCameraInputs(bool blocked)
		{
			if (m_LockCamera == !blocked)
			{
				m_LockCamera = blocked;

				var aim = m_Character.aimController;
				if (m_LockCamera)
				{
					aim.SetPitchConstraints(aim.pitch, aim.pitch);
					aim.SetYawConstraints(aim.forward, 0f);
				}
				else
				{
					aim.ResetPitchConstraints();
					aim.ResetYawConstraints();
				}
			}
		}

		protected override void OnAwake()
		{
			base.OnAwake();

			m_CarrySystem = GetComponent<ICarrySystem>();
		}

		protected override void UpdateInput()
		{
			bool manipulating = false;

			// Main controls
			var controls = NeoFpsNewInputManager.controls;

			switch (m_CarrySystem.carryState)
			{
				case CarryState.ValidTarget:
					{
						// Pick up
						if (GetButtonDown(controls.Interaction.PickUp))
							m_CarrySystem.PickUpObject();
					}
					break;
				case CarryState.Carrying:
					{
						// Manipulate
						if (GetButton(controls.Combat.SecondaryFire))
						{
							manipulating = true;
							Vector2 m_MouseDelta = GetVector2(controls.Movement.MouseLook) * 0.05f;
							Vector2 m_Analogue = GetVector2(controls.Movement.AnalogueLook);
							m_CarrySystem.ManipulateObject(m_MouseDelta, m_Analogue);
						}

						// Drop
						if (GetButtonDown(controls.Interaction.PickUp))
							m_CarrySystem.DropObject();

						// Throw
						if (GetButtonDown(controls.Combat.PrimaryFire))
							m_CarrySystem.ThrowObject();
					}
					break;
			}

			// Block camera input if manipulating the object
			BlockCameraInputs(manipulating);
        }

#endif
    }
}