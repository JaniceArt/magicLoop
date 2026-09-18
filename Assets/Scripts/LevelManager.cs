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
        public Animator customerAnimator;
        public GameObject customerBubbleObject;
        public SpriteRenderer customerPotionRenderer;
        public Transform deliveryPoint;
        
        [HideInInspector] public List<LevelData.CustomerOrder> queue;
        [HideInInspector] public int currentCustomerIndex = 0;
        [HideInInspector] public bool isProcessing = false;
        [HideInInspector] public bool isWaitingForDelivery = false;
    }

    [Header("Level Settings")]
    [Tooltip("Kéo thả toàn bộ 30 LevelData vào đây. Game sẽ tự động load dựa theo tiến trình!")]
    public LevelData[] allLevels;
    public GameDatabase database;
    [Tooltip("Level hiện tại (tự động gán khi Play, không cần kéo tay nữa)")]
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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Tự động load level dựa trên tiến trình đã lưu
        int savedLevelIndex = PlayerPrefs.GetInt("CurrentLevelIndex", 0);
        if (allLevels != null && allLevels.Length > 0)
        {
            // Đảm bảo index không vượt quá số level hiện có
            if (savedLevelIndex >= allLevels.Length)
            {
                savedLevelIndex = allLevels.Length - 1; // Load level cuối nếu đã chơi hết
            }
            currentLevel = allLevels[savedLevelIndex];
        }

        GameObject bgObj = GameObject.Find("BG (1)");
        if (bgObj != null && bgObj.GetComponent<BackgroundScaler>() == null)
        {
            bgObj.AddComponent<BackgroundScaler>();
        }
        GameObject bgAlt = GameObject.Find("BG");
        if (bgAlt != null && bgAlt.GetComponent<BackgroundScaler>() == null)
        {
            bgAlt.AddComponent<BackgroundScaler>();
        }
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
            
            if (lane.customerAnimator != null && database != null)
            {
                RuntimeAnimatorController anim = database.GetCustomerAnimator(customer.customerName);
                lane.customerAnimator.runtimeAnimatorController = anim;
            }
            
            if (lane.customerAnimator != null && database != null)
            {
                RuntimeAnimatorController anim = database.GetCustomerAnimator(customer.customerName);
                Debug.Log($"GetCustomerAnimator for {customer.customerName}: {(anim != null ? anim.name : "NULL")}");
                if (anim != null)
                {
                    lane.customerAnimator.enabled = false;
                    lane.customerAnimator.gameObject.SetActive(true);
                    lane.customerAnimator.runtimeAnimatorController = anim;
                    lane.customerAnimator.enabled = true;
                    lane.customerAnimator.Rebind();
                    lane.customerAnimator.Update(0f);
                    Debug.Log($"Animator controller set to: {lane.customerAnimator.runtimeAnimatorController?.name}");
                }
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
            if (lane.customerAnimator != null) lane.customerAnimator.gameObject.SetActive(false);
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
        
        // Chay animation nhay tung len cua vac
        CauldronWobble wobble = cauldron.GetComponent<CauldronWobble>();
        if (wobble != null)
        {
            wobble.PlayWobble();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPotionShoot();
        }

        lane.isWaitingForDelivery = true;
        SpawnPotion(lane);
    }

    void SpawnPotion(LaneState lane)
    {
        Debug.Log("SpawnPotion called for lane " + lane.cauldron.gameObject.name);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPotionSpawn();
        
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
        if (lane.customerAnimator != null) lane.customerAnimator.gameObject.SetActive(false);
        if (lane.customerBubbleObject != null) lane.customerBubbleObject.SetActive(false);
        if (lane.customerPotionRenderer != null) lane.customerPotionRenderer.sprite = null;


        if (magicEffectPrefab != null && lane.customerAnimator != null)
        {

            Vector3 fxPos = lane.customerRenderer.transform.position;
            fxPos.y += 0.5f; // Nang cao len mot chut
            fxPos.z = -5f;
            
            GameObject vfx = Instantiate(magicEffectPrefab, fxPos, Quaternion.identity);
            vfx.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // To ra 1.5 lan
            
            // Ep buoc tat ca cac sub-particle system phai tat Loop (phong hieu ung con nam trong folder con)
            ParticleSystem[] pss = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach(var ps in pss)
            {
                var main = ps.main;
                main.loop = false;
            }

            Debug.Log("💥 Đã spawn hiệu ứng ma thuật tại: " + fxPos);
            // Giam thoi gian ton tai xuong 1.5s de no bien mat nhanh hon (neu hieu ung ngan)
            Destroy(vfx, 1.5f);
        }


        // Doi 0.2s truoc khi khach tiep theo hien ra
        yield return new WaitForSeconds(0.2f);


        lane.isWaitingForDelivery = false;
        lane.currentCustomerIndex++;

        // Chi hien thi lai nhan vat neu con khach hang tiep theo trong hang doi
        if (lane.queue != null && lane.currentCustomerIndex < lane.queue.Count)
        {
            if (lane.customerRenderer != null) lane.customerRenderer.enabled = true;
            LoadNextCustomer(lane);
        }
        else
        {
            // Neu het khach thi dam bao tat luon hinh anh
            if (lane.customerRenderer != null) lane.customerRenderer.enabled = false;
            if (lane.customerAnimator != null) lane.customerAnimator.gameObject.SetActive(false);
        }

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
                
                // Lưu tiến trình level tiếp theo
                int currentIndex = PlayerPrefs.GetInt("CurrentLevelIndex", 0);
                PlayerPrefs.SetInt("CurrentLevelIndex", currentIndex + 1);
                PlayerPrefs.Save();

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

