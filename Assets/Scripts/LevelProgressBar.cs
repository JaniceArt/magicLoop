using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelProgressBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Kéo cục SlotSlider vào đây!")]
    public Slider progressSlider; 
    
    [Tooltip("Kéo chữ hiển thị số vào đây")]
    public TextMeshProUGUI progressText;

    private SlotMovement[] allSlots;

    void Start()
    {

        allSlots = FindObjectsByType<SlotMovement>(FindObjectsSortMode.None);
    }

    void Update()
    {

        if (allSlots == null || allSlots.Length == 0) return;

        int maxValue = allSlots.Length;
        int currentValue = 0;

        foreach (var slot in allSlots)
        {
            if (!slot.isEmpty)
            {
                currentValue++;
            }
        }


        if (progressSlider != null)
        {
            float percent = Mathf.Clamp01((float)currentValue / maxValue);
            progressSlider.value = percent;
        }
        
        if (progressText != null)
        {
            progressText.text = currentValue.ToString() + "/" + maxValue.ToString();
        }

        if (currentValue == maxValue && maxValue > 0)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.CheckStuckCondition();
            }
        }
    }
}
