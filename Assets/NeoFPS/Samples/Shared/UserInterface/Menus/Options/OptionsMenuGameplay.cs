using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
	public class OptionsMenuGameplay : OptionsMenuPanel
	{
		[SerializeField] private MultiInputMultiChoice m_CrosshairColourChoice = null;
		[SerializeField] private MultiInputToggle m_AutoReloadToggle = null;
		[SerializeField] private MultiInputSlider m_HeadBobSlider = null;
		[SerializeField] private CrosshairColor[] m_CrosshairColorOptions = {
			new CrosshairColor { displayName = "White", color = new Color(1f, 1f, 1f, 0.5f) },
            new CrosshairColor { displayName = "Green", color = new Color(0f, 1f, 0f, 0.5f) },
            new CrosshairColor { displayName = "Red", color = new Color(1f, 0f, 0f, 0.5f) },
            new CrosshairColor { displayName = "Blue", color = new Color(0f, 0f, 1f, 0.5f) },
            new CrosshairColor { displayName = "Cyan", color = new Color(0f, 1f, 1f, 0.5f) },
            new CrosshairColor { displayName = "Magenta", color = new Color(1f, 0f, 1f, 0.5f) },
            new CrosshairColor { displayName = "Magenta", color = new Color(1f, 1f, 0f, 0.5f) }
        };

        [Serializable]
		private struct CrosshairColor
        {
            public string displayName;
            public Color color;
		}

		public override void Initialise (BaseMenu menu)
		{
			base.Initialise (menu);

			// Add listeners from code (saves user doing it as prefabs have a tendency to break)
			if (m_AutoReloadToggle != null)
				m_AutoReloadToggle.onValueChanged.AddListener(OnAutoReloadChanged);
			if (m_HeadBobSlider != null)
				m_HeadBobSlider.onValueChanged.AddListener(OnHeadBobChanged);

            m_CrosshairColourChoice.onIndexChanged.AddListener(OnCrosshairColourChanged);

			string[] crosshairOptions = new string[m_CrosshairColorOptions.Length];
			for (int i = 0; i < m_CrosshairColorOptions.Length; ++i)
				crosshairOptions[i] = m_CrosshairColorOptions[i].displayName;
			m_CrosshairColourChoice.options = crosshairOptions;

			m_CrosshairColourChoice.index = ColourToIndex(FpsSettings.gameplay.crosshairColor);
        }

		void OnAutoReloadChanged(bool value)
        {
			FpsSettings.gameplay.autoReload = value;
        }

		public void OnHeadBobChanged(int value)
		{
			FpsSettings.gameplay.headBob = value * 0.01f;
		}

		protected override void SaveOptions ()
		{
			FpsSettings.gameplay.Save ();
		}

		protected override void ResetOptions ()
		{
			m_CrosshairColourChoice.index = ColourToIndex (FpsSettings.gameplay.crosshairColor);

			if (m_AutoReloadToggle != null)
				m_AutoReloadToggle.value = FpsSettings.gameplay.autoReload;

			if (m_HeadBobSlider != null)
			{
				// Setup head bob from settings
				int current = Mathf.RoundToInt(FpsSettings.gameplay.headBob * 100f);
				m_HeadBobSlider.value = current;
			}
		}

		public void OnCrosshairColourChanged (int index)
		{
			FpsSettings.gameplay.crosshairColor = m_CrosshairColorOptions[index].color;
        }

		int ColourToIndex (Color c)
		{
			float bestDelta = float.MaxValue;
			int bestIndex = 0;

			for (int i = 0; i < m_CrosshairColorOptions.Length; ++i)
			{
				Color test = m_CrosshairColorOptions[i].color;
				float delta = 0f;
				delta += Mathf.Abs(test.r - c.r);
                delta += Mathf.Abs(test.g - c.g);
                delta += Mathf.Abs(test.b - c.b);
                delta += Mathf.Abs(test.a - c.a);

				if (delta < bestDelta)
				{
					bestDelta = delta;
					bestIndex = i;
				}
            }

			return bestIndex;
		}
	}
}