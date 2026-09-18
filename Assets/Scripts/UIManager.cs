using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Coin UI")]
    public TextMeshProUGUI[] coinTexts;

    [Header("Heart UI")]
    public TextMeshProUGUI[] heartTexts;

    [Header("Popups")]
    public GameObject winPopup;
    public GameObject losePopup;
    public TextMeshProUGUI winCoinText;
    public TextMeshProUGUI loseCoinText;
    
    // Panel đen như kiểu setting
    public GameObject overlayPanel; 

    [Header("Flow UI")]
    public GameObject homePanel;
    public GameObject loadingPanel;
    public GameObject gamePanel;
    public UnityEngine.UI.Slider loadingProgressBar;

    private int currentCoins;
    private int currentHearts;
    private const string COIN_KEY = "PlayerCoins";
    private const string HEART_KEY = "PlayerHearts";
    private const int MAX_HEARTS = 5;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!PlayerPrefs.HasKey(COIN_KEY))
        {
            PlayerPrefs.SetInt(COIN_KEY, 0);
        }
        currentCoins = PlayerPrefs.GetInt(COIN_KEY);
        if (currentCoins < 0)
        {
            currentCoins = 0;
            PlayerPrefs.SetInt(COIN_KEY, 0);
        }
        UpdateCoinUI();

        if (!PlayerPrefs.HasKey(HEART_KEY))
        {
            PlayerPrefs.SetInt(HEART_KEY, 5);
        }
        currentHearts = PlayerPrefs.GetInt(HEART_KEY);
        UpdateHeartUI();

        if (winPopup != null) winPopup.SetActive(false);
        if (losePopup != null) losePopup.SetActive(false);
        if (overlayPanel != null) overlayPanel.SetActive(false);

        if (PlayerPrefs.GetInt("SkipHome", 0) == 1)
        {
            // Reset cờ và bay thẳng vào game
            PlayerPrefs.SetInt("SkipHome", 0);
            PlayGame();
        }
        else
        {
            if (homePanel != null) homePanel.SetActive(true);
            if (loadingPanel != null) loadingPanel.SetActive(false);
            if (gamePanel != null) gamePanel.SetActive(false);
        }
    }

    public void PlayGame()
    {
        if (homePanel != null) homePanel.SetActive(false);
        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(true);
        if (loadingProgressBar != null) loadingProgressBar.value = 0f;

        // Bắt đầu tạo Level ngay lập tức (chạy ngầm phía sau màn hình Loading)
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartLevel();
        }

        float duration = 1.5f;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            if (loadingProgressBar != null)
            {
                loadingProgressBar.value = timer / duration;
            }
            yield return null;
        }

        // Tạo xong thì ẩn Loading đi để hiện ra GamePanel và Level đã tạo sẵn
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    public void UpdateCoinUI()
    {
        if (coinTexts != null)
        {
            foreach (var txt in coinTexts)
            {
                if (txt != null) txt.text = currentCoins.ToString();
            }
        }
        
        if (BoosterManager.Instance != null)
        {
            BoosterManager.Instance.UpdateBoosterButtons(currentCoins);
        }
    }

    public int GetCoins()
    {
        return currentCoins;
    }

    public void UpdateHeartUI()
    {
        if (heartTexts != null)
        {
            foreach (var txt in heartTexts)
            {
                if (txt != null) txt.text = currentHearts.ToString();
            }
        }
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        if (currentCoins < 0) currentCoins = 0;
        PlayerPrefs.SetInt(COIN_KEY, currentCoins);
        PlayerPrefs.Save();
        UpdateCoinUI();
    }

    public int GetCurrentCoins()
    {
        return currentCoins;
    }

    public bool SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            PlayerPrefs.SetInt(COIN_KEY, currentCoins);
            PlayerPrefs.Save();
            UpdateCoinUI();
            return true;
        }
        return false;
    }

    public void ShowWinPopup(int reward)
    {
        if (overlayPanel != null) overlayPanel.SetActive(true);
        if (winPopup != null)
        {
            winPopup.SetActive(true);
            if (winCoinText != null) winCoinText.text = "+" + reward.ToString();
        }
        
        AddCoins(reward);
        Time.timeScale = 0f;
    }

    public void ShowLosePopup()
    {
        if (overlayPanel != null) overlayPanel.SetActive(true);
        if (losePopup != null)
        {
            losePopup.SetActive(true);
            // Xóa đoạn hiển thị loseCoinText vì giờ trừ mạng, không trừ coin
        }
        
        // Trừ 1 mạng
        currentHearts--;
        if (currentHearts < 0) currentHearts = 0;
        PlayerPrefs.SetInt(HEART_KEY, currentHearts);
        PlayerPrefs.Save();
        UpdateHeartUI();
        
        // Cập nhật text trong lose popup nếu có
        if (loseCoinText != null) loseCoinText.text = "-1"; 
        
        Time.timeScale = 0f;
    }

    // Nút chức năng
    public void RetryLevel()
    {
        Time.timeScale = 1f;
        
        // Cắm cờ để lúc load lại Scene nó bay thẳng vào Game luôn
        PlayerPrefs.SetInt("SkipHome", 1);
        PlayerPrefs.Save();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        // Giả sử scene menu là index 0
        SceneManager.LoadScene(0);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        
        // Cắm cờ để lúc load lại Scene nó bay thẳng vào Game luôn
        PlayerPrefs.SetInt("SkipHome", 1);
        PlayerPrefs.Save();
        
        // Load lại chính Scene hiện tại (LevelManager sẽ tự động bốc Level tiếp theo vì đã +1 ở vòng trước)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
