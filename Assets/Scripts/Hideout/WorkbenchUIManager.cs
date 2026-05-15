using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using NeoFPS;
using NeoFPS.ModularFirearms;

public class WorkbenchUIManager : MonoBehaviour
{
    public GameObject playerPrefab; 

    [Header("방어구 업그레이드 (파란 열쇠 소모)")]
    public TextMeshProUGUI armorInfoText;
    public TextMeshProUGUI armorCostText;
    public Button armorUpgradeBtn;

    [Header("무기 업그레이드 (빨간 열쇠 소모)")]
    public TMP_Dropdown weaponDropdown; 
    public TextMeshProUGUI weaponInfoText;
    public TextMeshProUGUI weaponCostText;
    public Button weaponUpgradeBtn;

    public Button closeButton;
    private List<ModularFirearm> availableWeapons = new List<ModularFirearm>();

    private void Awake()
    {
        if (armorUpgradeBtn) armorUpgradeBtn.onClick.AddListener(OnClickArmorUpgrade);
        if (weaponUpgradeBtn) weaponUpgradeBtn.onClick.AddListener(OnClickWeaponUpgrade);
        if (closeButton) closeButton.onClick.AddListener(CloseWorkbenchUI);
        if (weaponDropdown != null) weaponDropdown.onValueChanged.AddListener(delegate { UpdateUI(); });
    }

    private void OnEnable()
    {
        LoadWeaponsFromPrefab(); 
        Invoke("UpdateUI", 0.1f); 
    }

    public void UpdateUI()
    {
        if (PlayerUpgradeManager.Instance == null || ItemHideout.Instance == null) return;

        // 1. 방어구 업데이트 (파란 열쇠 'blueKey' 필요)
        int currentArmor = PlayerUpgradeManager.Instance.armorLevel;
        bool hasBlueKey = ItemHideout.Instance.HasKey("blueKey");
        
        armorInfoText.text = $"방어구 레벨: {currentArmor}";
        armorCostText.text = hasBlueKey ? "<color=green>필요 아이템: 파란 열쇠 (보유 중)</color>" : "<color=red>필요 아이템: 파란 열쇠 (없음)</color>";
        armorUpgradeBtn.interactable = hasBlueKey;

        // 2. 무기 업데이트 (빨간 열쇠 'redKey' 필요)
        if (availableWeapons.Count > 0 && weaponDropdown != null)
        {
            string weaponName = availableWeapons[weaponDropdown.value].name.Replace("(Clone)", "").Trim();
            int weaponLevel = PlayerUpgradeManager.Instance.GetWeaponLevel(weaponName);
            bool hasRedKey = ItemHideout.Instance.HasKey("redKey");

            weaponInfoText.text = $"{weaponName} 레벨: {weaponLevel}";
            weaponCostText.text = hasRedKey ? "<color=green>필요 아이템: 빨간 열쇠 (보유 중)</color>" : "<color=red>필요 아이템: 빨간 열쇠 (없음)</color>";
            weaponUpgradeBtn.interactable = hasRedKey;
        }
    }

    private void OnClickArmorUpgrade()
    {
        // 파란 열쇠 소모 후 레벨업
        if (ItemHideout.Instance.HasKey("blueKey"))
        {
            ItemHideout.Instance.RemoveKey("blueKey");
            PlayerUpgradeManager.Instance.armorLevel++;
            Debug.Log("파란 열쇠를 사용하여 방어구를 업그레이드했습니다!");
            UpdateUI();
        }
    }

    private void OnClickWeaponUpgrade()
    {
        if (availableWeapons.Count == 0) return;

        string weaponName = availableWeapons[weaponDropdown.value].name.Replace("(Clone)", "").Trim();
        
        // 빨간 열쇠 소모 후 레벨업
        if (ItemHideout.Instance.HasKey("redKey"))
        {
            ItemHideout.Instance.RemoveKey("redKey");
            PlayerUpgradeManager.Instance.UpgradeWeaponLevel(weaponName);
            Debug.Log($"{weaponName}을(를) 빨간 열쇠로 강화했습니다!");
            UpdateUI();
        }
    }

    // (LoadWeaponsFromPrefab 및 CloseWorkbenchUI 로직은 기존과 동일)
    private void LoadWeaponsFromPrefab() { /* 기존 코드 유지 */ }
    private void CloseWorkbenchUI() { gameObject.SetActive(false); }
}