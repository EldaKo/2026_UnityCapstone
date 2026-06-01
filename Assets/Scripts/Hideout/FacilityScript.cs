using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 1. SpecialEffect(특수 효과) 타입 추가
public enum UpgradeType
{
    None,
    Armor,
    Weapon,
    Health,
    SpecialEffect
}

[System.Serializable]
public class MaterialRequirement
{
    public ItemData material;
    [Tooltip("레벨 1 → 2 업그레이드 시 필요량. 실제 요구치 = baseAmount × currentLevel")]
    public int baseAmount = 1;
}

[System.Serializable]
public class NeedItemSlot
{
    public GameObject root;
    [Tooltip("Assets/Inventory/icon 등에서 Sprite 드래그. 비우면 ItemData.icon 자동 사용.")]
    public Sprite iconSprite;
    public TextMeshProUGUI nameText;
    [Tooltip("비워두면 nameText에 '이름 X/Y' 합쳐서 표시")]
    public TextMeshProUGUI countText;
}

public class FacilityScript : MonoBehaviour
{
    [Header("시설 정보")]
    public string facilityName = "방탄복 시설";
    [TextArea]
    public string description = "방어구를 강화합니다.";
    public Sprite facilityIcon;

    [Header("현재 상태")]
    public int currentLevel = 1;

    [Header("카메라 연출")]
    [Tooltip("시설 클릭 시 카메라가 이동할 위치")]
    public Transform cameraViewPoint;

    [Header("기능 UI 설정")]
    [Tooltip("사용하기 버튼을 눌렀을 때 열릴 전용 UI (예: WorkbenchUIPanel)")]
    public GameObject functionUIPanel;

    [Header("업그레이드 설정")]
    public UpgradeType upgradeType = UpgradeType.None;

    [Tooltip("이 시설을 한 레벨 올리는 데 필요한 재료들. 실제 요구치는 baseAmount × currentLevel")]
    public List<MaterialRequirement> upgradeMaterials = new List<MaterialRequirement>();
    
    // 2. 레벨업 시 증가할 수치 추가 (체력 증가량 등)
    [Tooltip("레벨업 시 증가할 수치 (예: 체력 증가량)")]
    public float upgradeValuePerLevel = 10f; 

    [Header("UI 패널 (이 시설 전용)")]
    public GameObject panelRoot;
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI levelText;
    public NeedItemSlot[] needItemSlots;
    public Button upgradeButton;
    public Button useButton;
    [Tooltip("이 시설 패널의 뒤로가기/닫기 버튼. 누르면 패널 닫고 카메라 탑뷰 복귀")]
    public Button closeButton;

    private bool _subscribed;

    void Awake()
    {
        currentLevel = FacilityLevelTracker.GetLevel(facilityName, currentLevel);

        if (panelRoot != null) panelRoot.SetActive(false);
        if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
        if (useButton != null) useButton.onClick.AddListener(OnUseClicked);
        if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
    }

    void OnEnable() => TrySubscribe();
    void OnDisable() => TryUnsubscribe();

    private void TrySubscribe()
    {
        if (_subscribed || !Inventory.HasInstance) return;
        Inventory.Instance.OnInventoryChanged += UpdateUI;
        _subscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!_subscribed || !Inventory.HasInstance) return;
        Inventory.Instance.OnInventoryChanged -= UpdateUI;
        _subscribed = false;
    }

    public int GetRequiredAmount(MaterialRequirement req)
    {
        return req == null ? 0 : req.baseAmount * currentLevel;
    }

    public void Open()
    {
        TrySubscribe();
        UpdateUI();
        if (panelRoot != null) panelRoot.SetActive(true);
        if (cameraViewPoint != null && HideoutCamera.Instance != null)
            HideoutCamera.Instance.MoveToFacility(cameraViewPoint);
    }

    public void Close()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (functionUIPanel != null) functionUIPanel.SetActive(false);
    }

    public void UpdateUI()
    {
        if (nameText != null)  nameText.text  = facilityName;
        if (descText != null)  descText.text  = description;
        if (iconImage != null) iconImage.sprite = facilityIcon;
        if (levelText != null) levelText.text = $"Level: {currentLevel}";

        bool canUpgrade = upgradeMaterials != null && upgradeMaterials.Count > 0;

        if (needItemSlots != null)
        {
            for (int i = 0; i < needItemSlots.Length; i++)
            {
                var slot = needItemSlots[i];
                if (slot == null) continue;

                bool active = upgradeMaterials != null
                              && i < upgradeMaterials.Count
                              && upgradeMaterials[i] != null
                              && upgradeMaterials[i].material != null;

                if (slot.root != null) slot.root.SetActive(active);
                if (!active) continue;

                var req = upgradeMaterials[i];
                int required = GetRequiredAmount(req);
                int have = Inventory.HasInstance ? Inventory.Instance.GetTotalCount(req.material) : 0;
                string color = have >= required ? "green" : "red";

                Sprite spriteToShow = slot.iconSprite != null ? slot.iconSprite : req.material.icon;
                var slotImage = slot.root != null ? slot.root.GetComponent<Image>() : null;
                if (slotImage != null)
                {
                    slotImage.sprite = spriteToShow;
                    slotImage.enabled = spriteToShow != null;
                }

                if (slot.countText != null)
                {
                    if (slot.nameText != null) slot.nameText.text = req.material.itemName;
                    slot.countText.text = $"<color={color}>{have}/{required}</color>";
                }
                else if (slot.nameText != null)
                {
                    slot.nameText.text = $"{req.material.itemName} <color={color}>{have}/{required}</color>";
                }

                if (have < required) canUpgrade = false;
            }
        }

        if (upgradeButton != null) upgradeButton.interactable = canUpgrade;
    }

    // 3. LevelUp 함수에 체력 증가 및 특수 효과 로직 통합
    public void LevelUp()
    {
        currentLevel++;
        FacilityLevelTracker.SetLevel(facilityName, currentLevel);
        Debug.Log($"{facilityName}의 시설 레벨이 {currentLevel}로 상승했습니다!");

        if (PlayerUpgradeManager.Instance != null)
        {
            if (upgradeType == UpgradeType.Armor)
                PlayerUpgradeManager.Instance.armorLevel = currentLevel;
            else if (upgradeType == UpgradeType.Weapon)
                Debug.Log("무기 업그레이드 적용");
        }

        // 체력 증가 처리
        if (upgradeType == UpgradeType.Health)
        {
            ApplyHealthUpgrade(upgradeValuePerLevel);
        }
        // 특수 효과 처리
        else if (upgradeType == UpgradeType.SpecialEffect)
        {
            // PlayerUpgradeManager.Instance.speedMultiplier += upgradeValuePerLevel; (예시)
            Debug.Log($"특수 효과 적용! 수치: {upgradeValuePerLevel} 증가");
        }
    }

    // 4. 체력 증가 헬퍼 함수
    private void ApplyHealthUpgrade(float amount)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            NeoFPS.BasicHealthManager healthManager = player.GetComponentInChildren<NeoFPS.BasicHealthManager>();
            if (healthManager != null)
            {
                healthManager.healthMax += amount;
                healthManager.AddHealth(amount);
                Debug.Log($"플레이어 최대 체력이 {amount}만큼 증가했습니다! (현재 최대 체력: {healthManager.healthMax})");
            }
            else
            {
                Debug.LogError("플레이어에게 BasicHealthManager 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.LogError("Player 태그를 가진 게임 오브젝트를 찾을 수 없습니다.");
        }
    }

    private void OnMouseDown()
    {
        if (HideoutUIManager.Instance != null)
            HideoutUIManager.Instance.OpenFacility(this);
        else
            Debug.LogError("씬에 HideoutUIManager가 없습니다!");
    }

    private void OnUpgradeClicked()
    {
        if (!Inventory.HasInstance || upgradeMaterials == null || upgradeMaterials.Count == 0) return;

        foreach (var req in upgradeMaterials)
        {
            if (req == null || req.material == null) continue;
            if (Inventory.Instance.GetTotalCount(req.material) < GetRequiredAmount(req)) return;
        }

        foreach (var req in upgradeMaterials)
        {
            if (req == null || req.material == null) continue;
            Inventory.Instance.TryConsume(req.material, GetRequiredAmount(req));
        }

        LevelUp();
        UpdateUI();
    }

    private void OnUseClicked()
    {
        if (functionUIPanel == null) return;
        if (panelRoot != null) panelRoot.SetActive(false);
        functionUIPanel.SetActive(true);
    }

    private void OnCloseClicked()
    {
        if (HideoutUIManager.Instance != null) HideoutUIManager.Instance.CloseAll();
        else Close();
    }
}
