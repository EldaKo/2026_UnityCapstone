using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NeoFPS.Constants;
using System.Collections.Generic;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace NeoFPS.Samples
{
	public class MultiInputNewKeyBinding : MultiInputButtonGroup
    {
#if ENABLE_INPUT_SYSTEM

		private IInputActionCollection2 m_ControlMap = null;
        private EventSystem m_EventSystem = null;
		private InputAction m_InputAction = null;
		private List<BindingButton> m_Bindings = new List<BindingButton>();
		private int m_CurrentlyRebinding = -1;

		class BindingButton
        {
			public Text textField = null;
			public int bindingIndex = -1;

			public void GetBindingDisplayName(InputAction action)
			{
				textField.text = action.GetBindingDisplayString(bindingIndex);
            }
        }

		public FpsInputButton button
		{
			get;
			private set;
		}

		protected override void Awake ()
		{
			base.Awake ();
			for (int i = 0; i < buttons.Length; ++i)
			{
				int index = i;
				buttons[i].onClick.AddListener(() => SetBinding(index));
			}
		}

		public void Initialise (IInputActionCollection2 map, InputAction action, string group)
		{
			m_ControlMap = map;
			m_InputAction = action;

			// Set label
			label = action.name;

			// Get rebinding buttons
			int targetCount = 0;
			for (int i = 0; i < buttons.Length; ++i)
			{
				if (buttons[i].rect != null)
				{
					m_Bindings.Add(new BindingButton() { textField = buttons[i].rect.GetComponentInChildren<Text>() });
					++targetCount;
				}
			}

			int found = 0;
			for (int i = 0; i < action.bindings.Count && found < targetCount; ++i)
			{
				// Filter by group if required
				if (group != string.Empty && (action.bindings[i].groups == null || !action.bindings[i].groups.Contains(group)))
					continue;

				// Does this need some horrific dictionary of dictionaries thing, or re-architecting to use shared groups???
				// Currently just saying group is contained in groups will skip over any cases where partial names are used

				m_Bindings[found++].bindingIndex = i;
			}

			// Set text entries
			for (int i = 0; i < found; ++i)
				m_Bindings[i].GetBindingDisplayName(m_InputAction);
		}

		public void SetBinding (int index)
		{
			m_CurrentlyRebinding = index;

			if (m_Bindings[index].bindingIndex != -1)
			{
				m_Bindings[index].textField.text = "???";
				PlayAudio(MenuAudio.ClickValid);

				m_EventSystem = EventSystem.current;
				m_EventSystem.enabled = false;

				NeoFpsNewInputManager.RemapKeyBinding(m_InputAction, m_Bindings[index].bindingIndex, OnCompleted);
			}
		}

		void OnCompleted()
		{
			m_Bindings[m_CurrentlyRebinding].GetBindingDisplayName(m_InputAction);

			m_EventSystem.enabled = true;
			m_EventSystem.SetSelectedGameObject(gameObject);

			onRebound?.Invoke();
		}

		public event Action onRebound;

		public void ResetDisplayNames()
		{
			for (int i = 0; i < m_Bindings.Count; ++i)
				m_Bindings[i].GetBindingDisplayName(m_InputAction);
		}

#endif
    }
}

