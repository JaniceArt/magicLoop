using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BoosterManager : MonoBehaviour
{
    public static BoosterManager Instance;

    [Header("UI References")]
    public Button shuffleButton;
    public Button magnetButton;

    [Header("Costs")]
    public int shuffleCost = 50;
    public int magnetCost = 100;

    [Header("Debug")]
    [Tooltip("Tick vao day de test game, dung booster khong ton tien")]
    public bool freeBoosterMode = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Cap nhat lan dau
        if (UIManager.Instance != null)
        {
            UpdateBoosterButtons(UIManager.Instance.GetCoins());
        }
    }

    public void UpdateBoosterButtons(int currentCoins)
    {
        if (shuffleButton != null) shuffleButton.interactable = freeBoosterMode || (currentCoins >= shuffleCost);
        if (magnetButton != null) magnetButton.interactable = freeBoosterMode || (currentCoins >= magnetCost);
    }

    public void UseShuffleBooster()
    {
        if (!freeBoosterMode && UIManager.Instance != null && UIManager.Instance.GetCoins() < shuffleCost) return;

        if (LaneManager.Instance != null)
        {
            if (!freeBoosterMode) UIManager.Instance.AddCoins(-shuffleCost);
            LaneManager.Instance.ShuffleLanes();
        }
    }

    public void UseMagnetBooster()
    {
        if (!freeBoosterMode && UIManager.Instance != null && UIManager.Instance.GetCoins() < magnetCost) return;

        if (LaneManager.Instance != null)
        {
            Cauldron[] cauldrons = FindObjectsByType<Cauldron>(FindObjectsSortMode.None);
            
            bool foundAndSucked = false;

            foreach (Cauldron c in cauldrons)
            {
                if (c.isBusy && c.NeedsAnyIngredient())
                {
                    IngredientType neededType = c.GetCurrentNeededType();
                    int maxCount = c.GetRemainingQuantity();
                    
                    if (maxCount > 0)
                    {
                        List<Ingredient> ingredientsToSuck = LaneManager.Instance.FindIngredientsForMagnet(neededType, maxCount);
                        if (ingredientsToSuck.Count > 0)
                        {
                            foreach (Ingredient ing in ingredientsToSuck)
                            {
                                c.AbsorbFromMagnet(ing);
                            }
                            foundAndSucked = true;
                        }
                    }
                }
            }
            
            if (!foundAndSucked)
            {
                Debug.Log("Nam cham: Khong tim thay nguyen lieu phu hop!");
            }
            else
            {
                if (!freeBoosterMode) UIManager.Instance.AddCoins(-magnetCost);
            }
        }
    }
}
