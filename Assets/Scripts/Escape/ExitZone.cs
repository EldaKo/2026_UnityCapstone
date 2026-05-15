using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // [필수] 이거 없으면 SceneManager 오류 납니다

[RequireComponent(typeof(Collider))]
public class ExitZone : MonoBehaviour
{
    public float promptCooldown = 2.5f;

    [Header("열쇠 근접 알림")]
    public float keyNearbyDistance = 8f;

    private bool cleared;
    private float nextPromptTime;

    private bool showFail;
    private float failUntil;
    private bool showClear;
    private bool showKeyNearby;

    private GUIStyle bigStyle;
    private GUIStyle hudStyle;
    private GUIStyle boxStyle;
    private GUIStyle btnStyle;
    private GUIStyle nearbyStyle;

    private Transform playerTransform;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Update()
    {
        if (playerTransform == null)
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        if (playerTransform != null && !EscapeItemRegistry.HasAll())
            showKeyNearby = IsKeyNearby();
        else
            showKeyNearby = false;
    }

    bool IsKeyNearby()
    {
        KeyPickup[] keys = Object.FindObjectsByType<KeyPickup>(FindObjectsSortMode.None);
        foreach (var key in keys)
        {
            if (Vector3.Distance(playerTransform.position, key.transform.position) <= keyNearbyDistance)
                return true;
        }
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (cleared || !other.CompareTag("Player")) return;

        if (EscapeItemRegistry.HasAll())
        {
            cleared = true;
            showClear = true;
            // 마우스 커서 활성화
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            StartCoroutine(FreezeAfter(0.5f));
        }
        else
        {
            if (Time.time < nextPromptTime) return;
            nextPromptTime = Time.time + promptCooldown;
            showFail = true;
            failUntil = Time.unscaledTime + 3f;
        }
    }

    IEnumerator FreezeAfter(float t)
    {
        yield return new WaitForSeconds(t);
        Time.timeScale = 0f; // 게임 일시정지
    }

    void EnsureStyles()
    {
        if (hudStyle != null) return;

        hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperLeft };
        hudStyle.normal.textColor = Color.white;

        bigStyle = new GUIStyle(GUI.skin.label) { fontSize = 52, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        bigStyle.normal.textColor = Color.white;

        boxStyle = new GUIStyle(GUI.skin.box) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        boxStyle.normal.textColor = Color.white;

        btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 24, fontStyle = FontStyle.Bold };

        nearbyStyle = new GUIStyle(GUI.skin.box) { fontSize = 16, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        nearbyStyle.normal.textColor = Color.yellow;
    }

    void OnGUI()
    {
        EnsureStyles();

        // ── 상단 HUD
        int got  = EscapeItemRegistry.CollectedCount;
        int need = EscapeItemRegistry.RequiredCount;
        string hudText = EscapeItemRegistry.HasAll() ? $"열쇠 {got}/{need}  ▶  출구로 가세요!" : $"열쇠 {got}/{need}";
        GUI.Label(new Rect(20, 20, 400, 36), hudText, hudStyle);

        // ── 실패/근처 알림 로직 (생략 가능하나 기존 유지)
        if (showFail && Time.unscaledTime < failUntil)
        {
            GUI.Box(new Rect(20, 62, 260, 64), $"열쇠가 {need - got}개 부족합니다", boxStyle);
        }

        if (showKeyNearby && !cleared)
        {
            GUI.Box(new Rect(20, 130, 260, 40), "열쇠가 근처에 있어.", nearbyStyle);
        }

        // ── [수정됨] 클리어 화면 및 버튼
        if (showClear)
        {
            // 배경 어둡게
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 중앙 메시지
            GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 80), "ESCAPE CLEAR!", bigStyle);

            // [수정] 버튼 위치 계산 및 이동
            float btnWidth = 250f;
            float btnHeight = 60f;
            float btnX = (Screen.width - btnWidth) / 2f;
            float btnY = Screen.height * 0.65f; // [변경] 0.52f에서 0.65f로 내려서 버튼을 아래로 배치

            if (GUI.Button(new Rect(btnX, btnY, btnWidth, btnHeight), "은신처로 이동", btnStyle))
            {
                // [매우 중요] 일시정지 상태를 풀고 씬을 이동해야 합니다!
                Time.timeScale = 1f; 
                Debug.Log("은신처 씬으로 이동합니다.");
                SceneManager.LoadScene("Hideout"); 
            }
        }
    }
}