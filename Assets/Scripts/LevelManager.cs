using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class LaneState
    {
        public Cauldron cauldron;
        public SpriteRenderer customerRenderer;
        public GameObject customerBubbleObject; // Toogle bubble visibility
        public SpriteRenderer customerPotionRenderer; // Popup sprite
        
        [HideInInspector] public List<LevelData.CustomerOrder> queue;
        [HideInInspector] public int currentCustomerIndex = 0;
        [HideInInspector] public bool isProcessing = false;
        [HideInInspector] public bool isWaitingForDelivery = false;
    }

    public GameDatabase database;
    public LevelData currentLevel;
    public GameObject potionPrefab;
    public GameObject magicEffectPrefab; // Hiệu ứng lúc khách biến mất
    public LevelProgressBar progressBar; // Thêm biến chứa thanh tiến độ

    public LaneState pinkLane = new LaneState();
    public LaneState greenLane = new LaneState();

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f); // Đợi các script khác (như Cauldron) Start xong
        
        if (currentLevel != null)
        {
            // Tự động đếm tổng số nguyên liệu cần thiết cho màn chơi này để gán cho thanh Tiến độ
            if (progressBar != null)
            {
                int totalIngredients = 0;
                foreach (var lane in currentLevel.ingredientLanes) 
                {
                    foreach (var group in lane.groups)
                    {
                        totalIngredients += group.quantity;
                    }
                }
                
                progressBar.SetProgress(0, totalIngredients > 0 ? totalIngredients : 32);
            }

            pinkLane.queue = new List<LevelData.CustomerOrder>(currentLevel.pinkQueue);
            greenLane.queue = new List<LevelData.CustomerOrder>(currentLevel.greenQueue);
            
            LoadNextCustomer(pinkLane);
            LoadNextCustomer(greenLane);
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
            lane.isProcessing = false; // Đã hết khách, tắt máy nghỉ ngơi
            if (lane.customerRenderer != null) lane.customerRenderer.sprite = null;
            if (lane.customerBubbleObject != null) lane.customerBubbleObject.SetActive(false);
            if (lane.customerPotionRenderer != null) lane.customerPotionRenderer.sprite = null;
        }
    }

    void Update()
    {
        if (currentLevel == null) return;
        ProcessLane(pinkLane);
        ProcessLane(greenLane);
    }

    void ProcessLane(LaneState lane)
    {
        if (lane == null || !lane.isProcessing || lane.cauldron == null) return;

        if (!lane.cauldron.isBusy && !lane.isWaitingForDelivery)
        {
            Debug.Log("ProcessLane: Cauldron is not busy and lane is not waiting for delivery. Spawning potion for lane " + lane.cauldron.gameObject.name);
            lane.isWaitingForDelivery = true;
            SpawnPotion(lane);
        }
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
        
        // Spawn at Cauldron's designated spawn point or mouth
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
        potionItem.levelManager = this;
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
        // 1. Tạm ẩn khách cũ và tắt bong bóng
        if (lane.customerRenderer != null) lane.customerRenderer.enabled = false;
        if (lane.customerBubbleObject != null) lane.customerBubbleObject.SetActive(false);

        // 2. Bắn hiệu ứng bùm chéo
        if (magicEffectPrefab != null && lane.customerRenderer != null)
        {
            // Ép Z = -5 để đảm bảo hiệu ứng luôn nổi lên trên cùng, không bị phông nền che mất
            Vector3 fxPos = lane.customerRenderer.transform.position;
            fxPos.z = -5f;
            
            GameObject vfx = Instantiate(magicEffectPrefab, fxPos, Quaternion.identity);
            Debug.Log("💥 Đã spawn hiệu ứng ma thuật tại: " + fxPos);
            Destroy(vfx, 3f); // Tự động xóa rác sau 3 giây (tránh giật lag)
        }

        // 3. Cứ cho nổ, và lập tức chuyển sang khách tiếp theo luôn (không bắt người chơi đợi)
        yield return new WaitForSeconds(0.1f); // Dừng đúng 1 nhịp siêu ngắn để cảm nhận độ giật

        // 4. Chuyển chỉ mục sang khách tiếp theo
        lane.isWaitingForDelivery = false;
        lane.currentCustomerIndex++;

        // 5. Khôi phục trạng thái và nạp khách mới (nếu hết khách thì LoadNextCustomer sẽ tự tắt isProcessing)
        if (lane.customerRenderer != null) lane.customerRenderer.enabled = true;
        LoadNextCustomer(lane);
    }
}
