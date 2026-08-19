using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameDatabase", menuName = "MagicLoop/Game Database")]
public class GameDatabase : ScriptableObject
{
    [System.Serializable]
    public class IngredientData
    {
        public IngredientType ingredientType;
        public Sprite ingredientSprite;
        [Range(0.1f, 3.0f)]
        public float scaleMultiplier = 1.0f;
    }



    [System.Serializable]
    public class RecipeStep
    {
        public IngredientType ingredientType;
        public int requiredQuantity;
    }

    [System.Serializable]
    public class PotionRecipeData
    {
        public PotionType potionType;
        public Sprite potionSprite;
        public List<RecipeStep> steps;
    }

    public List<IngredientData> ingredientDatabase = new List<IngredientData>();
    
    [Header("Customer Sprites")]
    public Sprite witchSprite;
    public Sprite elfSprite;
    public Sprite goblinSprite;
    public Sprite demonSprite;
    public Sprite dwarfSprite;
    public Sprite mysteryMaskSprite;

    public Sprite[] numberSprites = new Sprite[10];
    public List<PotionRecipeData> allRecipes = new List<PotionRecipeData>();

    public Sprite GetCustomerSprite(CustomerType type)
    {
        switch (type)
        {
            case CustomerType.PhuThuy: return witchSprite;
            case CustomerType.Elf: return elfSprite;
            case CustomerType.Goblin: return goblinSprite;
            case CustomerType.Demon: return demonSprite;
            case CustomerType.Dwarf: return dwarfSprite;
            default: return witchSprite;
        }
    }

    public Sprite GetIngredientSprite(IngredientType type)
    {
        foreach (var data in ingredientDatabase)
        {
            if (data.ingredientType == type) return data.ingredientSprite;
        }
        return null;
    }

    public float GetIngredientScale(IngredientType type)
    {
        foreach (var data in ingredientDatabase)
        {
            if (data.ingredientType == type) 
            {
                return data.scaleMultiplier <= 0.01f ? 1.0f : data.scaleMultiplier;
            }
        }
        return 1.0f;
    }

    public Sprite GetQuantitySprite(int amount)
    {
        if (numberSprites == null || numberSprites.Length == 0) return null;
        
        int safeIndex = Mathf.Clamp(amount, 0, numberSprites.Length - 1);
        return numberSprites[safeIndex];
    }

    [Header("Locked Lane Feature")]
    public Sprite keySprite;
    public GameObject lockedLanePrefab;
    public Sprite[] keyNumberSprites = new Sprite[10];

    public Sprite GetPotionSprite(PotionType type)
    {
        foreach (var data in allRecipes)
        {
            if (data.potionType == type) return data.potionSprite;
        }
        return null;
    }

    public List<RecipeStep> GetRecipe(PotionType type)
    {
        foreach (var data in allRecipes)
        {
            if (data.potionType == type) return data.steps;
        }
        return null;
    }
}
