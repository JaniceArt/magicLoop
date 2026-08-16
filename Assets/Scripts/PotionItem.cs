using UnityEngine;
using System.Collections;

public class PotionItem : MonoBehaviour
{
    public LevelManager levelManager;
    public LevelManager.LaneState targetLane;
    
    public float flySpeed = 5f;
    public float jumpHeightToBelt = 1.0f;
    public float jumpHeightToCustomer = 1.0f;

    private bool isDelivered = false;
    private SlotMovement currentSlot;
    private bool isOnBelt = false;
    private float timeOnBelt = 0f;

    void Start()
    {
        Debug.Log($"[{gameObject.name}] Start() called at position {transform.position}, scale {transform.localScale}");
        FindAndFlyToSlot();
    }

    void FindAndFlyToSlot()
    {
        SlotMovement[] allSlots = FindObjectsByType<SlotMovement>(FindObjectsSortMode.None);
        Debug.Log($"[{gameObject.name}] Found {allSlots.Length} slots in the scene.");
        SlotMovement targetSlot = null;
        float minDistance = float.MaxValue;

        foreach (SlotMovement slot in allSlots)
        {
            if (slot.isEmpty)
            {
                float dist = Vector2.Distance(transform.position, slot.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    targetSlot = slot;
                }
            }
        }

        if (targetSlot != null)
        {
            Debug.Log($"[{gameObject.name}] Found empty slot at {targetSlot.transform.position}. Starting jump!");
            targetSlot.isEmpty = false; 
            currentSlot = targetSlot;
            StartCoroutine(FlyToSlot(targetSlot));
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] NO EMPTY SLOT FOUND! Potion is stuck at {transform.position}");
        }
    }

    IEnumerator FlyToSlot(SlotMovement slot)
    {
        Debug.Log($"[{gameObject.name}] FlyToSlot Coroutine Started!");
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        transform.localScale = Vector3.zero; // Bắt đầu từ 0 để tạo cảm giác chui từ trong nồi ra

        // Luôn bay với thời gian cố định là 0.8 giây để dù khoảng cách rất ngắn thì cú nhảy vẫn chậm và mượt
        float duration = 0.8f; 
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            Vector3 currentTarget = slot.transform.position;
            Vector3 linearPos = Vector3.Lerp(startPos, currentTarget, t);
            
            // Nhảy vòng cung với độ cao tùy chỉnh
            linearPos.y += Mathf.Sin(t * Mathf.PI) * jumpHeightToBelt; 
            
            transform.position = linearPos;

            // Phóng to dần khi nhảy ra khỏi vạc (đạt max size khi lên đỉnh)
            transform.localScale = Vector3.Lerp(Vector3.zero, startScale, Mathf.Clamp01(t * 2f));
            
            // Xoay vài vòng cho đẹp
            transform.Rotate(0, 0, 360f * Time.deltaTime);

            yield return null;
        }

        transform.position = slot.transform.position;
        transform.localScale = startScale;
        transform.rotation = Quaternion.identity; // Reset góc xoay
        transform.SetParent(slot.transform); 
        isOnBelt = true;
        Debug.Log($"[{gameObject.name}] Finished jumping! Now on belt at {transform.position}");
    }

    void Update()
    {
        if (isDelivered || targetLane == null || targetLane.customerRenderer == null) return;

        if (currentSlot == null)
        {
            FindAndFlyToSlot();
            return;
        }

        if (isOnBelt)
        {
            timeOnBelt += Time.deltaTime;
            
            // Bay đến khách hàng sau khi nằm trên băng chuyền được 1.5 giây (bỏ giới hạn khoảng cách để linh hoạt hơn)
            if (timeOnBelt > 1.5f) 
            {
                isDelivered = true;
                isOnBelt = false;
                if (currentSlot != null)
                {
                    currentSlot.isEmpty = true; 
                    transform.SetParent(null);
                }
                StartCoroutine(FlyToCustomer());
            }
        }
    }

    IEnumerator FlyToCustomer()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = targetLane.customerRenderer.transform.position;
        Vector3 startScale = transform.localScale;
        
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / 4.0f; // Slower speed: 4 units per sec
        float t = 0;
        
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            
            Vector3 linearPos = Vector3.Lerp(startPos, targetPos, t);
            // Add slight arc (jump up)
            linearPos.y += Mathf.Sin(t * Mathf.PI) * jumpHeightToCustomer; 
            
            transform.position = linearPos;
            transform.Rotate(0, 0, 360f * Time.deltaTime);
            // Không thu nhỏ kích thước khi lao vào khách hàng nữa
            
            yield return null;
        }
        
        if (levelManager != null)
        {
            levelManager.OnPotionDelivered(targetLane);
        }
        
        Destroy(gameObject);
    }
}
