using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using System.Collections;

namespace NeoFPS
{
    public class InputCharacterFlashlight : FpsInput
    {
        [SerializeField, Tooltip("The flashlight to toggle")]
        private GameObject m_FlashlightObject = null;

        private IWieldableFlashlight m_Flashlight = null;

        public override FpsInputContext inputContext
        {
            get { return FpsInputContext.Character; }
        }

        protected void Reset()
        {
            if (TryGetComponent(out IWieldableFlashlight flashlight))
                m_FlashlightObject = (flashlight as Component).gameObject;
        }

        protected void OnValidate()
        {
            if (m_FlashlightObject != null && !m_FlashlightObject.TryGetComponent(out IWieldableFlashlight _))
            {
                Debug.LogError("Object must have a behaviour which implements IWieldableFlashlight attached");
                m_FlashlightObject = null;
            }
        }

        protected override void OnAwake()
        {
            if (m_FlashlightObject != null)
                m_Flashlight = m_FlashlightObject.GetComponent<IWieldableFlashlight>();
        }

        protected override void UpdateInput()
        {
            if (GetButtonDown(FpsInputButton.Flashlight) && m_Flashlight != null)
                m_Flashlight.Toggle();
        }
    }
}