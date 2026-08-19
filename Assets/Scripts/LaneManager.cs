using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MysteryMaskTracker : MonoBehaviour
{
    public MysteryGroup mGroup;
    public float padding = 0.2f;
    public float maskWidth = 0.74f;
    public float offsetY = 0f;
    
    void LateUpdate()
    {
        if (mGroup == null || mGroup.ingredients.Count == 0) return;
        
        float mWidth = LaneManager.Instance != null ? LaneManager.Instance.maskWidth : 0.74f;

        float maxY = float.MinValue;
        float minY = float.MaxValue;
        float xPos = 0;
        float zPos = 0;
        int activeCount = 0;

        foreach (var ing in mGroup.ingredients)
        {
            if (ing != null && !ing.isFlying && !ing.isBeingAbsorbed && !ing.isSlotted)
            {
                SpriteRenderer ingSr = ing.GetComponent<SpriteRenderer>();
                if (ingSr != null)
                {
                    float top = ingSr.bounds.max.y;
                    float bottom = ingSr.bounds.min.y;
                    if (top > maxY) maxY = top;
                    if (bottom < minY) minY = bottom;
                }
                
                if (activeCount == 0)
                {
                    xPos = ing.transform.position.x;
                    zPos = ing.transform.position.z;
                }
                activeCount++;
            }
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (activeCount == 0 || maxY == float.MinValue)
        {
            if (sr != null) sr.enabled = false;
            return;
        }
        else
        {
            if (sr != null) sr.enabled = true;
        }

        float centerY = (maxY + minY) / 2f;
        transform.position = new Vector3(xPos, centerY, zPos);
        
        if (sr != null)
        {
            Vector3 lossy = transform.lossyScale;
            float worldWidth = mWidth;
            // Tự động bao trọn từ đỉnh cao nhất đến đáy thấp nhất của các nguyên liệu, cộng thêm xíu viền
            float worldHeight = (maxY - minY) + 0.05f;
            
            sr.size = new Vector2(
                lossy.x != 0 ? worldWidth / lossy.x : worldWidth, 
                lossy.y != 0 ? worldHeight / lossy.y : worldHeight
            );

            sr.sortingOrder = 70;
        }
    }
}

public class MysteryGroup
{
    public List<Ingredient> ingredients = new List<Ingredient>();
    public GameObject maskObject;
    public bool isRevealed = false;

    public void Reveal()
    {
        if (isRevealed) return;
        isRevealed = true;
        if (maskObject != null)
        {
            GameObject.Destroy(maskObject);
        }
    }
}

public class LaneManager : MonoBehaviour
{
    [System.Serializable]
    public class LaneLayoutConfig
    {
        public GameObject layoutImage;
        public Transform[] startPoints;
    }

    public LaneLayoutConfig layout2Lanes;
    public LaneLayoutConfig layout3Lanes;
    public LaneLayoutConfig layout4Lanes;

    [Header("Lane Setup")]
    public float laneSpacing = 1.5f;
    public float itemSpacing = 0.8f;
    public float groupSpacing = 1.5f;
    public float ingredientScale = 0.8f; 
    
    [Header("Ingredient Settings")]
    public GameObject ingredientPrefab;
    public int maxSortingOrder = 40;

    [Header("Mystery Mask Visuals")]
    public float maskPadding = 1.2f;
    public float maskWidth = 0.74f;
    public float maskOffsetY = 0.5f;

    private List<List<Ingredient>> activeLanes = new List<List<Ingredient>>();
    private LevelManager levelManager;
    private Dictionary<Ingredient, Coroutine> moveCoroutines = new Dictionary<Ingredient, Coroutine>();

    public static LaneManager Instance;

    [Header("Locked Lane Feature")]
    public int lockedLaneIndex = -1;
    public int totalKeysRequired = 0;
    public int currentKeysCollected = 0;
    public LockVisual activeLock;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        levelManager = FindObjectOfType<LevelManager>();
    }

    public void InitializeLanesNow()
    {
        StartCoroutine(InitializeLanes());
    }

    IEnumerator InitializeLanes()
    {
        yield return new WaitForSeconds(0.2f); 

        if (levelManager != null && levelManager.currentLevel != null && levelManager.database != null)
        {
            int laneCount = 0;
            LaneLayoutConfig activeLayout = null;
            var layoutEnum = levelManager.currentLevel.selectedLayout;

            if (layout2Lanes.layoutImage != null) layout2Lanes.layoutImage.SetActive(false);
            if (layout3Lanes.layoutImage != null) layout3Lanes.layoutImage.SetActive(false);
            if (layout4Lanes.layoutImage != null) layout4Lanes.layoutImage.SetActive(false);

            if (layoutEnum == LevelData.LayoutType.TwoLanes) 
            {
                activeLayout = layout2Lanes;
                laneCount = 2;
            }
            else if (layoutEnum == LevelData.LayoutType.ThreeLanes) 
            {
                activeLayout = layout3Lanes;
                laneCount = 3;
            }
            else if (layoutEnum == LevelData.LayoutType.FourLanes) 
            {
                activeLayout = layout4Lanes;
                laneCount = 4;
            }

            if (activeLayout != null)
            {
                if (activeLayout.layoutImage != null) activeLayout.layoutImage.SetActive(true);

                foreach (Transform startPoint in activeLayout.startPoints)
                {
                    if (startPoint != null && startPoint.GetComponent<SpriteRenderer>() != null)
                    {
                        startPoint.gameObject.SetActive(false);
                    }
                }

                int globalGroupId = 0;
                for (int i = 0; i < laneCount; i++)
                {
                    List<Ingredient> laneIngredients = new List<Ingredient>();
                    activeLanes.Add(laneIngredients);

                    if (i < activeLayout.startPoints.Length)
                    {
                        Transform startPoint = activeLayout.startPoints[i];

                        if (i < levelManager.currentLevel.ingredientLanes.Count)
                        {
                            var laneData = levelManager.currentLevel.ingredientLanes[i];

                            if (laneData.isLocked)
                            {
                                lockedLaneIndex = i;
                                totalKeysRequired = laneData.requiredKeys;
                            }

                            foreach (var group in laneData.groups)
                            {
                                globalGroupId++;
                                MysteryGroup mGroup = null;
                                if (group.isMystery && group.quantity > 0 && levelManager.database.mysteryMaskSprite != null)
                                {
                                    mGroup = new MysteryGroup();
                                }

                                for (int count = 0; count < group.quantity; count++)
                                {
                                    bool isKeyHolder = group.hasKey && count == group.keyIndex;
                                    SpawnIngredient(group.ingredientType, startPoint, i, laneIngredients, isKeyHolder, mGroup, globalGroupId);
                                }

                                if (mGroup != null && mGroup.ingredients.Count > 0)
                                {
                                    GameObject maskObj = new GameObject("MysteryMask");
                                    mGroup.maskObject = maskObj;
                                    
                                    // Bỏ làm con của LaneManager để tránh dính tỷ lệ scale khổng lồ gây hỏng 9-slice
                                    maskObj.transform.SetParent(null);

                                    SpriteRenderer maskSr = maskObj.AddComponent<SpriteRenderer>();
                                    maskSr.sprite = levelManager.database.mysteryMaskSprite;
                                    maskSr.drawMode = SpriteDrawMode.Sliced;
                                    
                                    MysteryMaskTracker tracker = maskObj.AddComponent<MysteryMaskTracker>();
                                    tracker.mGroup = mGroup;
                                }
                            }
                        }
                        UpdatePositionsAndSorting(i, startPoint);
                    }
                }

                if (lockedLaneIndex != -1 && totalKeysRequired > 0 && levelManager.database.lockedLanePrefab != null)
                {
                    Transform lockedStart = activeLayout.startPoints[lockedLaneIndex];
                    GameObject lockObj = Instantiate(levelManager.database.lockedLanePrefab, lockedStart.position + Vector3.down * 1.5f, Quaternion.identity, transform);
                    activeLock = lockObj.GetComponent<LockVisual>();
                    if (activeLock != null)
                    {
                        activeLock.UpdateNumber(totalKeysRequired, levelManager.database);
                    }
                }
            }
        }
    }

    void SpawnIngredient(IngredientType type, Transform startPoint, int laneIndex, List<Ingredient> laneIngredients, bool hasKey, MysteryGroup mGroup = null, int groupId = 0)
    {
        if (ingredientPrefab == null || startPoint == null) return;
        
        GameObject go = Instantiate(ingredientPrefab, startPoint.position, Quaternion.identity);
        float scaleMult = levelManager.database != null ? levelManager.database.GetIngredientScale(type) : 1.0f;
        go.transform.localScale = startPoint.localScale * ingredientScale * scaleMult;
        Ingredient ing = go.GetComponent<Ingredient>();
        ing.ingredientType = type;
        ing.laneManager = this; 
        ing.laneIndex = laneIndex;
        ing.hasKey = hasKey;
        ing.mysteryGroup = mGroup;
        ing.groupId = groupId;

        if (mGroup != null)
        {
            mGroup.ingredients.Add(ing);
        }
        
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = levelManager.database.GetIngredientSprite(type);
        }

        if (go.GetComponent<BoxCollider2D>() == null && go.GetComponent<CircleCollider2D>() == null)
        {
            go.AddComponent<BoxCollider2D>();
        }

        if (hasKey && levelManager.database.keySprite != null)
        {
            GameObject keyObj = new GameObject("KeyVisual");
            keyObj.transform.SetParent(go.transform);
            keyObj.transform.localPosition = Vector3.zero; 
            keyObj.transform.localScale = Vector3.one * 2f;
            SpriteRenderer keySr = keyObj.AddComponent<SpriteRenderer>();
            keySr.sprite = levelManager.database.keySprite;
            keySr.sortingOrder = sr != null ? sr.sortingOrder + 1 : 50; 
            ing.keyVisual = keyObj.transform;
        }

        laneIngredients.Add(ing);
    }

    public void UpdatePositionsAndSorting(int laneIndex, Transform startPoint = null)
    {
        if (laneIndex < 0 || laneIndex >= activeLanes.Count) return;

        List<Ingredient> laneIngredients = activeLanes[laneIndex];

        if (startPoint == null)
        {
            var layoutEnum = levelManager.currentLevel.selectedLayout;
            LaneLayoutConfig activeLayout = null;
            if (layoutEnum == LevelData.LayoutType.TwoLanes) activeLayout = layout2Lanes;
            else if (layoutEnum == LevelData.LayoutType.ThreeLanes) activeLayout = layout3Lanes;
            else if (layoutEnum == LevelData.LayoutType.FourLanes) activeLayout = layout4Lanes;

            if (activeLayout != null && laneIndex < activeLayout.startPoints.Length)
            {
                startPoint = activeLayout.startPoints[laneIndex];
            }
        }

        if (startPoint == null) return;

        laneIngredients.RemoveAll(item => item == null);

        float currentY = 0f;
        for (int i = 0; i < laneIngredients.Count; i++)
        {
            Ingredient ing = laneIngredients[i];
            if (ing == null) continue;

            if (i > 0)
            {
                Ingredient prevIng = laneIngredients[i - 1];
                if (ing.groupId == prevIng.groupId)
                {
                    currentY -= itemSpacing;
                }
                else
                {
                    currentY -= groupSpacing;
                }
            }
            
            Vector3 targetPos = startPoint.position + new Vector3(0, currentY, 0);
            
            if (moveCoroutines.ContainsKey(ing) && moveCoroutines[ing] != null)
            {
                StopCoroutine(moveCoroutines[ing]);
            }
            moveCoroutines[ing] = StartCoroutine(SmoothMove(ing.transform, targetPos));

            SpriteRenderer sr = ing.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = maxSortingOrder - i;
            }

            if (ing.mysteryGroup != null)
            {
                if (i == 0 && !ing.mysteryGroup.isRevealed)
                {
                    ing.mysteryGroup.Reveal();
                }
                else if (ing.mysteryGroup.ingredients.Count > 0 && ing.mysteryGroup.ingredients[0] == ing && !ing.mysteryGroup.isRevealed)
                {
                    if (ing.mysteryGroup.maskObject != null)
                    {
                        SpriteRenderer maskSr = ing.mysteryGroup.maskObject.GetComponent<SpriteRenderer>();
                        if (maskSr != null)
                        {
                            maskSr.sortingOrder = 60;
                        }
                    }
                }
            }
        }
    }

    IEnumerator SmoothMove(Transform t, Vector3 target)
    {
        Ingredient ing = t != null ? t.GetComponent<Ingredient>() : null;
        while (t != null && ing != null && ing.laneManager == this && Vector3.Distance(t.position, target) > 0.05f)
        {
            t.position = Vector3.MoveTowards(t.position, target, 5f * Time.deltaTime);
            yield return null;
        }
        if (t != null && ing != null && ing.laneManager == this) t.position = target;
    }

    public bool IsTopIngredient(Ingredient ing)
    {
        if (ing.laneIndex >= 0 && ing.laneIndex < activeLanes.Count)
        {
            List<Ingredient> laneIngredients = activeLanes[ing.laneIndex];
            if (laneIngredients.Count > 0)
            {
                return laneIngredients[0] == ing;
            }
        }
        return false;
    }

    public Ingredient GetTopIngredient(int laneIndex)
    {
        if (laneIndex >= 0 && laneIndex < activeLanes.Count)
        {
            List<Ingredient> laneIngredients = activeLanes[laneIndex];
            if (laneIngredients.Count > 0)
            {
                return laneIngredients[0];
            }
        }
        return null;
    }

    public void PopIngredient(Ingredient ing)
    {
        if (ing.laneIndex >= 0 && ing.laneIndex < activeLanes.Count)
        {
            List<Ingredient> laneIngredients = activeLanes[ing.laneIndex];
            if (laneIngredients.Contains(ing))
            {
                laneIngredients.Remove(ing);
                int savedLaneIndex = ing.laneIndex;
                ing.laneManager = null;
                UpdatePositionsAndSorting(savedLaneIndex); 
            }
        }
    }

    public bool IsLaneLocked(int laneIndex)
    {
        return lockedLaneIndex == laneIndex && currentKeysCollected < totalKeysRequired;
    }

    public void OnKeyCollected()
    {
        currentKeysCollected++;
        if (activeLock != null)
        {
            int remaining = totalKeysRequired - currentKeysCollected;
            activeLock.UpdateNumber(remaining, levelManager.database);
            
            if (remaining <= 0)
            {
                activeLock.Unlock();
            }
        }
    }

    public Ingredient FindIngredientForMagnet(IngredientType type)
    {
        for (int i = 0; i < activeLanes.Count; i++)
        {
            if (IsLaneLocked(i)) continue;

            List<Ingredient> laneIngredients = activeLanes[i];
            for (int j = 0; j < laneIngredients.Count; j++)
            {
                Ingredient ing = laneIngredients[j];
                if (ing.ingredientType == type && !ing.hasKey && ing.mysteryGroup == null)
                {
                    PopIngredient(ing);
                    return ing;
                }
            }
        }
        return null;
    }

    public void ShuffleLanes()
    {
        for (int i = 0; i < activeLanes.Count; i++)
        {
            if (IsLaneLocked(i)) continue;

            List<Ingredient> laneIngredients = activeLanes[i];
            if (laneIngredients.Count <= 1) continue;

            List<List<Ingredient>> chunks = new List<List<Ingredient>>();
            List<bool> isFixedChunk = new List<bool>();
            int j = 0;
            while (j < laneIngredients.Count)
            {
                Ingredient ing = laneIngredients[j];
                List<Ingredient> currentChunk = new List<Ingredient>();
                
                int currentGroupId = ing.groupId;
                bool hasFixedItem = false;

                // Gom toan bo cac nguyen lieu cung groupId vao 1 chunk
                while (j < laneIngredients.Count && laneIngredients[j].groupId == currentGroupId)
                {
                    if (laneIngredients[j].mysteryGroup != null || laneIngredients[j].hasKey)
                    {
                        hasFixedItem = true;
                    }
                    currentChunk.Add(laneIngredients[j]);
                    j++;
                }
                
                chunks.Add(currentChunk);
                isFixedChunk.Add(hasFixedItem);
            }

            // Tach cac chunk duoc phep xao bai
            List<List<Ingredient>> shufflableChunks = new List<List<Ingredient>>();
            for (int k = 0; k < chunks.Count; k++)
            {
                if (!isFixedChunk[k]) shufflableChunks.Add(chunks[k]);
            }

            // Xao cac chunk nay
            for (int c = shufflableChunks.Count - 1; c > 0; c--)
            {
                int r = Random.Range(0, c + 1);
                List<Ingredient> temp = shufflableChunks[c];
                shufflableChunks[c] = shufflableChunks[r];
                shufflableChunks[r] = temp;
            }

            // Ghep lai vao danh sach (giu nguyen vi tri cua cac chunk co dinh)
            laneIngredients.Clear();
            int shuffleIndex = 0;
            for (int k = 0; k < chunks.Count; k++)
            {
                if (isFixedChunk[k])
                {
                    laneIngredients.AddRange(chunks[k]);
                }
                else
                {
                    laneIngredients.AddRange(shufflableChunks[shuffleIndex]);
                    shuffleIndex++;
                }
            }

            StartCoroutine(FlipAndMoveLane(i));
        }
    }

    IEnumerator FlipAndMoveLane(int laneIndex)
    {
        List<Ingredient> laneIngredients = activeLanes[laneIndex];
        
        Transform startPoint = null;
        var layoutEnum = levelManager.currentLevel.selectedLayout;
        LaneLayoutConfig activeLayout = null;
        if (layoutEnum == LevelData.LayoutType.TwoLanes) activeLayout = layout2Lanes;
        else if (layoutEnum == LevelData.LayoutType.ThreeLanes) activeLayout = layout3Lanes;
        else if (layoutEnum == LevelData.LayoutType.FourLanes) activeLayout = layout4Lanes;
        
        if (activeLayout != null && laneIndex < activeLayout.startPoints.Length)
        {
            startPoint = activeLayout.startPoints[laneIndex];
        }

        if (startPoint == null) yield break;

        float t = 0;
        float flipDuration = 0.15f;
        while (t < flipDuration)
        {
            t += Time.deltaTime;
            float scaleX = Mathf.Lerp(1, 0, t / flipDuration);
            foreach (Ingredient ing in laneIngredients)
            {
                if (ing == null) continue;
                Vector3 scale = ing.transform.localScale;
                float scaleMult = levelManager.database != null ? levelManager.database.GetIngredientScale(ing.ingredientType) : 1.0f;
                scale.x = scaleX * startPoint.localScale.x * ingredientScale * scaleMult;
                ing.transform.localScale = scale;
            }
            yield return null;
        }

        foreach (Ingredient ing in laneIngredients)
        {
            if (ing == null) continue;
            if (moveCoroutines.ContainsKey(ing) && moveCoroutines[ing] != null)
            {
                StopCoroutine(moveCoroutines[ing]);
            }
        }
        
        for (int j = 0; j < laneIngredients.Count; j++)
        {
            Ingredient ing = laneIngredients[j];
            if (ing == null) continue;
            float dist = j * itemSpacing;
            Vector3 targetPos = startPoint.position + Vector3.down * dist;
            ing.transform.position = targetPos;
        }

        UpdatePositionsAndSorting(laneIndex, startPoint);

        t = 0;
        while (t < flipDuration)
        {
            t += Time.deltaTime;
            float scaleX = Mathf.Lerp(0, 1, t / flipDuration);
            foreach (Ingredient ing in laneIngredients)
            {
                if (ing == null) continue;
                Vector3 scale = ing.transform.localScale;
                float scaleMult = levelManager.database != null ? levelManager.database.GetIngredientScale(ing.ingredientType) : 1.0f;
                scale.x = scaleX * startPoint.localScale.x * ingredientScale * scaleMult;
                ing.transform.localScale = scale;
            }
            yield return null;
        }
    }
}
