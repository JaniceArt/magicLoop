using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelProgressBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Kéo ảnh ThanhXanhLa (đã chỉnh Image Type thành Filled) vào đây")]
    public Image fillImage; 
    
    [Tooltip("Kéo chữ ChuTienDo (TextMeshPro) vào đây")]
    public TextMeshProUGUI progressText;

    [Header("Settings")]
    public int maxValue = 30;
    public int currentValue = 11;

    // Chạy thử 1 lần lúc mới vào game
    void Start()
    {
        UpdateUI();
    }

    // Các script khác (ví dụ lúc giao thuốc thành công) có thể gọi hàm này để tự cập nhật thanh
    public void SetProgress(int current, int max)
    {
        currentValue = current;
        maxValue = max;
        UpdateUI();
    }

    // Gọi hàm này để cộng thêm điểm tiến độ
    public void AddProgress(int amount = 1)
    {
        currentValue += amount;
        if (currentValue > maxValue) currentValue = maxValue;
        UpdateUI();
    }

    // Hàm này tính toán thanh dài ngắn và hiển thị số
    public void UpdateUI()
    {
        // Kéo độ dài thanh xanh lá
        if (fillImage != null && maxValue > 0)
        {
            fillImage.fillAmount = (float)currentValue / maxValue;
        }
        
        // Cập nhật chữ hiển thị
        if (progressText != null)
        {
            progressText.text = currentValue.ToString() + "/" + maxValue.ToString();
        }
    }
}
