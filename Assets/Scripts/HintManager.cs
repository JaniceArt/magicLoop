using UnityEngine;
using UnityEngine.InputSystem;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance;

    [Header("Hint Settings")]
    public GameObject hintWandPrefab;
    public float idleTimeLevel1 = 3f;
    public float idleTimeOtherLevels = 7f;

    [Header("Wand Animation")]
    public float floatSpeed = 4f;
    public float floatAmount = 0.2f;
    public Vector3 offset = new Vector3(0.4f, 0.2f, 0f);

    private float idleTimer = 0f;
    private GameObject currentWand;
    private Vector3 wandBasePos;

    private Ingredient currentTarget;
    private GameObject outlineObj;
    private float hintStartTime = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        bool isTouched = false;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            isTouched = true;
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isTouched = true;
        }

        if (isTouched)
        {
            ResetHint();
            return;
        }

        if (LevelManager.Instance == null || LevelManager.Instance.isGameOver || LevelManager.Instance.currentLevel == null)
            return;

        float threshold = (LevelManager.Instance.currentLevel.levelNumber == 1) ? idleTimeLevel1 : idleTimeOtherLevels;

        idleTimer += Time.deltaTime;

        if (currentTarget != null)
        {
            if (currentTarget.isBeingAbsorbed || currentTarget.isFlying || !IsIngredientStillNeeded(currentTarget))
            {
                ResetHint();
                return;
            }
        }
        else if (currentWand != null && currentWand.activeSelf)
        {
            ResetHint();
            return;
        }

        if (idleTimer >= threshold)
        {
            if (currentWand == null || !currentWand.activeSelf)
            {
                Debug.Log($"[HintManager] idleTimer={idleTimer:F1} >= threshold={threshold:F1}, calling ShowHint");
                ShowHint();
            }
            else if (currentWand != null && currentWand.activeSelf)
            {
                AnimateWand();
            }
        }

        if (currentTarget != null && outlineObj != null)
        {
            SpriteRenderer outlineSR = outlineObj.GetComponent<SpriteRenderer>();
            if (outlineSR != null)
            {
                float pingPong = Mathf.PingPong(Time.time * 2f, 1f); 
                float outlineScale = Mathf.Lerp(1.05f, 1.15f, pingPong);
                outlineObj.transform.localScale = new Vector3(outlineScale, outlineScale, 1f);

                float timeActive = Time.time - hintStartTime;
                float fadeInAlpha = Mathf.Clamp01(timeActive / 0.5f);

                Color c = outlineSR.color;
                c.a = Mathf.Lerp(0.3f, 0.9f, pingPong) * fadeInAlpha;
                outlineSR.color = c;
            }
        }
    }

    public void ResetHint()
    {
        idleTimer = 0f;
        if (currentWand != null)
        {
            currentWand.SetActive(false);
        }

        if (currentTarget != null)
        {
            currentTarget = null;
        }

        if (outlineObj != null)
        {
            Destroy(outlineObj);
            outlineObj = null;
        }
    }

    private void ShowHint()
    {
        Debug.Log($"[HintManager] ShowHint | hintWandPrefab={(hintWandPrefab == null ? "NULL" : hintWandPrefab.name)}");
        if (hintWandPrefab == null) return;

        Ingredient targetIng = FindBestIngredientToClick();
        Debug.Log($"[HintManager] FindBestIngredientToClick returned: {(targetIng == null ? "NULL" : targetIng.ingredientType.ToString())}");

        if (targetIng != null)
        {
            if (currentTarget != targetIng)
            {
                if (currentTarget != null)
                {
                    currentTarget = null;
                }
                if (outlineObj != null)
                {
                    Destroy(outlineObj);
                    outlineObj = null;
                }
                currentTarget = targetIng;
                hintStartTime = Time.time;
                
                SpriteRenderer targetSR = currentTarget.GetComponent<SpriteRenderer>();
                if (targetSR != null)
                {
                    outlineObj = new GameObject("HintWhiteOutline");
                    outlineObj.transform.SetParent(currentTarget.transform);
                    outlineObj.transform.localPosition = Vector3.zero;

                    SpriteRenderer outlineSR = outlineObj.AddComponent<SpriteRenderer>();
                    outlineSR.sprite = targetSR.sprite;
                    
                    Shader whiteShader = Shader.Find("GUI/Text Shader");
                    if (whiteShader != null)
                    {
                        outlineSR.material = new Material(whiteShader);
                    }
                    
                    outlineSR.color = new Color(1f, 1f, 1f, 0f); 
                    outlineSR.sortingLayerID = targetSR.sortingLayerID;
                    outlineSR.sortingOrder = targetSR.sortingOrder - 1; 
                }
            }

            if (currentWand == null)
            {
                currentWand = Instantiate(hintWandPrefab);
            }

            SpriteRenderer[] srs = currentWand.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in srs)
            {
                sr.sortingOrder = 200;
            }
            
            currentWand.SetActive(true);
            wandBasePos = targetIng.transform.position + offset;
            currentWand.transform.position = wandBasePos;
        }
    }

    private Ingredient FindBestIngredientToClick()
    {
        if (LaneManager.Instance == null) { Debug.Log("[HintManager] LaneManager.Instance is NULL!"); return null; }

        Cauldron[] dbgCauldrons = FindObjectsByType<Cauldron>(FindObjectsSortMode.None);
        Debug.Log($"[HintManager] Found {dbgCauldrons.Length} cauldrons");
        foreach (Cauldron dbgC in dbgCauldrons)
        {
            Debug.Log($"[HintManager] Cauldron isBusy={dbgC.isBusy} NeedsAnyIngredient={dbgC.NeedsAnyIngredient()}");
        }

        Ingredient globalClosest = null;
        float globalClosestDist = float.MaxValue;

        Cauldron[] cauldrons = FindObjectsByType<Cauldron>(FindObjectsSortMode.None);
        foreach (Cauldron c in cauldrons)
        {
            if (c.NeedsAnyIngredient())
            {
                IngredientType neededType = c.GetCurrentNeededType();
                Vector3 cauldronPos = (c.absorbPoint != null) ? c.absorbPoint.position : c.transform.position;

                for (int i = 0; i < LaneManager.Instance.ActiveLanesCount; i++)
                {
                    if (LaneManager.Instance.IsLaneLocked(i)) continue;

                    var laneList = LaneManager.Instance.GetLaneIngredients(i);
                    if (laneList == null) continue;
                    
                    int scanLimit = Mathf.Min(11, laneList.Count);
                    for (int j = 0; j < scanLimit; j++)
                    {
                        Ingredient ing = laneList[j];
                        if (ing != null && ing.ingredientType == neededType && !ing.hasKey && !ing.isBeingAbsorbed && ing.mysteryGroup == null)
                        {
                            float dist = Vector2.Distance(cauldronPos, ing.transform.position);
                            if (dist < globalClosestDist)
                            {
                                globalClosestDist = dist;
                                globalClosest = ing;
                            }
                        }
                    }
                }
            }
        }

        return globalClosest;
    }

    private bool IsIngredientStillNeeded(Ingredient ing)
    {
        if (ing == null) return false;
        Cauldron[] cauldrons = FindObjectsByType<Cauldron>(FindObjectsSortMode.None);
        foreach (Cauldron c in cauldrons)
        {
            if (c.NeedsIngredient(ing.ingredientType))
            {
                return true;
            }
        }
        return false;
    }

    private void AnimateWand()
    {
        if (currentWand != null && currentTarget != null)
        {
            wandBasePos = currentTarget.transform.position + offset;
            float newY = wandBasePos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
            currentWand.transform.position = new Vector3(wandBasePos.x, newY, wandBasePos.z);
        }
    }
}
