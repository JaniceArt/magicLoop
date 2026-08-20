using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BoosterButtonState : MonoBehaviour
{
    public int cost = 50;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
    }

    void Update()
    {
        if (UIManager.Instance != null && button != null)
        {
            // Ki?m tra xem ti?n có d? d? mua không
            bool hasEnoughCoins = UIManager.Instance.GetCurrentCoins() >= cost;
            
            // B?t/t?t nút. Khi t?t (interactable = false), Unity s? t? d?ng
            // chuy?n nút sang màu t?i ho?c hình tr?ng den (do s?p c?u hình trong Button)
            button.interactable = hasEnoughCoins;
        }
    }
}
