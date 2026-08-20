using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Kéo object ch?a toàn b? b?ng Settings vào dây")]
    public GameObject settingsPanel;
    
    [Header("Toggle Buttons (Image components)")]
    public Image musicToggleImage;
    public Image soundToggleImage;
    public Image vibrationToggleImage;

    [Header("Toggle Sprites")]
    [Tooltip("Kéo ?nh nút B?T (Màu xanh) vào dây")]
    public Sprite onSprite;
    [Tooltip("Kéo ?nh nút T?T (Màu nâu) vào dây")]
    public Sprite offSprite;


    public static SettingsManager Instance { get; private set; }

    private bool isMusicOn = true;
    private bool isSoundOn = true;
    private bool isVibOn = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }


        isMusicOn = PlayerPrefs.GetInt("Setting_Music", 1) == 1;
        isSoundOn = PlayerPrefs.GetInt("Setting_Sound", 1) == 1;
        isVibOn = PlayerPrefs.GetInt("Setting_Vibration", 1) == 1;


        UpdateToggleUI(musicToggleImage, isMusicOn);
        UpdateToggleUI(soundToggleImage, isSoundOn);
        UpdateToggleUI(vibrationToggleImage, isVibOn);
        
        ApplySettings();
    }





    [Header("Buttons to Hide in Home")]
    public GameObject homeButtonObj;
    public GameObject retryButtonObj;

    public void OpenSettingsFromHome()
    {
        if (homeButtonObj != null) homeButtonObj.SetActive(false);
        if (retryButtonObj != null) retryButtonObj.SetActive(false);
        OpenSettings();
    }

    public void OpenSettingsFromGame()
    {
        if (homeButtonObj != null) homeButtonObj.SetActive(true);
        if (retryButtonObj != null) retryButtonObj.SetActive(true);
        OpenSettings();
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }





    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("Setting_Music", isMusicOn ? 1 : 0);
        UpdateToggleUI(musicToggleImage, isMusicOn);
        ApplySettings();
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("Setting_Sound", isSoundOn ? 1 : 0);
        UpdateToggleUI(soundToggleImage, isSoundOn);
        ApplySettings();
    }

    public void ToggleVibration()
    {
        isVibOn = !isVibOn;
        PlayerPrefs.SetInt("Setting_Vibration", isVibOn ? 1 : 0);
        UpdateToggleUI(vibrationToggleImage, isVibOn);

        if (isVibOn)
        {
            TriggerVibration();
        }
    }

    public void TriggerVibration()
    {
        if (isVibOn)
        {
            #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
            #endif
        }
    }


    private void UpdateToggleUI(Image toggleImage, bool isOn)
    {
        if (toggleImage != null)
        {
            toggleImage.sprite = isOn ? onSprite : offSprite;
        }
    }


    private void ApplySettings()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusic(isMusicOn);
            AudioManager.Instance.ToggleSound(isSoundOn);
        }
    }





    public void RetryLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoHome()
    {
        Time.timeScale = 1f;


        SceneManager.LoadScene("SampleScene"); 
    }
}


