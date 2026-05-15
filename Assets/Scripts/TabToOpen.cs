using UnityEngine;

public class TabToOpen : MonoBehaviour
{
    // 유니티 인스펙터에서 'BackGInven' 오브젝트를 여기에 드래그해서 넣으세요.
    public GameObject inventoryWindow;

    void Start()
    {
        // 게임 시작 시에는 인벤토리를 꺼둡니다.
        if (inventoryWindow != null)
        {
            inventoryWindow.SetActive(false);
        }
    }

    void Update()
    {
        // Tab 키를 눌렀을 때 실행
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryWindow != null)
        {
            // 현재 활성 상태를 반전시킵니다 (켜져있으면 끄고, 꺼져있으면 켬)
            bool isActive = inventoryWindow.activeSelf;
            inventoryWindow.SetActive(!isActive);

            // 인벤토리가 열릴 때 마우스 커서를 보이고 조작을 멈추는 로직 (필요 시)
            if (!isActive) // 이제 열린 상태라면
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                // Time.timeScale = 0f; // 게임을 일시정지하고 싶다면 추가
            }
            else // 이제 닫힌 상태라면
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                // Time.timeScale = 1f; // 게임을 다시 재생
            }
        }
    }
}