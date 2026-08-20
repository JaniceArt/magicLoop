using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance;

    [Header("Hint Settings")]
    public float firstHintIdleThreshold = 2f; // Lần đầu tiên xuất hiện sau 2s
    public float defaultIdleThreshold = 7f;   // Các lần sau xuất hiện sau 7s
    
    private bool hasShownFirstHint = false;
    
    [Header("UI / Visuals")]
    public GameObject hintWandPrefab; // Hình ảnh gậy phép hoặc bàn tay (GameObject 2D)
    public Vector3 offset = new Vector3(0.3f, 0.2f, 0); // Vị trí gậy lệch đi so với nguyên liệu
    public float floatSpeed = 4f;
    public float floatAmplitude = 0.05f; // Chỉnh nhỏ lại cho gậy nhấp nhô nhẹ nhàng hơn

    private GameObject currentHintWand;
    private float idleTimer = 0f;
    private Ingredient targetIngredient;
    private Vector3 baseWandPos;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        // 1. Theo dõi thao tác người chơi
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ResetIdleTimer();
        }

        // 2. Nếu đã có gậy đang hiển thị
        if (currentHintWand != null && currentHintWand.activeSelf)
        {
            // Kiểm tra xem target còn hợp lệ không (có bị người chơi click hút mất không)
            if (targetIngredient == null || targetIngredient.laneManager == null)
            {
                HideHint();
                return;
            }

            // Animation nảy nảy cho gậy
            float newY = baseWandPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            currentHintWand.transform.position = new Vector3(baseWandPos.x, newY, baseWandPos.z);
            return;
        }

        // 3. Đếm thời gian idle
        idleTimer += Time.deltaTime;
        
        float currentThreshold = hasShownFirstHint ? defaultIdleThreshold : firstHintIdleThreshold;

        if (idleTimer >= currentThreshold)
        {
            hasShownFirstHint = true; // Đánh dấu là đã qua lần đầu
            ShowHint(currentThreshold);
        }
    }

    private void ShowHint(float currentThreshold)
    {
        if (LaneManager.Instance == null) return;
        
        // Tìm các vạc đang hoạt động
        Cauldron[] cauldrons = FindObjectsByType<Cauldron>(FindObjectsSortMode.None);
        Ingredient bestTarget = null;

        foreach (Cauldron c in cauldrons)
        {
            if (c.isBusy && c.NeedsAnyIngredient())
            {
                IngredientType neededType = c.GetCurrentNeededType();
                Ingredient found = LaneManager.Instance.GetHintIngredient(neededType);
                if (found != null)
                {
                    bestTarget = found;
                    break;
                }
            }
        }

        if (bestTarget != null)
        {
            // Xóa viền sáng ở mục tiêu cũ nếu có
            if (targetIngredient != null && targetIngredient != bestTarget)
            {
                BlinkingGlow oldGlow = targetIngredient.GetComponent<BlinkingGlow>();
                if (oldGlow != null) Destroy(oldGlow);
            }

            targetIngredient = bestTarget;
            
            // Gắn viền sáng nhấp nháy vào mục tiêu mới
            if (targetIngredient.GetComponent<BlinkingGlow>() == null)
            {
                targetIngredient.gameObject.AddComponent<BlinkingGlow>();
            }

            if (currentHintWand == null)
            {
                if (hintWandPrefab != null)
                {
                    currentHintWand = Instantiate(hintWandPrefab);
                }
                else
                {
                    // Chờ sếp gắn ảnh vào
                    return;
                }
            }
            
            currentHintWand.SetActive(true);
            baseWandPos = targetIngredient.transform.position + offset;
            currentHintWand.transform.position = baseWandPos;
            
            // Set Sorting Order cao nhất để nó nổi lên trên
            SpriteRenderer wandSr = currentHintWand.GetComponent<SpriteRenderer>();
            if (wandSr != null) wandSr.sortingOrder = 999;
        }
        else
        {
            // Reset timer một chút để không check liên tục mỗi frame gây lag
            idleTimer = currentThreshold - 1f; 
        }
    }

    public void HideHint()
    {
        if (currentHintWand != null)
        {
            currentHintWand.SetActive(false);
        }
        
        // Gỡ bỏ viền sáng nhấp nháy
        if (targetIngredient != null)
        {
            BlinkingGlow glow = targetIngredient.GetComponent<BlinkingGlow>();
            if (glow != null) Destroy(glow);
        }

        targetIngredient = null;
    }

    public void ResetIdleTimer()
    {
        idleTimer = 0f;
        HideHint();
    }
}
