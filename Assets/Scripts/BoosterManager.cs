using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoosterManager : MonoBehaviour
{
    public static BoosterManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void UseShuffleBooster()
    {
        if (LaneManager.Instance != null)
        {
            if (UIManager.Instance.SpendCoins(50))
            {
                LaneManager.Instance.ShuffleLanes();
            }
        }
    }

    public void UseMagnetBooster()
    {
        if (LaneManager.Instance != null)
        {
            // Kiểm tra xem có vạc nào đang cần nguyên liệu không
            Cauldron[] cauldrons = FindObjectsByType<Cauldron>(FindObjectsSortMode.None);
            bool anyNeed = false;
            foreach (Cauldron c in cauldrons)
            {
                if (c.isBusy && c.NeedsAnyIngredient())
                {
                    anyNeed = true;
                    break;
                }
            }
            
            // Nếu không vạc nào cần thì không cho xài (đỡ phí 50 vàng)
            if (!anyNeed) return;
            
            // Trừ tiền trước khi dùng
            if (!UIManager.Instance.SpendCoins(50)) return;
            
            bool foundAndSucked = false;

            // Lấy TẤT CẢ các nguyên liệu hợp lệ tại thời điểm bấm nút
            List<Ingredient> validIngredients = LaneManager.Instance.GetCurrentlyValidIngredientsForMagnet();
            Debug.Log($"[Magnet] Bắt đầu quét. Tìm thấy {validIngredients.Count} nguyên liệu hợp lệ trên sân.");
            foreach (Ingredient v in validIngredients) {
                Debug.Log($" - Hợp lệ: {v.ingredientType}");
            }

            foreach (Cauldron c in cauldrons)
            {
                if (c.isBusy && c.NeedsAnyIngredient())
                {
                    IngredientType neededType = c.GetCurrentNeededType();
                    int maxNeeded = c.GetRemainingQuantity();
                    Debug.Log($"[Magnet] Vạc {c.gameObject.name} đang cần {maxNeeded} cái {neededType}");
                    
                    List<Ingredient> ingredientsToSuck = new List<Ingredient>();
                    
                    // Tìm trong danh sách hợp lệ đã chốt từ trước
                    for (int i = validIngredients.Count - 1; i >= 0; i--)
                    {
                        if (validIngredients[i].ingredientType == neededType)
                        {
                            ingredientsToSuck.Add(validIngredients[i]);
                            validIngredients.RemoveAt(i);
                            if (ingredientsToSuck.Count >= maxNeeded) break;
                        }
                    }

                    Debug.Log($"[Magnet] Vạc {c.gameObject.name} gom được {ingredientsToSuck.Count} cái {neededType}");

                    if (ingredientsToSuck.Count > 0)
                    {
                        foreach (Ingredient ing in ingredientsToSuck)
                        {
                            LaneManager.Instance.PopIngredient(ing);
                            c.AbsorbFromMagnet(ing);
                        }
                        foundAndSucked = true;
                    }
                }
                else
                {
                    Debug.Log($"[Magnet] Vạc {c.gameObject.name} không bận hoặc không cần nguyên liệu.");
                }
            }
            
            if (!foundAndSucked)
            {
                Debug.Log("Nam cham: Khong tim thay nguyen lieu phu hop!");
            }
        }
    }
}
