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


    [Header("Volume Sliders (Tùy chọn)")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private bool isMusicOn = true;
    private bool isSoundOn = true;
    private bool isVibOn = true;

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
        
        // Khởi tạo slider
        float musicVol = PlayerPrefs.GetFloat("Setting_MusicVol", 1f);
        float sfxVol = PlayerPrefs.GetFloat("Setting_SFXVol", 1f);
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVol;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVol;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        ApplySettings();
    }





    [Header("Buttons to Hide in Home")]
    public GameObject homeButtonObj;
    public GameObject retryButtonObj;

    public void OpenSettingsFromHome()
    {
        Debug.Log("==== ĐÃ BẤM NÚT SETTING Ở HOME! Đang gọi mở bảng... ====");
        if (homeButtonObj != null) homeButtonObj.SetActive(false);
        if (retryButtonObj != null) retryButtonObj.SetActive(false);
        OpenSettings();
    }

    public void OpenSettingsFromGame()
    {
        Debug.Log("==== ĐÃ BẤM NÚT SETTING Ở GAME! Đang gọi mở bảng... ====");
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
            Debug.Log("==== CODE XỬ LÝ: Đã bật SettingPanel (SetActive = true) và Dừng thời gian (TimeScale = 0)! Nếu sếp không nhìn thấy bảng thì 1000% là do nó bị một Panel khác đè lên mặt (Lỗi Hierarchy)! ====");
        }
        else
        {
            Debug.Log("==== LỖI: Ô Settings Panel trong Inspector đang bị trống (None), không biết mở cái gì! ====");
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
        // Áp dụng toggle
        if (AudioManager.Instance != null)
        {
            if (AudioManager.Instance.bgmSource != null)
                AudioManager.Instance.bgmSource.mute = !isMusicOn;
            
            if (AudioManager.Instance.sfxSource != null)
                AudioManager.Instance.sfxSource.mute = !isSoundOn;
        }
    }

    public void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("Setting_MusicVol", value);
        if (AudioManager.Instance != null && AudioManager.Instance.bgmSource != null)
        {
            AudioManager.Instance.bgmSource.volume = value;
        }
    }

    public void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("Setting_SFXVol", value);
        if (AudioManager.Instance != null && AudioManager.Instance.sfxSource != null)
        {
            AudioManager.Instance.sfxSource.volume = value;
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


