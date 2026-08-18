using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Ingredient Settings")]
    public GameObject ingredientPrefab; 
    public float verticalSpacing = 1.0f; 
    public int maxSortingOrder = 40;

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
                                for (int count = 0; count < group.quantity; count++)
                                {
                                    bool isKeyHolder = group.hasKey && count == group.keyIndex;
                                    SpawnIngredient(group.ingredientType, startPoint, i, laneIngredients, isKeyHolder);
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

    void SpawnIngredient(IngredientType type, Transform startPoint, int laneIndex, List<Ingredient> laneIngredients, bool hasKey)
    {
        if (ingredientPrefab == null || startPoint == null) return;
        
        GameObject go = Instantiate(ingredientPrefab, startPoint.position, Quaternion.identity, transform);
        float scaleMult = levelManager.database != null ? levelManager.database.GetIngredientScale(type) : 1.0f;
        go.transform.localScale = startPoint.localScale * scaleMult;
        Ingredient ing = go.GetComponent<Ingredient>();
        ing.ingredientType = type;
        ing.laneManager = this; 
        ing.laneIndex = laneIndex;
        ing.hasKey = hasKey;
        
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

        for (int i = 0; i < laneIngredients.Count; i++)
        {
            Ingredient ing = laneIngredients[i];
            if (ing == null) continue;
            
            Vector3 targetPos = startPoint.position + new Vector3(0, -i * verticalSpacing, 0);
            
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
}
