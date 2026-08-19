using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Coin UI")]
    public TextMeshProUGUI coinText;

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
    private const string COIN_KEY = "PlayerCoins";

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
        // Khởi tạo vốn 500 nếu chưa có
        if (!PlayerPrefs.HasKey(COIN_KEY))
        {
            PlayerPrefs.SetInt(COIN_KEY, 500);
        }
        currentCoins = PlayerPrefs.GetInt(COIN_KEY);
        UpdateCoinUI();

        if (winPopup != null) winPopup.SetActive(false);
        if (losePopup != null) losePopup.SetActive(false);
        if (overlayPanel != null) overlayPanel.SetActive(false);

        if (homePanel != null) homePanel.SetActive(true);
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
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
        if (coinText != null)
        {
            coinText.text = currentCoins.ToString();
        }
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        PlayerPrefs.SetInt(COIN_KEY, currentCoins);
        PlayerPrefs.Save();
        UpdateCoinUI();
    }

    public void ShowWinPopup(int reward)
    {
        if (overlayPanel != null) overlayPanel.SetActive(true);
        if (winPopup != null)
        {
            winPopup.SetActive(true);
            if (winCoinText != null) winCoinText.text = reward.ToString();
        }
        
        AddCoins(reward);
        Time.timeScale = 0f;
    }

    public void ShowLosePopup(int penalty)
    {
        if (overlayPanel != null) overlayPanel.SetActive(true);
        if (losePopup != null)
        {
            losePopup.SetActive(true);
            if (loseCoinText != null) loseCoinText.text = penalty.ToString();
        }
        
        AddCoins(-penalty);
        Time.timeScale = 0f;
    }

    // Nút chức năng
    public void RetryLevel()
    {
        Time.timeScale = 1f;
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
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels in build settings!");
            GoHome();
        }
    }
}
