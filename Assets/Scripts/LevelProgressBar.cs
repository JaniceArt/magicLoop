using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelProgressBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Kéo ThanhXanhLa vào đây (Sẽ thay đổi độ rộng của nó)")]
    public RectTransform fillRect; 
    
    [Tooltip("Kéo chữ ChuTienDo (TextMeshPro) vào đây")]
    public TextMeshProUGUI progressText;

    [Header("Settings")]
    public int maxValue = 30;
    public int currentValue = 11;
    
    [Tooltip("Chiều dài tối đa của thanh xanh khi đầy kịch kim (Ví dụ: 500)")]
    public float maxWidth = 400f;

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
        // Thay đổi chiều dài thanh xanh lá (giữ nguyên độ cao)
        if (fillRect != null && maxValue > 0)
        {
            float percent = Mathf.Clamp01((float)currentValue / maxValue);
            fillRect.sizeDelta = new Vector2(maxWidth * percent, fillRect.sizeDelta.y);
        }
        
        // Cập nhật chữ hiển thị
        if (progressText != null)
        {
            progressText.text = currentValue.ToString() + "/" + maxValue.ToString();
        }
    }
}
