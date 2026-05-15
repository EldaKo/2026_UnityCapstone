using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HideoutUIManager : MonoBehaviour
{
    public static HideoutUIManager Instance;

    [Header("UI 연결")]
    public GameObject uiPanel; 
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText; // 이제 "필요 열쇠"를 표시합니다.
    
    [Header("버튼 설정")]
    public Button upgradeButton;
    public Button useButton; 
    public Button exitToMainButton; 

    private FacilityScript currentSelectedFacility;

    void Awake()
    {
        Instance = this;
        if (uiPanel != null) uiPanel.SetActive(false);
        
        // 버튼 안전장치 및 연결
        if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        if (useButton != null) useButton.onClick.AddListener(OnUseButtonClicked);
        if (exitToMainButton != null) exitToMainButton.onClick.AddListener(GoToMainScreen);
    }

    // [도우미] 시설 타입에 따라 필요한 열쇠 ID와 이름을 정합니다.
    private (string id, string name) GetRequiredKeyInfo(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Armor: return ("blueKey", "파란 열쇠");
            case UpgradeType.Weapon: return ("redKey", "빨간 열쇠");
            default: return ("greenKey", "초록 열쇠");
        }
    }

    public void OpenFacilityUI(FacilityScript facility)
    {
        currentSelectedFacility = facility;
        UpdateUIData();
        if (uiPanel != null) uiPanel.SetActive(true);

        if (facility.cameraViewPoint != null && HideoutCamera.Instance != null)
            HideoutCamera.Instance.MoveToFacility(facility.cameraViewPoint);
    }

    public void UpdateUIData()
    {
        if (currentSelectedFacility == null) return;

        nameText.text = currentSelectedFacility.facilityName;
        descText.text = currentSelectedFacility.description;
        iconImage.sprite = currentSelectedFacility.facilityIcon;
        levelText.text = $"Level: {currentSelectedFacility.currentLevel}";
        
        // --- 열쇠 체크 로직 ---
        var keyInfo = GetRequiredKeyInfo(currentSelectedFacility.upgradeType);
        bool hasKey = ItemHideout.Instance.HasKey(keyInfo.id);

        if (hasKey)
        {
            costText.text = $"<color=green>필요 아이템: {keyInfo.name} (보유 중)</color>";
            upgradeButton.interactable = true;
        }
        else
        {
            costText.text = $"<color=red>필요 아이템: {keyInfo.name} (부족함)</color>";
            upgradeButton.interactable = false;
        }
    }

    public void OnUpgradeButtonClicked()
    {
        if (currentSelectedFacility == null) return;

        var keyInfo = GetRequiredKeyInfo(currentSelectedFacility.upgradeType);

        // 열쇠가 진짜 있는지 확인 후 소모
        if (ItemHideout.Instance.HasKey(keyInfo.id))
        {
            ItemHideout.Instance.RemoveKey(keyInfo.id); // 열쇠 소모!
            currentSelectedFacility.LevelUp();
            UpdateUIData(); // UI 갱신 (열쇠가 사라졌으므로 버튼이 다시 비활성화됨)
        }
    }

    public void OnUseButtonClicked()
    {
        if (currentSelectedFacility != null && currentSelectedFacility.functionUIPanel != null)
        {
            uiPanel.SetActive(false);
            currentSelectedFacility.functionUIPanel.SetActive(true);
        }
    }

    public void CloseUI()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
        if (currentSelectedFacility != null && currentSelectedFacility.functionUIPanel != null)
            currentSelectedFacility.functionUIPanel.SetActive(false);
        if (HideoutCamera.Instance != null) HideoutCamera.Instance.ReturnToTopView();
    }

    public void GoToMainScreen()
    {
        SceneManager.LoadScene("mainScreen");
    }
}