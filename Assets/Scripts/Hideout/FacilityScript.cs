using UnityEngine;

// [변경됨] Revolver -> Weapon 으로 범용적으로 사용할 수 있게 수정
public enum UpgradeType
{
    None,       // 단순 기능만 있는 시설 (작업대 등)
    Armor,      // 방탄복 단독 업그레이드 시설
    Weapon      // 무기 단독 업그레이드 시설
}

public class FacilityScript : MonoBehaviour
{
    [Header("시설 정보")]
    public string facilityName = "작업대";
    [TextArea]
    public string description = "무기와 방어구를 개조하고 강화할 수 있습니다.";
    public Sprite facilityIcon;

    [Header("현재 상태")]
    public int currentLevel = 1;
    public float currentBuffValue = 0f; 

    [Header("카메라 연출")]
    [Tooltip("시설 클릭 시 카메라가 이동할 위치")]
    public Transform cameraViewPoint; 

    [Header("기능 UI 설정")]
    [Tooltip("사용하기 버튼을 눌렀을 때 열릴 전용 UI (예: WorkbenchUIPanel)")]
    public GameObject functionUIPanel; 

    [Header("업그레이드 설정")]
    [Tooltip("시설 자체의 레벨을 올릴 때 영향을 줄 스탯 (작업대는 보통 None)")]
    public UpgradeType upgradeType = UpgradeType.None;

    // 시설 자체 레벨업 비용 계산
    public int GetUpgradeCost()
    {
        return currentLevel * 50; 
    }

    // 시설 자체 레벨업 함수
    public void LevelUp()
    {
        currentLevel++;
        Debug.Log($"{facilityName}의 시설 레벨이 {currentLevel}로 상승했습니다!");

        if (PlayerUpgradeManager.Instance != null)
        {
            if (upgradeType == UpgradeType.Armor)
            {
                PlayerUpgradeManager.Instance.armorLevel = currentLevel;
            }
            // [변경됨] Revolver -> Weapon
            else if (upgradeType == UpgradeType.Weapon)
            {
                Debug.Log("무기 업그레이드 적용 (단일 무기 시설일 경우 사용)");
            }
        }
    }

    // 시설 클릭 시 호출
    private void OnMouseDown()
    {
        if (HideoutUIManager.Instance != null)
        {
            HideoutUIManager.Instance.OpenFacilityUI(this);
        }
        else
        {
            Debug.LogError("씬에 HideoutUIManager가 없습니다!");
        }
    }
}