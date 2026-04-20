using UnityEngine;
using TMPro;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;
    public float typingSpeed = 0.05f; // 글자가 써지는 속도 (낮을수록 빠름)
    public bool hasTriggeredGunTutorial = false;

    void Start()
    {
        // 첫 번째 메시지 시작
        StartCoroutine(SequenceIntro());
    }
    // IntroManager.cs에 이 함수를 추가하세요
    public void PlayNewMessage(string newMessage)
    {
        // 이미 나오고 있는 대사가 있다면 멈추고 새로 시작
        StopAllCoroutines();
        StartCoroutine(TypeText(newMessage));
    }
    IEnumerator SequenceIntro()
    {
        // 1. 첫 번째 문장 타이핑
        yield return StartCoroutine(TypeText("안녕, 당신은 폐허가 된 도시의 기억잃은 생존자입니다."));

        // 다 써지고 나서 2초 대기
        yield return new WaitForSeconds(2f);

        // 2. 두 번째 문장 타이핑
        yield return StartCoroutine(TypeText("지금부터 지시에 따라 행동해주시길 바랍니다."));


        // 다 써지고 나서 2초 대기
        yield return new WaitForSeconds(2f);

        // 2. 두 번째 문장 타이핑
        yield return StartCoroutine(TypeText("WASD키를 눌러 앞으로 이동해주십시오."));

        // 마지막 메시지 보여주고 3초 뒤 삭제
        yield return new WaitForSeconds(3f);
        subtitleText.text = "";
    }

    // 실제 타이핑을 담당하는 핵심 함수
    IEnumerator TypeText(string message)
    {
        subtitleText.text = ""; // 일단 텍스트 비우기

        foreach (char letter in message.ToCharArray())
        {
            subtitleText.text += letter; // 한 글자씩 추가
            yield return new WaitForSeconds(typingSpeed); // 설정한 속도만큼 대기
        }
    }
}
