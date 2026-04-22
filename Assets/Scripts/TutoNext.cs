using UnityEngine;
using TMPro;
using System.Collections;

public class TutoNext : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;
    public float typingSpeed = 0.05f;
    public void StartTuto()
    {
        StopAllCoroutines();
        StartCoroutine(SequenceIntro());
    }

    IEnumerator SequenceIntro()
    {
    
        yield return StartCoroutine(TypeText("이제 반짝이는 과녁을 향해 총기를 발사해봅시다."));

        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(TypeText("듀듀듇듀ㅠ."));

        yield return new WaitForSeconds(3f);
        subtitleText.text = "";
    }

    // 타이핑 효과
    IEnumerator TypeText(string message)
    {
        subtitleText.text = "";

        foreach (char letter in message)
        {
            subtitleText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}