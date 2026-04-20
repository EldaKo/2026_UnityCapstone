using UnityEngine;

public class ItemDisableWatcher : MonoBehaviour
{
    private IntroManager introManager;
    public string message = "총기를 획득했습니다. 좌클릭으로 발사할 수 있습니다.";
    void Start()
    {
        introManager = FindObjectOfType<IntroManager>();
    }
    void OnDisable()
    {
        // 아이템이 비활성화/삭제될 때 실행됨
        if (introManager != null)
        {
            introManager.PlayNewMessage(message);
        }
    }
}