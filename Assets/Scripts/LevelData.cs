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
        public bool isMystery = false;
        public bool hasKey = false;
        public int keyIndex = 0;
    }

    [System.Serializable]
    public class IngredientLaneData
    {
        public bool isLocked = false;
        public int requiredKeys = 1;
        public List<IngredientGroup> groups;
    }

    [Header("Level Settings")]
    public int levelNumber = 1;
    public LayoutType selectedLayout = LayoutType.TwoLanes;
    
    [Header("Rewards & Penalties")]
    public int winCoinReward = 200;
    public int loseCoinPenalty = 120;

    public List<CustomerOrder> pinkQueue;
    public List<CustomerOrder> greenQueue;
    public List<IngredientLaneData> ingredientLanes;
}
