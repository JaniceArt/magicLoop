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
    [HideInInspector] public bool isFlying = false;

    [Header("Locked Lane Feature")]
    public bool hasKey = false;
    public Transform keyVisual;

    [HideInInspector] public MysteryGroup mysteryGroup = null;
    [HideInInspector] public int groupId = 0;

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
        if (mysteryGroup != null && !mysteryGroup.isRevealed)
        {
            Debug.Log("Không thể click vì nguyên liệu đang bị ẩn (Mystery)!");
            return;
        }

        if (LaneManager.Instance != null && LaneManager.Instance.IsLaneLocked(this.laneIndex))
        {
            return;
        }

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
        if (isSlotted || isBeingAbsorbed || isFlying) return;

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
            targetSlot.isEmpty = false;

            if (hasKey && keyVisual != null && LaneManager.Instance != null && LaneManager.Instance.activeLock != null)
            {
                keyVisual.SetParent(null);
                StartCoroutine(FlyKeyToLock(keyVisual, LaneManager.Instance.activeLock.transform.position));
                hasKey = false;
            }

            if (laneManager != null)
            {
                laneManager.PopIngredient(this);
            }
            
            isFlying = true;
            
            StartCoroutine(FlyToSlot(targetSlot));
        }
    }

    IEnumerator FlyKeyToLock(Transform key, Vector3 lockPos)
    {
        while (key != null && Vector3.Distance(key.position, lockPos) > 0.1f)
        {
            key.position = Vector3.MoveTowards(key.position, lockPos, (flySpeed * 0.4f) * Time.deltaTime);
            yield return null;
        }
        if (key != null)
        {
            Destroy(key.gameObject);
        }
        if (LaneManager.Instance != null)
        {
            LaneManager.Instance.OnKeyCollected();
        }
    }

    IEnumerator FlyToSlot(SlotMovement slot)
    {
        if (col != null) col.enabled = false; 


        transform.SetParent(slot.transform);
        Vector3 startLocalPos = transform.localPosition;
        float t = 0;

        while (t < 1f)
        {

            t += Time.deltaTime * 4f; 
            transform.localPosition = Vector3.Lerp(startLocalPos, Vector3.zero, t);
            yield return null;
        }

        transform.localPosition = Vector3.zero;
        isSlotted = true; 
        isFlying = false;
    }

    public void StartAbsorb(Transform mouth, float absorbSpeed)
    {
        isBeingAbsorbed = true;
        StartCoroutine(AbsorbRoutine(mouth, absorbSpeed));
    }

    IEnumerator AbsorbRoutine(Transform mouth, float absorbSpeed)
    {

        while (Vector3.Distance(transform.position, mouth.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, mouth.position, absorbSpeed * Time.deltaTime);
            yield return null;
        }


        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 startScale = transform.localScale;
        float t = 0;
        
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            

            transform.Rotate(0, 0, 1080f * Time.deltaTime); 
            

            transform.position += Vector3.down * 1.0f * Time.deltaTime; 
            

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            

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
