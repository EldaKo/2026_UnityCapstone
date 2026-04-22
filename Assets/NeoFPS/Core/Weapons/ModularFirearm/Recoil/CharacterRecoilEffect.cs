using NeoCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    public class CharacterRecoilEffect : MonoBehaviour, IAdditiveTransform
    {
        private IAimController m_AimController = null;
        private AnimationCurve m_Curve = null;
        private Vector2 m_StartRotation = Vector2.zero;
        private Vector2 m_RecoilRotation = Vector2.zero;
        private Vector2 m_CurrentRotation = Vector2.zero;
        private float m_InverseDuration = 1f;
        private float m_Lerp = 1f;

        public IAdditiveTransformHandler transformHandler
        {
            get;
            private set;
        }

        public Quaternion rotation
        {
            get
            {
                if (m_Lerp < 1f)
                {
                    return Quaternion.Euler(m_CurrentRotation.x, m_CurrentRotation.y, 0f);
                }
                else
                    return Quaternion.identity;
            }
        }

        public Vector3 position
        {
            get { return Vector3.zero; }
        }

        public bool bypassPositionMultiplier
        {
            get { return true; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

        protected void Awake()
        {
            transformHandler = GetComponent<IAdditiveTransformHandler>();
            m_AimController = GetComponentInParent<IAimController>();
        }

        protected void OnEnable()
        {
            transformHandler.ApplyAdditiveEffect(this);
        }

        protected void OnDisable()
        {
            transformHandler.RemoveAdditiveEffect(this);
        }

        public void UpdateTransform()
        {
            if (m_Lerp < 1f)
            {
                m_Lerp += Time.deltaTime * m_InverseDuration;
                if (m_Lerp >= 1f)
                {
                    m_Lerp = 1f;
                    m_CurrentRotation = Vector2.zero;
                }
                else
                {
                    // Get the "spring from"
                    float eased = EasingFunctions.EaseInOutQuadratic(m_Lerp);
                    Vector2 springFrom = m_StartRotation * (1f - eased);

                    // Evaluate the spring curve
                    m_CurrentRotation = Vector2.LerpUnclamped(springFrom, m_RecoilRotation, m_Curve.Evaluate(m_Lerp));
                }
            }
        }

        public void AddRecoil (Vector2 recoil, float duration, AnimationCurve curve, float springReturn = 1f)
        {
            float aimFactor = 0f;

            if (m_AimController != null && springReturn < 0.999f)
            {
                aimFactor = (1f - springReturn);
                float yaw = recoil.x * aimFactor;
                float pitch = -recoil.y * aimFactor;
                recoil *= springReturn;
                m_AimController.AddRotation(yaw, pitch);
            }

            if (aimFactor < 0.999f)
            {
                // Set the animation curve
                m_Curve = curve;

                // Get the starting rotation
                m_StartRotation = m_CurrentRotation;

                // Calculate the new recoil rotation
                m_RecoilRotation = m_StartRotation + new Vector2(-recoil.y, recoil.x);

                // Sort timing
                m_InverseDuration = 1f / Mathf.Max(duration, 0.001f);
                m_Lerp = 0f;
            }
        }
    }
}