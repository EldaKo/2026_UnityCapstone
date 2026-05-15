using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [Header("열쇠 정보")]
    public string keyId = "blueKey";       // 열쇠 고유 ID (blueKey, greenKey, redKey)
    public string displayName = "파란 열쇠"; // UI에 띄울 이름

    [Header("애니메이션 효과")]
    public float bobAmplitude = 0.12f;     // 위아래로 움직이는 폭
    public float bobFrequency = 1.2f;      // 움직이는 속도
    public float rotateSpeed = 80f;        // 회전 속도

    [Header("효과음")]
    public AudioClip pickupSound;          // 획득 시 재생할 사운드

    private Vector3 startPos;

    void Start()
    {
        // 시작 위치 기억
        startPos = transform.position;
    }

    void Update()
    {
        // 열쇠가 둥둥 떠다니며 회전하는 멋진 효과
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
        float newY = startPos.y + Mathf.Sin(Time.time * Mathf.PI * 2f * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    // 플레이어가 열쇠에 몸을 부딪혔을 때 (Collider의 IsTrigger가 체크되어 있어야 함)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectKey();
        }
    }

    // ※ 만약 NeoFPS의 상호작용(F키 줍기)을 사용 중이라면 이 함수를 그 이벤트에 연결하세요.
    public void CollectKey()
    {
        if (ItemHideout.Instance != null)
        {
            // 1. 매니저에 열쇠 저장! (씬이 넘어가도 유지됨)
            ItemHideout.Instance.AddKey(keyId);
            
            // 2. 사운드 재생
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // 3. 필드에서 열쇠 삭제
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("씬에 ItemHideout 매니저가 없어서 열쇠를 저장할 수 없습니다!");
        }
    }
}