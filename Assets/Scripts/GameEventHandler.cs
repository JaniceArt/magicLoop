using System;
using UnityEngine;

public class GameEventHandler : MonoBehaviour
{
    public static GameEventHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public event Action<Cauldron> OnRecipeCompleted;
    public void RecipeCompleted(Cauldron cauldron)
    {
        OnRecipeCompleted?.Invoke(cauldron);
    }

    public event Action<LevelManager.LaneState> OnPotionDelivered;
    public void PotionDelivered(LevelManager.LaneState lane)
    {
        OnPotionDelivered?.Invoke(lane);
    }
}
