using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPotionRecipe", menuName = "MagicLoop/Potion Recipe")]
public class PotionRecipe : ScriptableObject
{
    [System.Serializable]
    public class RecipeStep
    {
        [Tooltip("Click chọn loại nguyên liệu từ Menu")]
        public IngredientType ingredientType;
        [Tooltip("Số lượng cần hút (VD: 3)")]
        public int requiredQuantity;
    }

    [Tooltip("Tên loại thuốc này (VD: Thuốc Trị Thương)")]
    public string potionName;

    [Tooltip("GDD setup công thức ở đây (Không cần kéo ảnh)")]
    public List<RecipeStep> steps;
}
