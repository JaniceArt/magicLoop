using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Level", menuName = "Magic Loop/Level Data")]
public class LevelData : ScriptableObject
{
    [System.Serializable]
    public class CustomerOrder
    {
        public CustomerType customerName;
        public PotionType potion;
    }

    public enum LayoutType
    {
        TwoLanes,
        ThreeLanes,
        FourLanes
    }

    [System.Serializable]
    public class IngredientGroup
    {
        public IngredientType ingredientType;
        public int quantity = 1;
    }

    [System.Serializable]
    public class IngredientLaneData
    {
        public List<IngredientGroup> groups;
    }

    public LayoutType selectedLayout = LayoutType.TwoLanes;

    public List<CustomerOrder> pinkQueue;
    public List<CustomerOrder> greenQueue;
    public List<IngredientLaneData> ingredientLanes;
}
