using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [System.Serializable]
    public class LaneState
    {
        public Cauldron cauldron;
        public SpriteRenderer customerRenderer;
        public GameObject customerBubbleObject;
        public SpriteRenderer customerPotionRenderer;
        public Transform deliveryPoint;
        
        [HideInInspector] public List<LevelData.CustomerOrder> queue;
        [HideInInspector] public int currentCustomerIndex = 0;
        [HideInInspector] public bool isProcessing = false;
        [HideInInspector] public bool isWaitingForDelivery = false;
    }

    [Header("Level Settings")]
    public GameDatabase database;
    public LevelData currentLevel;
    public TextMeshProUGUI levelText;

    public GameObject potionPrefab;
    public GameObject magicEffectPrefab;
    public LevelProgressBar progressBar;

    public LaneState pinkLane = new LaneState();
    public LaneState greenLane = new LaneState();

    public bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Don't auto-start if UIManager is active and will handle the flow.
        // But if UIManager is missing or disabled (e.g. testing), auto-start immediately!
        if (UIManager.Instance == null)
        {
            StartLevel();
        }
    }

    public void StartLevel()
    {
        StartCoroutine(StartLevelRoutine());
    }

    IEnumerator StartLevelRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        
        if (currentLevel != null)
        {

            if (levelText != null)
            {
                levelText.text = currentLevel.levelNumber.ToString();
            }

            pinkLane.queue = new List<LevelData.CustomerOrder>(currentLevel.pinkQueue);
            greenLane.queue = new List<LevelData.CustomerOrder>(currentLevel.greenQueue);
            
            LoadNextCustomer(pinkLane);
            LoadNextCustomer(greenLane);

            if (LaneManager.Instance != null)
            {
                LaneManager.Instance.InitializeLanesNow();
            }

            if (GameEventHandler.Instance != null)
            {
                GameEventHandler.Instance.OnRecipeCompleted += HandleRecipeCompleted;
                GameEventHandler.Instance.OnPotionDelivered += OnPotionDelivered;
            }
        }
    }

    void OnDestroy()
    {

        if (GameEventHandler.Instance != null)
        {
            GameEventHandler.Instance.OnRecipeCompleted -= HandleRecipeCompleted;
            GameEventHandler.Instance.OnPotionDelivered -= OnPotionDelivered;
        }
    }

    void LoadNextCustomer(LaneState lane)
    {
        Debug.Log($"LoadNextCustomer called for lane {lane.cauldron?.gameObject.name}. Index: {lane.currentCustomerIndex}, Queue Size: {lane.queue?.Count}");
        
        if (lane.queue != null && lane.currentCustomerIndex < lane.queue.Count)
        {
            var customer = lane.queue[lane.currentCustomerIndex];
            Debug.Log($"Loading customer: {customer.customerName} who ordered {customer.potion}");
            
            if (lane.customerRenderer != null && database != null)
            {
                Sprite s = database.GetCustomerSprite(customer.customerName);
                lane.customerRenderer.sprite = s;
                Debug.Log($"Assigned sprite for {customer.customerName}: {(s != null ? s.name : "NULL!")}");
            }
            
            if (lane.customerBubbleObject != null) lane.customerBubbleObject.SetActive(true);
            
            if (lane.customerPotionRenderer != null && database != null)
            {
                lane.customerPotionRenderer.sprite = database.GetPotionSprite(customer.potion);
            }
            
            lane.isProcessing = true;
            if (lane.cauldron != null) lane.cauldron.AssignRecipe(customer.potion, database);
        }
        else
        {
            lane.isProcessing = false;
            if (lane.customerRenderer != null) lane.customerRenderer.sprite = null;
            if (lane.customerBubbleObject != null) lane.customerBubbleObject.SetActive(false);
            if (lane.customerPotionRenderer != null) lane.customerPotionRenderer.sprite = null;
        }
    }

    private void HandleRecipeCompleted(Cauldron cauldron)
    {
        LaneState lane = null;
        if (pinkLane.cauldron == cauldron) lane = pinkLane;
        else if (greenLane.cauldron == cauldron) lane = greenLane;

        if (lane == null || !lane.isProcessing || lane.isWaitingForDelivery) return;

        Debug.Log("Vạc đã nấu xong! LevelManager nhận event và chuẩn bị spawn thuốc cho lane " + cauldron.gameObject.name);
        lane.isWaitingForDelivery = true;
        SpawnPotion(lane);
    }

    void SpawnPotion(LaneState lane)
    {
        Debug.Log("SpawnPotion called for lane " + lane.cauldron.gameObject.name);
        var customer = lane.queue[lane.currentCustomerIndex];
        Sprite potionSprite = database.GetPotionSprite(customer.potion);

        GameObject potionGO = null;
        if (potionPrefab != null)
        {
            potionGO = Instantiate(potionPrefab);
            Debug.Log("Spawned potion clone: " + potionGO.name);
        }
        else
        {
            potionGO = new GameObject("PotionItem");
            Debug.Log("Created empty PotionItem GameObject");
        }
        

        Vector3 spawnPos = lane.cauldron.transform.position;
        if (lane.cauldron.spawnPoint != null)
        {
            spawnPos = lane.cauldron.spawnPoint.position;
        }
        else if (lane.cauldron.absorbPoint != null)
        {
            spawnPos = lane.cauldron.absorbPoint.position;
        }
        potionGO.transform.position = spawnPos;

        SpriteRenderer sr = potionGO.GetComponent<SpriteRenderer>();
        if (sr == null) sr = potionGO.AddComponent<SpriteRenderer>();
        sr.sprite = potionSprite;
        sr.sortingOrder = 20;

        PotionItem potionItem = potionGO.GetComponent<PotionItem>();
        if (potionItem == null) potionItem = potionGO.AddComponent<PotionItem>();
        potionItem.targetLane = lane;
        
        potionGO.SetActive(true);
    }

    public void OnPotionDelivered(LaneState lane)
    {
        if (lane != null)
        {
            StartCoroutine(CustomerTransitionRoutine(lane));
        }
    }

    IEnumerator CustomerTransitionRoutine(LaneState lane)
    {

        if (lane.customerRenderer != null) lane.customerRenderer.enabled = false;
        if (lane.customerBubbleObject != null) lane.customerBubbleObject.SetActive(false);


        if (magicEffectPrefab != null && lane.customerRenderer != null)
        {

            Vector3 fxPos = lane.customerRenderer.transform.position;
            fxPos.z = -5f;
            
            GameObject vfx = Instantiate(magicEffectPrefab, fxPos, Quaternion.identity);
            Debug.Log("💥 Đã spawn hiệu ứng ma thuật tại: " + fxPos);
            Destroy(vfx, 3f);
        }


        yield return new WaitForSeconds(0.1f);


        lane.isWaitingForDelivery = false;
        lane.currentCustomerIndex++;


        if (lane.customerRenderer != null) lane.customerRenderer.enabled = true;
        LoadNextCustomer(lane);
        CheckWinCondition();
    }

    public void CheckWinCondition()
    {
        if (isGameOver) return;

        bool pinkDone = pinkLane.queue == null || pinkLane.currentCustomerIndex >= pinkLane.queue.Count;
        bool greenDone = greenLane.queue == null || greenLane.currentCustomerIndex >= greenLane.queue.Count;

        if (pinkDone && greenDone)
        {
              isGameOver = true;
              if (UIManager.Instance != null && currentLevel != null)
              {
                  int reward = 10;
                  if (currentLevel.difficulty == LevelData.Difficulty.Medium) reward = 20;
                  else if (currentLevel.difficulty == LevelData.Difficulty.Hard) reward = 30;
                  UIManager.Instance.ShowWinPopup(reward);
              }
          }
    }

    public void CheckStuckCondition()
    {
        if (isGameOver) return;

        bool canFreeSlot = false;

        // 1. Kiểm tra xem có lọ thuốc nào đã chiếm slot trên băng chuyền chưa.
        // Nếu lọ thuốc đã có slot (dù đang bay tới hay đang nằm trên đó),
        // nó sẽ cưỡi băng chuyền rồi giao cho khách, sau đó giải phóng slot đó.
        PotionItem[] allPotions = FindObjectsByType<PotionItem>(FindObjectsSortMode.None);
        foreach (PotionItem potion in allPotions)
        {
            if (potion.currentSlot != null && !potion.isDelivered)
            {
                canFreeSlot = true;
                break;
            }
        }

        // 2. Nếu không có lọ thuốc nào sắp giải phóng chỗ, kiểm tra xem Vạc có hút được món nào trên băng chuyền không.
        if (!canFreeSlot)
        {
            Ingredient[] allIngredients = FindObjectsByType<Ingredient>(FindObjectsSortMode.None);
            foreach (Ingredient ing in allIngredients)
            {
                if ((ing.isSlotted || ing.isFlying) && !ing.isBeingAbsorbed)
                {
                    bool pinkNeeds = pinkLane.cauldron != null && pinkLane.cauldron.NeedsIngredient(ing.ingredientType);
                    bool greenNeeds = greenLane.cauldron != null && greenLane.cauldron.NeedsIngredient(ing.ingredientType);

                    if (pinkNeeds || greenNeeds)
                    {
                        canFreeSlot = true;
                        break;
                    }
                }
            }
        }

        // Nếu hoàn toàn KHÔNG CÓ CÁCH NÀO giải phóng chỗ trống, thì mới báo Thua.
        if (!canFreeSlot)
        {
            LoseGame();
        }
    }

    public void LoseGame()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        StartCoroutine(LoseGameRoutine());
    }

    private IEnumerator LoseGameRoutine()
    {
          yield return new WaitForSeconds(3f);
          if (UIManager.Instance != null && currentLevel != null)
          {
              UIManager.Instance.ShowLosePopup();
          }
      }
}
