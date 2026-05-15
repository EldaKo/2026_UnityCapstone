using UnityEngine;

namespace NeoFPS
{
	[RequireComponent(typeof (Collider))]
    [HelpURL("https://docs.neofps.com/manual/healthref-mb-basicdamagehandler.html")]
	public class BasicDamageHandler : MonoBehaviour, IDamageHandler, IHasHealthManager
    {
		// 들어오는 데미지에 곱해질 배율.
		[SerializeField, Tooltip("The value to multiply any incoming damage by. Use to reduce damage to areas like feet, or raise it for areas like the head.")]
		private float m_Multiplier = 1f;
		// 특정 부위에 데미지를 입을 시, 크리티컬 (치명타) 판정 여부
		[SerializeField, Tooltip("Does the damage count as critical. Used to change the feedback for the damage taker and dealer.")]
		private bool m_Critical = false;
		// 글로벌 피격 이벤트 발생 여부
        [SerializeField, Tooltip("The global hit event is used by the crosshair to show hit markers, and can be subscribed. In some cases you might not want this (eg decorative scene elements).")]
        private bool m_FireGlobalHitEvent = true;

		// 현재 오브젝트의 충돌체를 저장할 변수
        private Collider m_Collider = null;
		
		// 캐릭터의 전체 체력을 실질적으로 관리하는 매니저 스크립트를 연결할 속성.
		public IHealthManager healthManager
		{
			get;
			private set;
		}

		// 체력 매니저가 부착된 기준 Transform
        public Transform healthTransform
        {
            get;
            private set;
        }

#if UNITY_EDITOR

// 유니티 에디터의 인스펙터에서 값이 변경될 때마다 호출
// 데미지 배율이 음수가 되는 것을 방지하여, 공격받았을 때 체력이 회복되는 등의 버그를 막음.
        protected virtual void OnValidate ()
		{
			if (m_Multiplier < 0f)
				m_Multiplier = 0f;
		}
#endif

// 게임이 시작될 때(오브젝트가 활성화될 때) 가장 먼저 실행되는 초기화 함수.
		protected virtual void Awake ()
		{
			// 상위 부모 객체들을 탐색하여 전체 체력을 관리하는 컴포넌트를 찾음.
			healthManager = GetComponentInParent<IHealthManager>();
			if (healthManager == null)
			{
				// 체력 관리자가 없다면 자기 자신을 기준으로 삼음.
				healthTransform = transform;
				// 데미지 배율이 0 이하라면 데미지를 받을 일이 없으므로 최적화를 위해 스크립트를 비활성화.
                if (m_Multiplier <= 0f)
                    enabled = false;
            }
			else
			// 체력 관리자를 정상적으로 찾았다면 해당 Transform을 기준으로 설정함.
                healthTransform = healthManager.transform;

			// 충돌체를 할당받고, 만약 충돌체가 없다면 에러 콘솔을 띄워 개발자에게 알림.
            m_Collider = GetComponent<Collider>();
			Debug.Assert(m_Collider != null, "Damage handlers should only be placed on objects with a collider");
		}

		#region IDamageHandler implementation

		// 외부에서 들어오는 데미지를 필터링하는 속성.
		private DamageFilter m_InDamageFilter = DamageFilter.AllDamageAllTeams;
		public DamageFilter inDamageFilter 
		{
			get { return m_InDamageFilter; }
			set { m_InDamageFilter = value; }
		}
		// [내부 전용 핵심 함수] Raycast 충돌 지점 정보(Hit)가 없을 때 데미지를 계산하고 적용함.
		// 1.2: Remove raycast hit. It's redundant in health manager imo (원 개발자 왈 : health manager에는 필요없어서 raycast hit 제거 해도 된다고 해도 됨.)
		protected DamageResult AddDamageInternal(IDamageSource source, float inDamage, out float outDamage)
		{
			// 데미지 소스가 존재하지만 아군 사격(Friendly Fire) 제한 등으로 인해 타격이 불가능한 경우
			if (source != null && !CheckDamageCollision(source))
			{
				outDamage = 0f;
				return DamageResult.Blocked;
			}
			else
			{
				// 들어온 데미지에 현재 부위의 배율을 곱해 최종 데미지(outDamage)를 계산힘.
				outDamage = inDamage * m_Multiplier;
				// 크리티컬 부위 여부에 따라 데미지 타입을 결정함.
				var result = m_Critical ? DamageResult.Critical : DamageResult.Standard;

				if (outDamage > 0f && source != null && source.controller != null)
					source.controller.currentCharacter.ReportTargetHit(m_Critical);

				// 상위 체력 매니저에게 최종 계산된 데미지 수치를 전달하여 실제 체력을 깎음.
				healthManager.AddDamage(outDamage, m_Critical, source);

				return result;
			}
		}
		// [내부 전용 핵심 함수] 총알 등이 꽂힌 정확한 위치 정보(RaycastHit)를 함께 받아 처리할 때 사용함.

		protected DamageResult AddDamageInternal(IDamageSource source, RaycastHit hit, float inDamage, out float outDamage)
        {
			// 팀킬 여부 등 필터 검사
			if (source != null && !CheckDamageCollision(source))
			{
				outDamage = 0f;
				return DamageResult.Blocked;
			}
			else
			{
				// 부위 배율 적용
				outDamage = inDamage * m_Multiplier;
				var result = m_Critical ? DamageResult.Critical : DamageResult.Standard;

				// 공격자에게 타격 성공 알림
				if (outDamage > 0f && source != null && source.controller != null)
					source.controller.currentCharacter.ReportTargetHit(m_Critical);
				
				// 체력 매니저에게 데미지와 함께 맞은 위치(hit) 정보도 전달하여 피격 혈흔(Blood decal)이나 파티클 생성 등에 쓰이게 함.
				healthManager.AddDamage(outDamage, m_Critical, source, hit);

				return result;
			}
		}

		// [외부 노출 함수] 단순 데미지 수치만 들어왔을 때 호출됨. (독가스, 추락 데미지 등 특정 주체가 없을 때 유용)
		public virtual DamageResult AddDamage (float damage)
		{
			if (enabled && healthManager != null)
            {
				var result = AddDamageInternal(null, damage, out damage);
			
			// 글로벌 히트 이벤트를 뿌려 시스템 단에서 타격 이펙트를 처리하게 함.. 맞은 위치는 현재 충돌체의 중심점(bounds.center)으로 처리함.
                if (m_FireGlobalHitEvent)
                    DamageEvents.ReportDamageHandlerHit(this, null, m_Collider.bounds.center, result, damage);

				return result;
			}
			else
				return DamageResult.Ignored;
		}

		// [외부 노출 함수] 데미지 수치와 공격 주체(source) 정보가 함께 들어왔을 때 호출함.
		public virtual DamageResult AddDamage (float damage, IDamageSource source)
		{
			if (enabled && healthManager != null)
            {
				var result = AddDamageInternal(source, damage, out damage);

				// 정확한 타격 지점(hit.point)을 바탕으로 이벤트를 발생시킴.
                if (m_FireGlobalHitEvent)
                    DamageEvents.ReportDamageHandlerHit(this, source, m_Collider.bounds.center, result, damage);

				return result;
			}
			else
				return DamageResult.Ignored;
		}

		// [외부 노출 함수] 데미지 수치와 물리적 타격 지점(hit) 정보가 들어왔을 때 호출됨.
        public virtual DamageResult AddDamage(float damage, RaycastHit hit)
		{
			if (enabled && healthManager != null)
            {
				var result = AddDamageInternal(null, hit, damage, out damage);

				// 정확한 타격 지점(hit.point)을 바탕으로 이벤트를 발생시킴.
                if (m_FireGlobalHitEvent)
                    DamageEvents.ReportDamageHandlerHit(this, null, hit.point, result, damage);

				return result;
			}
			else
				return DamageResult.Ignored;
		}

		// [외부 노출 함수] 데미지, 타격 지점, 공격 주체 정보가 모두 포함된 가장 완전한 형태의 함수. (주로 총기 사격 시 사용됨)
        public virtual DamageResult AddDamage(float damage, RaycastHit hit, IDamageSource source)
		{
			// 데미지 처리를 시작하기 전에 아군 사격 등의 필터(CheckDamageCollision)를 가장 먼저 체크함.
			// Apply damage
			if (enabled && healthManager != null && m_Multiplier > 0f && CheckDamageCollision(source))
			{
				var result = AddDamageInternal(source, hit, damage, out damage);

                if (m_FireGlobalHitEvent)
                    DamageEvents.ReportDamageHandlerHit(this, source, hit.point, result, damage);

				return result;
			}
			else
				return DamageResult.Ignored;
		}
		// 들어온 데미지 소스가 현재 게임의 설정(예: 아군 오인 사격 허용 여부 등)에 따라 타격이 가능한 대상인지 검사하는 헬퍼 함수.
		bool CheckDamageCollision(IDamageSource source)
		{
			// 공격 주체가 존재하고, 그 공격 주체의 데미지 필터가 현재 이 객체의 필터와 충돌 가능한 관계인지 반환.
			return !(source != null && !source.outDamageFilter.CollidesWith(inDamageFilter, FpsGameMode.friendlyFire));
		}

        #endregion
    }
}