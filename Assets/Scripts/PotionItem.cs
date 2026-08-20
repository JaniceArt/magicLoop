using UnityEngine;
using System;
using System.Collections;

public class PotionItem : MonoBehaviour
{
    public LevelManager.LaneState targetLane;
    
    public float flySpeed = 5f;
    public float jumpHeightToBelt = 1.0f;
    public float jumpHeightToCustomer = 1.0f;

    [HideInInspector] public bool isDelivered = false;
    public SlotMovement currentSlot;
    [HideInInspector] public bool isOnBelt = false;
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
        transform.localScale = Vector3.zero;


        float duration = 0.8f; 
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            Vector3 currentTarget = slot.transform.position;
            Vector3 linearPos = Vector3.Lerp(startPos, currentTarget, t);
            

            linearPos.y += Mathf.Sin(t * Mathf.PI) * jumpHeightToBelt; 
            
            transform.position = linearPos;


            transform.localScale = Vector3.Lerp(Vector3.zero, startScale, Mathf.Clamp01(t * 2f));
            

            transform.Rotate(0, 0, 360f * Time.deltaTime);

            yield return null;
        }

        transform.position = slot.transform.position;
        transform.localScale = startScale;
        transform.rotation = Quaternion.identity;
        transform.SetParent(slot.transform); 
        isOnBelt = true;
        Debug.Log($"[{gameObject.name}] Finished jumping! Now on belt at {transform.position}");
    }

    void Update()
    {
        if (isDelivered || targetLane == null || targetLane.customerAnimator == null) return;

        if (currentSlot == null)
        {
            FindAndFlyToSlot();
            return;
        }

        if (isOnBelt)
        {
            timeOnBelt += Time.deltaTime;
            
            bool shouldDeliver = false;

            if (targetLane.deliveryPoint != null)
            {
                float dist = Vector2.Distance(transform.position, targetLane.deliveryPoint.position);
                if (dist < 0.5f)
                {
                    shouldDeliver = true;
                }
            }
            else
            {
                if (timeOnBelt > 1.5f) 
                {
                    shouldDeliver = true;
                }
            }

            if (shouldDeliver)
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
        Vector3 targetPos = targetLane.customerAnimator.transform.position;
        Vector3 startScale = transform.localScale;
        
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / 4.0f;
        float t = 0;
        
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            
            Vector3 linearPos = Vector3.Lerp(startPos, targetPos, t);

            linearPos.y += Mathf.Sin(t * Mathf.PI) * jumpHeightToCustomer; 
            
            transform.position = linearPos;
            transform.Rotate(0, 0, 360f * Time.deltaTime);

            
            yield return null;
        }
        
        if (GameEventHandler.Instance != null)
            GameEventHandler.Instance.PotionDelivered(targetLane);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPotionDeliver();
        }
        
        Destroy(gameObject);
    }
}
