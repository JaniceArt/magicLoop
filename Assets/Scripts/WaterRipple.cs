using UnityEngine;

public class WaterRipple : MonoBehaviour
{
    [Header("Size")]
    [SerializeField] private float startScale = 0.05f;
    [SerializeField] private float endScale = 1.0f;

    [Header("Timing")]
    [SerializeField] private float duration = 3.0f;
    [SerializeField] private float startDelay = 0f;

    [Header("Alpha")]
    [SerializeField] private float maxAlpha = 0.55f;

    private float timer;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        timer = -startDelay;

        transform.localScale =
            Vector3.one * startScale;

        SetAlpha(0f);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Chưa đến thời gian xuất hiện
        if (timer < 0f)
        {
            SetAlpha(0f);
            return;
        }

        // =========================
        // LOOP
        // =========================

        if (timer >= duration)
        {
            timer -= duration;
        }

        float progress = timer / duration;


        // =========================
        // EXPAND
        // =========================

        float scale = Mathf.Lerp(
            startScale,
            endScale,
            progress
        );

        transform.localScale =
            Vector3.one * scale;


        // =========================
        // FADE
        // =========================

        float alpha;

        if (progress < 0.85f)
        {
            alpha = maxAlpha;
        }
        else
        {
            float fadeProgress = Mathf.InverseLerp(
                0.85f,
                1.0f,
                progress
            );

            alpha = Mathf.Lerp(
                maxAlpha,
                0f,
                fadeProgress
            );
        }

        SetAlpha(alpha);
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderer == null)
            return;

        Color color = spriteRenderer.color;
        color.a = alpha;

        spriteRenderer.color = color;
    }
}