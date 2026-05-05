using System.Collections;
using UnityEngine;

/// <summary>
/// 출구 존. 플레이어가 들어왔을 때 모든 아이템을 모았으면 클리어,
/// 아니면 안내 메시지를 띄운다.
/// 동시에 진행 상황을 IMGUI로 화면에 표시한다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitZone : MonoBehaviour
{
    [Header("UI")]
    public bool showHud = true;
    public Color hudTextColor = Color.white;

    [Header("Cooldown")]
    [Tooltip("부족한 상태로 들어왔을 때 메시지 재표시까지의 쿨다운(초)")]
    public float promptCooldown = 2f;

    private bool cleared;
    private string transientMessage;
    private float transientUntil;
    private GUIStyle hudStyle;
    private GUIStyle bigStyle;
    private GUIStyle smallStyle;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (cleared) return;
        if (!other.CompareTag("Player")) return;

        if (EscapeItemRegistry.HasAll())
        {
            Clear();
        }
        else
        {
            int got = EscapeItemRegistry.CollectedCount;
            int need = EscapeItemRegistry.RequiredCount;
            ShowTransient($"아이템이 부족합니다 ({got}/{need})");
        }
    }

    private void Clear()
    {
        cleared = true;
        Debug.Log("[Escape] CLEAR! 맵 탈출 성공");
        // 게임 시간 정지 (간단한 클리어 처리)
        StartCoroutine(FreezeAfterDelay(0.5f));
    }

    private IEnumerator FreezeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Time.timeScale = 0f;
    }

    private void ShowTransient(string message)
    {
        transientMessage = message;
        transientUntil = Time.unscaledTime + promptCooldown;
    }

    private void EnsureStyles()
    {
        if (hudStyle == null)
        {
            hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
            };
            hudStyle.normal.textColor = hudTextColor;
        }
        if (bigStyle == null)
        {
            bigStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 60,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            bigStyle.normal.textColor = Color.white;
        }
    }

    private void EnsureSmallStyle()
    {
        if (smallStyle != null) return;
        smallStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.UpperLeft,
        };
        smallStyle.normal.textColor = new Color(1f, 0.95f, 0.55f, 0.95f);
    }


    private void OnGUI()
    {
        if (!showHud) return;
        EnsureStyles();

        int got = EscapeItemRegistry.CollectedCount;
        int need = EscapeItemRegistry.RequiredCount;
        string statusLine = EscapeItemRegistry.HasAll()
            ? "아이템 " + got + "/" + need + "  ▶  출구로!"
            : "아이템 " + got + "/" + need;

        GUI.Label(new Rect(20, 20, 600, 40), statusLine, hudStyle);

        DrawProximityNotice();

        if (!string.IsNullOrEmpty(transientMessage) && Time.unscaledTime < transientUntil)
        {
            var rect = new Rect(0, Screen.height * 0.55f, Screen.width, 50);
            GUI.Label(rect, transientMessage, bigStyle);
        }

        if (cleared)
        {
            var bgRect = new Rect(0, 0, Screen.width, Screen.height);
            var prevColor = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.6f);
            GUI.DrawTexture(bgRect, Texture2D.whiteTexture);
            GUI.color = prevColor;

            var rect = new Rect(0, Screen.height * 0.4f, Screen.width, 100);
            GUI.Label(rect, "CLEAR!", bigStyle);

            var subRect = new Rect(0, Screen.height * 0.5f, Screen.width, 50);
            GUI.Label(subRect, "맵을 탈출했습니다", hudStyle.WithCenterTopAnchor());
        }
    }

    private void DrawProximityNotice()
    {
        var tracker = EscapeProximityTracker.Instance;
        if (tracker == null) return;
        var closest = tracker.ClosestItem;
        if (closest == null) return;

        if (EscapeItemRegistry.Has(closest.itemId)) return;

        EnsureSmallStyle();

        string label = ItemIdToLabel(closest.itemId);
        float dist = tracker.ClosestDistance;
        string text = "✨ 근처에 " + label + " 있음 (" + dist.ToString("F1") + "m)";

        GUI.Label(new Rect(20, 55, 400, 28), text, smallStyle);
    }

    private static string ItemIdToLabel(string id)
    {
        switch (id)
        {
            case "redKey":   return "빨간 열쇠가";
            case "greenKey": return "초록 열쇠가";
            case "blueKey":  return "파란 열쇠가";
            default:         return "아이템이";
        }
    }
}

internal static class GUIStyleExtensions
{
    public static GUIStyle WithCenterTopAnchor(this GUIStyle s)
    {
        return new GUIStyle(s) { alignment = TextAnchor.UpperCenter };
    }
}
