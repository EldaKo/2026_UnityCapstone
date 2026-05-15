﻿using System;
using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Events;

namespace NeoFPS
{
    // 캐릭터(플레이어, 적 등)의 체력을 관리하는 기본 컴포넌트입니다.
    // IHealthManager 인터페이스를 구현하여 데미지를 받고, INeoSerializableComponent를 통해 세이브/로드를 지원합니다.
    [HelpURL("https://docs.neofps.com/manual/healthref-mb-basichealthmanager.html")]
    public class BasicHealthManager : MonoBehaviour, IHealthManager, INeoSerializableComponent
    {
        // 게임 시작 시 캐릭터가 가질 기본 체력입니다. 인스펙터에서 최대 체력을 넘지 못하도록 제한(MaxValue)되어 있습니다.
        [Tooltip("The starting health of the character.")]
        [MaxValue("m_HealthMax"), SerializeField] private float m_Health = 100f;
        
        // 캐릭터의 최대 체력 한도입니다.
        [Tooltip("The maximum health of the character.")]
        [Delayed, SerializeField] private float m_HealthMax = 100f;
        
        // 폭발물 등 자신이 발생시킨 데미지에 스스로 피해를 입을지 여부입니다. (로켓 점프 등을 구현할 때 유용합니다)
        [Tooltip("Can the character damage itself (eg with explosives).")]
        [SerializeField] private bool m_CanDamageSelf = true;
        
        // 체력이 변경될 때마다 유니티 인스펙터 창에서 연결할 수 있는 이벤트입니다. (예: 체력바 UI 업데이트, 피격 시 화면 붉어짐 효과 등)
        [Tooltip("An event called whenever the health changes")]
        [SerializeField] private FloatEvent m_OnHealthChanged = null;
        
        // 생존 여부(살았는지/죽었는지)가 바뀔 때 호출되는 이벤트입니다. (예: 사망 애니메이션 재생, 게임 오버 화면 출력 등)
        [Tooltip("An event called whenever the alive state of the health manager changes")]
        [SerializeField] private BoolEvent m_OnIsAliveChanged = null;

        // 세이브/로드(직렬화) 시스템에서 이 컴포넌트의 특정 데이터를 찾기 위해 사용하는 고유 키값들입니다.
        private static readonly NeoSerializationKey k_HealthKey = new NeoSerializationKey("health");
        private static readonly NeoSerializationKey k_HealthMaxKey = new NeoSerializationKey("healthMax");
        private static readonly NeoSerializationKey k_IsAliveKey = new NeoSerializationKey("isAlive");
        private static readonly NeoSerializationKey k_InvincibleKey = new NeoSerializationKey("invincible");

        // 다른 C# 스크립트들에서 체력 변화나 사망 이벤트를 구독(Subscribe)할 수 있도록 열어둔 델리게이트 이벤트들입니다.
        public event HealthDelegates.OnIsAliveChanged onIsAliveChanged;
        public event HealthDelegates.OnHealthChanged onHealthChanged;
        public event HealthDelegates.OnHealthMaxChanged onHealthMaxChanged;

        // 이 캐릭터를 조종하는 컨트롤러(플레이어의 입력 또는 AI)를 저장하는 변수입니다. 자해(Self-damage) 판정에 쓰입니다.
        private IController m_Controller = null;

        // 유니티 인스펙터에 노출하기 위한 커스텀 이벤트 클래스 정의입니다. (float 값 전달용)
        [Serializable]
        public class FloatEvent : UnityEvent<float>
        {
        }

        // 유니티 인스펙터에 노출하기 위한 커스텀 이벤트 클래스 정의입니다. (bool 값 전달용)
        [Serializable]
        public class BoolEvent : UnityEvent<bool>
        {
        }

        // 무적 상태 여부입니다. true면 데미지를 받아도 체력이 깎이지 않습니다. (치트키, 리스폰 직후 무적 시간 등에 사용)
        public bool invincible
        {
            get;
            set;
        } = false;

        // 내부적으로 살아있는지 여부를 저장하는 변수입니다.
        private bool m_IsAlive = true;
        
        // 외부에서 생존 여부를 읽을 수 있는 속성입니다. 값이 변경되면 자동으로 사망/부활 이벤트를 발생시킵니다.
        public bool isAlive
        {
            get { return m_IsAlive; }
            protected set
            {
                if (m_IsAlive != value)
                {
                    m_IsAlive = value;
                    OnIsAliveChanged(); // 상태가 바뀌면 이벤트 호출
                }
            }
        }

        // 현재 체력에 접근하는 속성입니다. 값을 대입하면 SetHealth 함수를 거쳐 안전하게 처리됩니다.
        public float health
        {
            get { return m_Health; }
            set { SetHealth(value, false, null); }
        }

        // 최대 체력에 접근하는 속성입니다.
        public float healthMax
        {
            get { return m_HealthMax; }
            set
            {
                if (m_HealthMax != value)
                {
                    float old = m_HealthMax;
                    // 새로운 최대 체력값 설정
                    m_HealthMax = value;
                    // 최대 체력이 0보다 작아지지 않도록 방어
                    if (m_HealthMax < 0f)
                        m_HealthMax = 0f;
                    // 최대 체력 변경 이벤트 발생
                    OnMaxHealthChanged(old, m_HealthMax);
                    // 만약 현재 체력이 새로 설정된 최대 체력보다 높다면, 최대 체력에 맞춰 깎아냅니다.
                    if (health > m_HealthMax)
                        health = m_HealthMax;
                }
            }
        }
		
        // 정규화된 체력 (0.0 ~ 1.0 사이의 값)을 반환하거나 설정합니다. 주로 UI의 체력바(Fill Amount)를 채울 때 매우 유용합니다.
		public float normalisedHealth
		{
			get { return health / healthMax; }
			set { health = value * healthMax; }
		}

        // 유니티 에디터에서 인스펙터 값을 수정할 때 호출되어 비정상적인 값(예: 음수 체력)이 들어가는 것을 막습니다.
        protected virtual void OnValidate()
        {
            m_HealthMax = Mathf.Clamp(m_HealthMax, 1f, 10000f);
            m_Health = Mathf.Clamp(m_Health, 1f, m_HealthMax);
        }

        // 컴포넌트가 초기화될 때 실행됩니다.
        protected virtual void Awake()
        {
            var character = GetComponent<ICharacter>();
            if (character != null)
            {
                // 캐릭터의 조종권(Controller)이 바뀔 때마다 이를 감지하여 변수를 업데이트하도록 이벤트를 연결합니다.
                character.onControllerChanged += OnCharacterControllerChanged;
                OnCharacterControllerChanged(character, character.controller);
            }
        }

        // 컴포넌트가 파괴될 때 메모리 누수를 막기 위해 연결했던 이벤트를 해제합니다.
        protected virtual void OnDestroy()
        {
            var character = GetComponent<ICharacter>();
            if (character != null)
                character.onControllerChanged += OnCharacterControllerChanged; // 주의: 네오FPS 원본 로직상 '-=' 가 아닌 '+='로 되어 있는 부분이나, 요청에 따라 수정하지 않음.
        }

        // 컨트롤러(플레이어/AI)가 변경될 때 호출되어 현재 컨트롤러를 갱신합니다.
        void OnCharacterControllerChanged(ICharacter character, IController controller)
        {
            m_Controller = controller;
        }

        // [내부 헬퍼 함수] 들어온 데미지 소스가 체력을 깎을 수 있는 유효한 소스인지 검사합니다.
        protected virtual bool CheckDamageSource(IDamageSource source)
        {
            // 데미지 주체가 없다면(환경 데미지 등) 일단 허용합니다.
            if (source == null)
                return true;

            // 만약 나 자신에게 피해를 줄 수 없는 설정(m_CanDamageSelf == false)인데, 공격자가 나 자신이라면
            if (!m_CanDamageSelf && m_Controller != null && source.controller == m_Controller)
            {
                // 예외 처리: 추락 데미지나 익사 데미지는 공격자가 나 자신이어도 무조건 피해를 입어야 합니다.
                if (source.outDamageFilter.IsDamageType(DamageType.Fall))
                    return true;
                if (source.outDamageFilter.IsDamageType(DamageType.Drowning))
                    return true;

                // 그 외의 자해 데미지(자신이 던진 수류탄 등)는 무시합니다.
                return false;
            }
            return true; // 그 외 일반적인 공격은 모두 허용합니다.
        }

        // [핵심 로직] 체력을 실질적으로 증감시키는 가장 중요한 함수입니다. 모든 데미지/회복 처리는 결국 이 함수를 거칩니다.
        public void SetHealth(float h, bool critical, IDamageSource source)
        {
            // 무적이 아니거나, 현재 체력보다 더 높은 체력(즉, 회복)이 들어온 경우에만 로직을 실행합니다. (무적일 때는 깎이지 않음)
            if (!invincible || h >= m_Health)
            {
                float old = m_Health;
                // 체력을 0부터 최대 체력 사이로 제한하여 설정합니다.
                m_Health = Mathf.Clamp(h, 0f, m_HealthMax);
                
                // 체력에 실제로 변화가 생겼다면
                if (!Mathf.Approximately(m_Health, old))
                {
                    // 체력 변경 이벤트를 발생시키고
                    OnHealthChanged(old, m_Health, critical, source);
                    // 체력이 0 초과인지 확인하여 생존 여부(isAlive)를 갱신합니다. 0 이하라면 사망 처리됩니다.
                    isAlive = (m_Health > 0.0001f); // Checks if new value == old automatically
                }
            }
        }

        // 체력이 변경되었을 때 델리게이트와 유니티 이벤트를 호출하여 연결된 다른 스크립트(UI 등)에 알립니다.
        protected virtual void OnHealthChanged(float from, float to, bool critical, IDamageSource source)
        {
            // Fire event
            if (onHealthChanged != null)
                onHealthChanged(from, to, critical, source);
            m_OnHealthChanged.Invoke(to);
        }

        // 최대 체력이 변경되었을 때 델리게이트를 호출합니다.
        protected virtual void OnMaxHealthChanged(float from, float to)
        {
            // Fire event
            if (onHealthMaxChanged != null)
                onHealthMaxChanged(from, to);
        }

        // 생존 여부가 변경되었을 때 델리게이트와 유니티 이벤트를 호출합니다. (사망 처리 등에 사용)
        protected virtual void OnIsAliveChanged ()
        {
            if (onIsAliveChanged != null)
                onIsAliveChanged(m_IsAlive);
            m_OnIsAliveChanged.Invoke(m_IsAlive);
        }

        // [외부 노출 함수] 단순 수치만큼 데미지를 입힙니다.
        public virtual void AddDamage(float damage)
        {
            SetHealth(health - damage, false, null);
        }

        // [외부 노출 함수] 데미지 수치와 크리티컬 여부를 받아 데미지를 입힙니다.
        public virtual void AddDamage(float damage, bool critical)
        {
            SetHealth(health - damage, critical, null);
        }

        // [외부 노출 함수] 공격 주체 정보와 함께 데미지를 입힙니다. (자해 판정 포함)
        public virtual void AddDamage(float damage, IDamageSource source)
        {
            if (CheckDamageSource(source))
                SetHealth(health - damage, false, source);
        }

        // [외부 노출 함수] 공격 주체 정보 및 크리티컬 여부와 함께 데미지를 입힙니다.
        public virtual void AddDamage(float damage, bool critical, IDamageSource source)
        {
            if (CheckDamageSource(source))
                SetHealth(health - damage, critical, source);
        }

        // [외부 노출 함수] 타격 지점(Hit)을 포함해 데미지를 입힙니다.
        public void AddDamage(float damage, bool critical, RaycastHit hit)
        {
            SetHealth(health - damage, critical, null);
        }

        // [외부 노출 함수] 타격 지점, 주체, 크리티컬 모두 포함된 가장 상세한 데미지 함수입니다.
        public void AddDamage(float damage, bool critical, IDamageSource source, RaycastHit hit)
        {
            if (CheckDamageSource(source))
                SetHealth(health - damage, critical, source);
        }

        // 이벤트를 발생시키지 않고 조용히 체력 수치만 강제로 설정합니다. (리스폰이나 초기화 시 UI 알림을 막기 위해 사용)
        public virtual void SetHealthSilent(float h)
        {
            m_Health = Mathf.Clamp(h, 0f, m_HealthMax);
        }

        // 이벤트를 발생시키지 않고 0~1 사이의 정규화된 값으로 체력을 강제로 설정합니다.
        public virtual void SetNormalisedHealthSilent(float h)
        {
            m_Health = Mathf.Clamp(m_HealthMax * h, 0f, m_HealthMax);
        }

        // 단순 수치만큼 체력을 회복시킵니다. (메디킷 등)
        public virtual void AddHealth(float h)
        {
            SetHealth(health + h, false, null);
        }

        // 회복 주체 정보와 함께 체력을 회복시킵니다.
        public virtual void AddHealth(float h, IDamageSource source)
        {
            SetHealth(health + h, false, source);
        }

        // [세이브/로드 지원] 게임을 저장할 때 현재 체력, 최대 체력, 생존 여부, 무적 상태를 파일에 기록합니다.
        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_HealthMaxKey, healthMax);
            writer.WriteValue(k_HealthKey, health);
            writer.WriteValue(k_IsAliveKey, isAlive);
            writer.WriteValue(k_InvincibleKey, invincible);
        }

        // [세이브/로드 지원] 저장된 게임을 불러올 때 기록된 체력 상태들을 읽어와 복구합니다.
        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            float floatValue;
            if (reader.TryReadValue(k_HealthMaxKey, out floatValue, healthMax))
                healthMax = floatValue;
            if (reader.TryReadValue(k_HealthKey, out floatValue, health))
                health = floatValue;

            bool boolValue;
            if (reader.TryReadValue(k_IsAliveKey, out boolValue, true))
                isAlive = boolValue;
            if (reader.TryReadValue(k_InvincibleKey, out boolValue, false))
                invincible = boolValue;
        }
    }
}