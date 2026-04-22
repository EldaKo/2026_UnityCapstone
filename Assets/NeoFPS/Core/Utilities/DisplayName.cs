using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class DisplayName : MonoBehaviour, IDisplayName
    {
        [SerializeField, Tooltip("")]
        private string m_DisplayName = string.Empty;

        public string displayName
        {
            get { return m_DisplayName; }
        }
    }
}