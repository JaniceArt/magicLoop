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
            if (itemIconRenderer != null) itemIconRenderer.sprite = null;
            if (quantityIconRenderer != null) quantityIconRenderer.sprite = null;
            if (bubbleBackground != null) bubbleBackground.SetActive(false);
            
            isBusy = false;
            currentSteps = null;
            
            if (GameEventHandler.Instance != null)
                GameEventHandler.Instance.RecipeCompleted(this);
        }
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
}
