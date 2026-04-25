using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/healthref-mb-damagezone.html")]
    public class DamageZone : CharacterTriggerZone, IDamageSource
    {
        [SerializeField, Tooltip("The amount of damage to apply to the player character per second.")]
        private float m_DamagePerSecond = 10f;
        [SerializeField, Tooltip("The type of damage to apply.")]
        private DamageType m_DamageType = DamageType.Default;
        [SerializeField, Tooltip("A description of the damage to use in logs, etc.")]
        private string m_DamageDescription = "Damage Zone";
        [SerializeField, Min(1), Tooltip("The amount of fixed frames to wait between damage ticks. Spacing out the damage into longer ticks can help with issues caused by very low damage values per tick.")]
        private int m_DamageInterval = 1;

        private DamageFilter m_OutDamageFilter = DamageFilter.AllDamageAllTeams;

        private Dictionary<int, IDamageHandler> m_DamageHandlers = new Dictionary<int, IDamageHandler>();
        private Dictionary<int, IHealthManager> m_HealthManagers = new Dictionary<int, IHealthManager>();
        private Coroutine m_DamageCoroutine = null;
        private WaitForFixedUpdate m_WaitForFixedUpdate = null;
        private float m_DamagePerTick = 0f;
        private int m_DamageTickCounter = 0;
        private static List<int> s_ToRemove = new List<int>();

        public float damagePerSecond
        {
            get { return m_DamagePerSecond; }
            protected set { m_DamagePerSecond = value; }
        }

        public DamageType damageType
        {
            get { return m_DamageType; }
            protected set
            {
                m_DamageType = value;
                m_OutDamageFilter.SetDamageType(m_DamageType);
            }
        }

        protected override void OnCharacterEntered(ICharacter c)
        {
            var handler = c.GetComponent<IDamageHandler>();
            if (handler != null)
                m_DamageHandlers.Add(c.gameObject.GetInstanceID(), handler);
            else
            {
                var hm = c.GetComponent<IHealthManager>();
                if (hm != null)
                    m_HealthManagers.Add(c.gameObject.GetInstanceID(), hm);
            }

            if (m_DamageCoroutine == null && (m_DamageHandlers.Count > 0 || m_HealthManagers.Count > 0))
                m_DamageCoroutine = StartCoroutine(DamageTick());
        }

        protected override void OnCharacterExited(ICharacter c)
        {
            int count = m_DamageHandlers.Count;

            int id = c.gameObject.GetInstanceID();
            m_DamageHandlers.Remove(id);
            m_HealthManagers.Remove(id);

            if (count > 0 && m_DamageHandlers.Count == 0 && m_HealthManagers.Count == 0)
            {
                StopCoroutine(m_DamageCoroutine);
                m_DamageCoroutine = null;
            }
        }

        protected void Awake()
        {
            m_OutDamageFilter.SetDamageType(m_DamageType);
            m_WaitForFixedUpdate = new WaitForFixedUpdate();

            // Calculate damage per tick
            m_DamagePerTick = m_DamagePerSecond * m_DamageInterval * Time.fixedDeltaTime;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            m_DamageHandlers.Clear();
            m_DamageCoroutine = null;
        }

        IEnumerator DamageTick()
        {
            m_DamageTickCounter = 1;
            while (m_DamageHandlers.Count > 0 || m_HealthManagers.Count > 0)
            {
                while (--m_DamageTickCounter >= 0)
                    yield return m_WaitForFixedUpdate;

                // Apply to damage handlers
                s_ToRemove.Clear();
                foreach (var kvp in m_DamageHandlers)
                {
                    if (kvp.Value is MonoBehaviour c && c != null && c.isActiveAndEnabled)
                        kvp.Value.AddDamage(m_DamagePerTick, this);
                    else
                        s_ToRemove.Add(kvp.Key);
                }
                foreach (int k in s_ToRemove)
                    m_DamageHandlers.Remove(k);

                // Apply to health managers
                s_ToRemove.Clear();
                foreach (var kvp in m_HealthManagers)
                {
                    if (kvp.Value is MonoBehaviour c && c != null && c.isActiveAndEnabled)
                        kvp.Value.AddDamage(m_DamagePerTick, false, this);
                    else
                        s_ToRemove.Add(kvp.Key);
                }
                foreach (int k in s_ToRemove)
                    m_HealthManagers.Remove(k);

                m_DamageTickCounter = m_DamageInterval;
            }

            m_DamageCoroutine = null;
        }

        #region IDamageSource IMPLEMENTATION

        public DamageFilter outDamageFilter
        {
            get { return m_OutDamageFilter; }
            set { m_OutDamageFilter = value; }
        }

        public IController controller
        {
            get { return null; }
        }

        public Transform damageSourceTransform
        {
            get { return transform; }
        }

        public string description
        {
            get { return m_DamageDescription; }
        }

        #endregion
    }
}