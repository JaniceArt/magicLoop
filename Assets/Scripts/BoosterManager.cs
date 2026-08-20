using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoosterManager : MonoBehaviour
{
    public static BoosterManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void UseShuffleBooster()
    {
        if (LaneManager.Instance != null)
        {
            LaneManager.Instance.ShuffleLanes();
        }
    }

    public void UseMagnetBooster()
    {
        if (LaneManager.Instance != null)
        {
            Cauldron[] cauldrons = FindObjectsByType<Cauldron>(FindObjectsSortMode.None);
            
            bool foundAndSucked = false;

            foreach (Cauldron c in cauldrons)
            {
                if (c.isBusy && c.NeedsAnyIngredient())
                {
                    IngredientType neededType = c.GetCurrentNeededType();
                    int maxNeeded = c.GetRemainingQuantity();
                    
                    List<Ingredient> ingredientsToSuck = LaneManager.Instance.FindIngredientsForMagnet(neededType, maxNeeded);
                    if (ingredientsToSuck != null && ingredientsToSuck.Count > 0)
                    {
                        foreach (Ingredient ing in ingredientsToSuck)
                        {
                            c.AbsorbFromMagnet(ing);
                        }
                        foundAndSucked = true;
                        // Khong break nua de no xet tiep cac noi khac
                    }
                }
            }
            
            if (!foundAndSucked)
            {
                Debug.Log("Nam cham: Khong tim thay nguyen lieu phu hop!");
            }
        }
    }
}
