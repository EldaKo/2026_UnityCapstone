using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingScript : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject InventoryPanel;

    [SerializeField] private Button goMainBtn;

    [Header("Audio")]
    public AudioSource bgmSource;
    public Slider volumeSlider;

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (InventoryPanel !=  null) InventoryPanel.SetActive(false);

        if (goMainBtn != null)
        {
            goMainBtn.onClick.AddListener(quitGame);
        }

        if (volumeSlider != null && bgmSource != null)
        {
            float saved = PlayerPrefs.GetFloat("BGMVolume", 1f);
            volumeSlider.value = saved;
            bgmSource.volume = saved;

            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            ToggleInven();
        }
    }

    public void ToggleSettings()
    {
        bool isActive = !settingsPanel.activeSelf;
        settingsPanel.SetActive(isActive);

        if (isActive)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ToggleInven()
    {
        bool isActive = !InventoryPanel.activeSelf;
        InventoryPanel.SetActive(isActive);

        if (isActive)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void SetVolume(float value)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = value;
            PlayerPrefs.SetFloat("BGMVolume", value);
        }
    }

    public void quitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("mainScreen");
    }
}