using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Ingredient : MonoBehaviour
{
    public float flySpeed = 10f;
    [HideInInspector] public IngredientType ingredientType;
    public LaneManager laneManager;
    [HideInInspector] public int laneIndex = -1;

    [HideInInspector] public bool isSlotted = false;
    [HideInInspector] public bool isBeingAbsorbed = false;

    private Collider2D col;
    private static int lastClickFrame = -1;

    void Start()
    {
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.frameCount == lastClickFrame) return;

            if (col != null && Camera.main != null)
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                if (col.OverlapPoint(mousePos))
                {
                    if (laneManager != null)
                    {
                        lastClickFrame = Time.frameCount;
                        HandleClick();
                    }
                }
            }
        }
    }

    public void HandleClick()
    {
        if (laneManager != null)
        {
            Ingredient topIng = laneManager.GetTopIngredient(this.laneIndex);
            if (topIng != null && topIng != this)
            {
                topIng.HandleClick();
                return;
            }
        }

        Debug.Log("Radar xác nhận đã click vào nấm trên cùng: " + gameObject.name);
        if (isSlotted || isBeingAbsorbed) return;

        SlotMovement[] allSlots = FindObjectsByType<SlotMovement>(FindObjectsSortMode.None);
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
            if (laneManager != null) laneManager.PopIngredient(this);
            targetSlot.isEmpty = false; 
            StartCoroutine(FlyToSlot(targetSlot));
        }
    }

    IEnumerator FlyToSlot(SlotMovement slot)
    {
        if (col != null) col.enabled = false; 

        while (Vector3.Distance(transform.position, slot.transform.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, slot.transform.position, flySpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = slot.transform.position;
        transform.SetParent(slot.transform); 
        isSlotted = true; 
    }

    public void StartAbsorb(Transform mouth, float absorbSpeed)
    {
        isBeingAbsorbed = true;
        StartCoroutine(AbsorbRoutine(mouth, absorbSpeed));
    }

    IEnumerator AbsorbRoutine(Transform mouth, float absorbSpeed)
    {
        // 1. Bay thẳng đến miệng nồi (giữ nguyên kích thước)
        while (Vector3.Distance(transform.position, mouth.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, mouth.position, absorbSpeed * Time.deltaTime);
            yield return null;
        }

        // 2. Hiệu ứng hút xoáy trôn ốc vào mặt nước
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 startScale = transform.localScale;
        float t = 0;
        
        while (t < 1f)
        {
            t += Time.deltaTime * 3f; // Thời gian xoáy tụt xuống (khoảng 0.33s)
            
            // Xoay tròn chóng mặt
            transform.Rotate(0, 0, 1080f * Time.deltaTime); 
            
            // Chìm dần xuống đáy vạc
            transform.position += Vector3.down * 1.0f * Time.deltaTime; 
            
            // Nhỏ dần về 0
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            
            // Mờ dần đi
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
