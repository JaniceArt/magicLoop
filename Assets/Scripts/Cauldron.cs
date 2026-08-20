using UnityEngine;
using System;
using System.Collections.Generic;

public class Cauldron : MonoBehaviour
{
    [Header("Physics")]
    public Transform absorbPoint;
    public Transform mouthPoint;
    public Transform spawnPoint;
    public float absorbRadius = 0.5f;
    public float absorbSpeed = 5.0f;

    public SpriteRenderer itemIconRenderer;
    public SpriteRenderer quantityIconRenderer;
    public GameObject bubbleBackground;
    
    [Header("Animation & VFX")]
    public Animator cauldronAnimator;
    public string shootTriggerName = "Shoot";
    [Tooltip("Hieu ung ban (effect) khi vạc nau xong")]
    public GameObject shootEffectPrefab;

    [HideInInspector] public bool isBusy = false;

    private List<GameDatabase.RecipeStep> currentSteps;
    private GameDatabase currentDatabase;
    
    private int currentStepIndex = 0;
    private int remainingQuantity = 0;

    void Start()
    {
        if (bubbleBackground != null) bubbleBackground.SetActive(false);
    }

    public void AssignRecipe(PotionType potionType, GameDatabase database)
    {
        currentDatabase = database;
        currentSteps = database.GetRecipe(potionType);
        
        currentStepIndex = 0;
        isBusy = true;

        if (currentSteps != null && currentSteps.Count > 0)
        {
            remainingQuantity = currentSteps[0].requiredQuantity;
            if (bubbleBackground != null) bubbleBackground.SetActive(true);
            UpdatePopUp();
        }
        else
        {
            isBusy = false;
        }
    }

    void Update()
    {
        if (!isBusy || currentSteps == null || currentStepIndex >= currentSteps.Count) return; 

        IngredientType neededType = currentSteps[currentStepIndex].ingredientType;
        Vector3 scanPosition = (absorbPoint != null) ? absorbPoint.position : transform.position;

        Ingredient[] allIngredients = FindObjectsByType<Ingredient>(FindObjectsSortMode.None);
        foreach (Ingredient ing in allIngredients)
        {
            if (ing.isSlotted && !ing.isBeingAbsorbed && ing.ingredientType == neededType)
            {
                float dist = Vector2.Distance(scanPosition, ing.transform.position);
                
                if (dist <= absorbRadius)
                {
                    if (ing.transform.parent != null)
                    {
                        SlotMovement slot = ing.transform.parent.GetComponent<SlotMovement>();
                        if (slot != null)
                        {
                            slot.isEmpty = true;
                        }

                        ing.transform.SetParent(null);
                    }

                    Transform targetMouth = (mouthPoint != null) ? mouthPoint : transform;
                    ing.StartAbsorb(targetMouth, absorbSpeed);
                    
                    remainingQuantity--;
                    
                    if (remainingQuantity <= 0)
                    {
                        currentStepIndex++;
                        if (currentStepIndex < currentSteps.Count)
                        {
                            remainingQuantity = currentSteps[currentStepIndex].requiredQuantity;
                        }
                    }

                    UpdatePopUp();
                    break; 
                }
            }
        }
    }

    void UpdatePopUp()
    {
        if (currentStepIndex < currentSteps.Count)
        {
            if (itemIconRenderer != null && currentDatabase != null)
            {
                IngredientType type = currentSteps[currentStepIndex].ingredientType;
                itemIconRenderer.sprite = currentDatabase.GetIngredientSprite(type);
            }
                
            if (quantityIconRenderer != null && currentDatabase != null)
            {
                quantityIconRenderer.sprite = currentDatabase.GetQuantitySprite(remainingQuantity);
            }
        }
        else
        {
            StartCoroutine(BoilAndShootRoutine());
        }
    }

    private System.Collections.IEnumerator BoilAndShootRoutine()
    {
        if (itemIconRenderer != null) itemIconRenderer.sprite = null;
        if (quantityIconRenderer != null) quantityIconRenderer.sprite = null;
        if (bubbleBackground != null) bubbleBackground.SetActive(false);
        
        currentSteps = null;

        // Bắt đầu âm thanh sôi
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCauldronBoil();
        }

        // Chờ 1.5s cho vạc "sôi" xong
        yield return new WaitForSeconds(1.5f);
        
        // Tắt âm thanh sôi
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopCauldronBoil();
        }

        // Chạy animation bắn thuốc
        if (cauldronAnimator != null && !string.IsNullOrEmpty(shootTriggerName))
        {
            cauldronAnimator.SetTrigger(shootTriggerName);
        }

        // Spawn hiệu ứng nổ
        if (shootEffectPrefab != null)
        {
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
            GameObject fx = Instantiate(shootEffectPrefab, spawnPos, Quaternion.identity);
            Destroy(fx, 3f); // Tự hủy sau 3 giây
        }

        // Chờ một chút để khớp với animation nổ bắn ra thuốc
        yield return new WaitForSeconds(0.2f);

        // Bắn thuốc ra (thông báo cho LevelManager)
        if (GameEventHandler.Instance != null)
            GameEventHandler.Instance.RecipeCompleted(this);
            
        // Đặt lại trạng thái rảnh rỗi cho vạc
        isBusy = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = (absorbPoint != null) ? absorbPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, absorbRadius);
    }

    public bool NeedsIngredient(IngredientType type)
    {
        if (!isBusy || currentSteps == null || currentStepIndex >= currentSteps.Count) return false;
        return currentSteps[currentStepIndex].ingredientType == type;
    }

    public bool NeedsAnyIngredient()
    {
        return isBusy && currentSteps != null && currentStepIndex < currentSteps.Count;
    }

    public int GetRemainingQuantity()
    {
        return remainingQuantity;
    }

    public IngredientType GetCurrentNeededType()
    {
        if (NeedsAnyIngredient())
        {
            return currentSteps[currentStepIndex].ingredientType;
        }
        return default;
    }

    public void AbsorbFromMagnet(Ingredient ing)
    {
        if (ing == null || !NeedsAnyIngredient()) return;

        if (ing.transform.parent != null)
        {
            SlotMovement slot = ing.transform.parent.GetComponent<SlotMovement>();
            if (slot != null) slot.isEmpty = true;
            ing.transform.SetParent(null);
        }

        Transform targetMouth = (mouthPoint != null) ? mouthPoint : transform;
        // Tang toc do bay khi dung nam cham
        ing.StartAbsorb(targetMouth, absorbSpeed * 2f); 
        
        remainingQuantity--;
        
        if (remainingQuantity <= 0)
        {
            currentStepIndex++;
            if (currentStepIndex < currentSteps.Count)
            {
                remainingQuantity = currentSteps[currentStepIndex].requiredQuantity;
            }
        }

        UpdatePopUp();
    }
}
