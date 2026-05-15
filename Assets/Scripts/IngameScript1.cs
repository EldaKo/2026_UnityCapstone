using UnityEngine;
using TMPro;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("OBJ setting")]
    public GameObject targetObject;
    public TextMeshProUGUI subtitleText;
    public float typingSpeed = 0.05f;
    public float pauseBetween = 2f;   // 대사 사이 쉬는 시간
    public float holdAfterType = 1.5f; // 다 쓴 후 유지 시간

    void Start()
    {
        StartCoroutine(SequenceIntro());
    }

    IEnumerator SequenceIntro()
    {
        yield return StartCoroutine(TypeText("안녕, 당신은 폐허가 된 도시의 기억잃은 생존자입니다."));
        yield return new WaitForSeconds(holdAfterType);

        yield return StartCoroutine(FadeOut());
        yield return new WaitForSeconds(pauseBetween);

        yield return StartCoroutine(TypeText("지금부터 지시에 따라 행동해주시길 바랍니다."));
        yield return new WaitForSeconds(holdAfterType);

        yield return StartCoroutine(FadeOut());
        yield return new WaitForSeconds(pauseBetween);

        yield return StartCoroutine(TypeText("WASD키를 눌러 앞으로 이동해주십시오."));
        yield return new WaitForSeconds(holdAfterType);

        yield return StartCoroutine(FadeOut());
    }

    IEnumerator TypeText(string message)
    {
        subtitleText.text = "";
        subtitleText.alpha = 1f;
        foreach (char letter in message)
        {
            subtitleText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    IEnumerator FadeOut()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            subtitleText.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }
        subtitleText.text = "";
        subtitleText.alpha = 1f;
    }

    // 아래는 기존 코드 유지
    public void StartMoveSequence()
    {
        StopAllCoroutines();
        StartCoroutine(MoveSequence());
    }

    public void StartGunSequence()
    {
        StopAllCoroutines();
        StartCoroutine(GunSequence());
    }

    IEnumerator MoveSequence()
    {
        yield return StartCoroutine(TypeText("잘했습니다. 이제 F키를 눌러 눈 앞의 총기를 주워봅시다."));
    }

    IEnumerator GunSequence()
    {
        if (targetObject != null)
            targetObject.SetActive(true);
        yield return StartCoroutine(TypeText("총기를 획득했습니다. 좌클릭으로 발사할 수 있습니다."));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(TypeText("이제 반짝이는 과녁을 향해 총기를 발사해봅시다."));
    }
}