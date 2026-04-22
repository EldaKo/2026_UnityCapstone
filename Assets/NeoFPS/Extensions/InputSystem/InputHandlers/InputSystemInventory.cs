using UnityEngine;
using NeoFPS.Constants;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NeoFPS
{
    [RequireComponent(typeof(ICharacter))]
    [HelpURL("https://docs.neofps.com/manual/inputref-mb-inputsysteminventory.html")]
    public class InputSystemInventory : CharacterInputHandlerBase
    {
#if ENABLE_INPUT_SYSTEM

        [Header("Input Properties")]

        [SerializeField, Range(0.1f, 1f), Tooltip("The delay between repeating input when holding the next or previous weapon buttons.")]
        private float m_RepeatDelay = 0.25f;

        [SerializeField, Range(0.01f, 1f), Tooltip("The delay between repeating input when rolling the mouse scroll wheel.")]
        private float m_ScrollDelay = 0.1f;

        [Header("Inputs")]

        [SerializeField, NeoFpsInputSystemActionRef, Tooltip("The input actions corresponding to each slot. If you have quick-melee / thrown inputs then you can map them to specific slots here.")]
        private string[] m_SlotActionPaths = {
            "Inventory/Quick-Slot 1",
            "Inventory/Quick-Slot 2",
            "Inventory/Quick-Slot 3",
            "Inventory/Quick-Slot 4",
            "Inventory/Quick-Slot 5",
            "Inventory/Quick-Slot 6",
            "Inventory/Quick-Slot 7",
            "Inventory/Quick-Slot 8",
            "Inventory/Quick-Slot 9",
            "Inventory/Quick-Slot 10"
        };

        private float m_WeaponCycleTimeout = 0f;
        private float m_ScrollTimer = 0f;
        private InputAction[] m_SlotActions = null;

        protected override void OnAwake()
        {
            base.OnAwake();

            var controls = NeoFpsNewInputManager.controls;
            m_SlotActions = new InputAction[m_SlotActionPaths.Length];
            for (int i = 0; i < m_SlotActionPaths.Length; ++i)
            {
                if (!string.IsNullOrEmpty(m_SlotActionPaths[i]))
                m_SlotActions[i] = controls.FindAction(m_SlotActionPaths[i]);
            }
        }

        protected override void UpdateInput()
        {
            if (!allowWeaponInput)
                return;

            // Switch weapons			
            if (m_Character.quickSlots != null)
            {
                // Main controls
                var controls = NeoFpsNewInputManager.controls;

                int weaponCycle = 0;
                if (m_WeaponCycleTimeout == 0f)
                {
                    // Get cycle direction
                    if (GetButton(controls.Inventory.PreviousWeapon))
                        weaponCycle -= 1;
                    if (GetButton(controls.Inventory.NextWeapon))
                        weaponCycle += 1;

                    // Cycle weapon
                    switch (weaponCycle)
                    {
                        case 1:
                            {
                                m_Character.quickSlots.SelectNextSlot();
                                m_WeaponCycleTimeout = m_RepeatDelay;
                                break;
                            }
                        case -1:
                            {
                                m_Character.quickSlots.SelectPreviousSlot();
                                m_WeaponCycleTimeout = m_RepeatDelay;
                                break;
                            }
                    }
                }
                else
                {
                    // Get cycle direction
                    if (GetButtonUp(controls.Inventory.PreviousWeapon) || GetButtonUp(controls.Inventory.NextWeapon))
                        m_WeaponCycleTimeout = 0f;
                    else
                    {
                        // Reduce repeat timeout
                        m_WeaponCycleTimeout -= Time.deltaTime;
                        if (m_WeaponCycleTimeout < 0f)
                            m_WeaponCycleTimeout = 0f;
                    }
                }

                // Quick-switch
                if (GetButtonDown(controls.Inventory.SwitchWeapon))
                    m_Character.quickSlots.SwitchSelection();

                // Quickslots
                for (int i = 0; i < m_SlotActions.Length; ++i)
                {
                    if (m_SlotActions[i] != null && m_SlotActions[i].WasPressedThisFrame())
                        m_Character.quickSlots.SelectSlot(i);
                }

                // Holster
                if (GetButtonDown(controls.Inventory.HolsterWeapon))
                    m_Character.quickSlots.SelectSlot(-1);

                // Drop selected weapon
                if (GetButtonDown(controls.Inventory.DropWeapon))
                    m_Character.quickSlots.DropSelected();

                // Mouse scroll
                if (m_ScrollTimer == 0f)
                {
                    float scroll = GetAxis(controls.Inventory.InventoryScroll);
                    if (scroll > 0.075f)
                    {
                        m_Character.quickSlots.SelectNextSlot();
                        m_ScrollTimer = m_ScrollDelay;
                    }
                    if (scroll < -0.075f)
                    {
                        m_Character.quickSlots.SelectPreviousSlot();
                        m_ScrollTimer = m_ScrollDelay;
                    }
                }
                else
                {
                    m_ScrollTimer -= Time.unscaledDeltaTime;
                    if (m_ScrollTimer < 0f)
                        m_ScrollTimer = 0f;
                }
            }
        }

#endif
    }
}