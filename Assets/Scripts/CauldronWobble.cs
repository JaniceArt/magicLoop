using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CauldronWobble : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] private float jumpHeight = 0.08f;

    [Header("Wobble")]
    [SerializeField] private float wobbleAngle = 5f;

    [Header("Animation")]
    [SerializeField] private float duration = 0.55f;

    [Header("Squash & Stretch")]
    [SerializeField] private float stretchAmount = 0.04f;

    private bool isAnimating = false;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private void Awake()
    {
        // Lưu trạng thái ban đầu
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalScale = transform.localScale;
    }

    // =========================
    // TEST BẰNG SPACE
    // =========================

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            PlayWobble();
        }
    }

    // =========================
    // KÍCH HOẠT ANIMATION
    // =========================

    public void PlayWobble()
    {
        if (isAnimating)
            return;

        StartCoroutine(WobbleAnimation());
    }

    // =========================
    // MAIN ANIMATION
    // =========================

    private IEnumerator WobbleAnimation()
    {
        isAnimating = true;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(timer / duration);

            // =================================
            // 1. JUMP
            // =================================

            // Parabola:
            // 0 -> lên cao -> rơi xuống 0
            float jump =
                Mathf.Sin(progress * Mathf.PI)
                * jumpHeight;


            // =================================
            // 2. WOBBLE
            // =================================

            // Giảm dần biên độ khi animation kết thúc
            float wobbleStrength =
                1f - progress;

            float rotation =
                Mathf.Sin(progress * Mathf.PI * 4f)
                * wobbleAngle
                * wobbleStrength;


            // =================================
            // 3. SQUASH & STRETCH
            // =================================

            float stretch =
                Mathf.Sin(progress * Mathf.PI)
                * stretchAmount;

            Vector3 scale = originalScale;

            // Khi bật lên -> hơi kéo dài theo Y
            scale.y *= 1f + stretch;

            // Đồng thời hơi thu lại theo X
            scale.x *= 1f - stretch * 0.5f;


            // =================================
            // APPLY
            // =================================

            transform.localPosition =
                originalPosition +
                Vector3.up * jump;

            transform.localRotation =
                originalRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    rotation
                );

            transform.localScale = scale;

            yield return null;
        }

        // =================================
        // RESET VỀ TRẠNG THÁI BAN ĐẦU
        // =================================

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        transform.localScale = originalScale;

        isAnimating = false;
    }
}