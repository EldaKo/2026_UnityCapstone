using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [TextArea] // 인스펙터에서 글을 쓰기 편하게 해줍니다
    public string messageToDisplay;

    // 아까 만든 IntroManager를 여기에 연결할 거예요
    private IntroManager introManager;

    void Start()
    {
        introManager = FindObjectOfType<IntroManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 태그를 가진 오브젝트가 들어왔을 때만!
        if (other.CompareTag("Player"))
        {
            introManager.PlayNewMessage(messageToDisplay);

            // 한 번만 나오게 하고 싶다면 이 오브젝트를 파괴하거나 끕니다.
            Destroy(gameObject);
        }
    }
}