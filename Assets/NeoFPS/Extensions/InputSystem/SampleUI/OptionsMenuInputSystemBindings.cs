using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoFPS.Constants;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace NeoFPS.Samples
{
	public class OptionsMenuInputSystemBindings : OptionsMenuPanel
	{
#if ENABLE_INPUT_SYSTEM

        [SerializeField]
		private Transform m_ContainerTransform = null;
		[SerializeField]
		private MultiInputButton m_ResetToDefaultsButton = null;
		[SerializeField]
		private MultiInputGroup m_PrototypeDivider = null;
        [SerializeField]
		private MultiInputNewKeyBinding m_PrototypeBinding = null;
		[SerializeField]
		private string[] m_IgnoreActions = { };

#if UNITY_EDITOR
		[HideInInspector] public bool expandRebindableButtons = false;
#endif

		private List<MultiInputGroup> m_Dividers = null;
        private List<MultiInputNewKeyBinding> m_Entries = null;

        public override void Initialise (BaseMenu menu)
		{
			base.Initialise (menu);
			LoadOptions ();
			onBackButtonPressed += menu.HidePanel;
		}

        public override void Hide()
        {
            base.Hide();
            SaveOptions();
        }

		void OnRebound()
        {
			for (int i = 0; i < m_Entries.Count; ++i)
				m_Entries[i].ResetDisplayNames();
        }

        protected void LoadOptions ()
		{
			// Hook in reset controls
			if (m_ResetToDefaultsButton != null)
				m_ResetToDefaultsButton.onClick.AddListener(OnResetToDefaultsButtonClicked);

			// Instantiate entries
			m_Entries = new List<MultiInputNewKeyBinding>();
				
			Dictionary<string, List<MultiInputNewKeyBinding>> sorted = new Dictionary<string, List<MultiInputNewKeyBinding>>();

			var mc = NeoFpsNewInputManager.controls;
			foreach (var map in mc.asset.actionMaps)
			{
				foreach (var action in map.actions)
				{
					// Only rebind button types
					if (action.type == InputActionType.Button)
					{
						// Check against ignore list
						var idString = action.id.ToString();
						bool ignored = false;
						for (int i = 0; i < m_IgnoreActions.Length; ++i)
						{
							if (idString == m_IgnoreActions[i])
                            {
								ignored = true;
								break;
                            }
						}

						if (!ignored)
						{
							// Create menu entry
							var entry = m_Entries.Count == 0 ? m_PrototypeBinding : Instantiate(m_PrototypeBinding);
							entry.Initialise(mc, action, NeoFpsNewInputManager.keyboardAndMouseBindingGroup);
							entry.onRebound += OnRebound;

							// Add to entries list
							m_Entries.Add(entry);

							// Set the category
							string category = map.name;
							if (sorted.ContainsKey(category))
								sorted[category].Add(entry);
							else
							{
								List<MultiInputNewKeyBinding> list = new List<MultiInputNewKeyBinding>();
								list.Add(entry);
								sorted.Add(category, list);
							}
						}
					}
				}
			}

			// Set up heirarchy
			m_Dividers = new List<MultiInputGroup>(sorted.Count);
			m_Dividers.Add(m_PrototypeDivider);
			for (int i = 1; i < sorted.Count; ++i)
				m_Dividers.Add(Instantiate<MultiInputGroup> (m_PrototypeDivider));
			int itr = 0;
			foreach (string key in sorted.Keys)
			{
				m_Dividers [itr].label = (key == string.Empty) ? "Misc" : key;
				m_Dividers [itr].transform.SetParent (m_ContainerTransform);
				m_Dividers [itr].transform.localScale = Vector3.one;

				List<MultiInputNewKeyBinding> category = sorted [key];
				GameObject[] groupContents = new GameObject[category.Count];
				for (int i = 0; i < category.Count; ++i)
				{
					category [i].transform.SetParent (m_ContainerTransform);
					category [i].transform.localScale = Vector3.one;
					groupContents [i] = category [i].gameObject;
				}
				m_Dividers [itr].contents = groupContents;

				++itr;
			}
		}

        private void OnResetToDefaultsButtonClicked()
        {
			NeoFpsNewInputManager.ResetKeyBindings();
		}

		protected override void SaveOptions()
		{
			NeoFpsNewInputManager.SaveBindings();
		}
		protected override void ResetOptions ()
		{
			NeoFpsNewInputManager.LoadBindings();
		}

#else

		protected override void SaveOptions () {}
		protected override void ResetOptions () {}

#endif
    }
}