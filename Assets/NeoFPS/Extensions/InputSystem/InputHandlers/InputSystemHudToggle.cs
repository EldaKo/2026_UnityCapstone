using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class InputSystemHudToggle : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM

        [SerializeField, Tooltip("The gameobject containing the HUD UI. Must be a child of this object")]
        private GameObject m_HudObject = null;

        private static bool s_HudVisible = true;

        protected void OnValidate()
        {
            if (m_HudObject != null && (!m_HudObject.transform.IsChildOf(transform) || m_HudObject.transform == transform))
            {
                Debug.Log("Hud Object should be a child of the Hud Toggle");
                m_HudObject = null;
            }
        }

        protected void Start()
        {
            if (m_HudObject != null)
                m_HudObject.SetActive(s_HudVisible);
        }

        protected void Update()
        {
            // Get result
            var controls = NeoFpsNewInputManager.controls;
            if (controls.Miscellaneous.HUDToggle.WasPressedThisFrame())
            { 
                s_HudVisible = !s_HudVisible;
                if (m_HudObject != null)
                    m_HudObject.SetActive(s_HudVisible);
            }
        }

#endif
    }
}