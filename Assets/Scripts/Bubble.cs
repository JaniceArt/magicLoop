using UnityEngine;

public class Bubble : MonoBehaviour
{
    [Header("Size")]
    [SerializeField] private float minSize = 0.3f;
    [SerializeField] private float maxSize = 0.5f;

    [Header("Animation")]
    [SerializeField] private float growTime = 0.15f;
    [SerializeField] private float lifeTime = 0.35f;
    [SerializeField] private float popTime = 0.1f;

    [Header("Wobble")]
    [SerializeField] private float wobbleAmount = 0.03f;
    [SerializeField] private float wobbleSpeed = 8f;

    private Vector3 startPosition;
    private Vector3 targetScale;

    private float timer;
    private bool popping;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize()
    {
        timer = 0f;
        popping = false;

        startPosition = transform.position;

        float randomSize = Random.Range(minSize, maxSize);

        targetScale = Vector3.one * randomSize;

        transform.localScale = Vector3.zero;

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }

    private void Update()
    {
        if (popping)
        {
            Pop();
            return;
        }

        timer += Time.deltaTime;

        // Bubble phồng lên
        float growProgress =
            Mathf.Clamp01(timer / growTime);

        transform.localScale =
            Vector3.Lerp(
                Vector3.zero,
                targetScale,
                EaseOutBack(growProgress)
            );

        // Bubble rung nhẹ
        float wobbleX =
            Mathf.Sin(Time.time * wobbleSpeed)
            * wobbleAmount;

        float wobbleY =
            Mathf.Cos(Time.time * wobbleSpeed * 0.8f)
            * wobbleAmount;

        transform.position =
            startPosition +
            new Vector3(
                wobbleX,
                wobbleY,
                0f
            );

        // Đến thời gian thì pop
        if (timer >= lifeTime)
        {
            popping = true;
            timer = 0f;
        }
    }

    private void Pop()
    {
        timer += Time.deltaTime;

        float progress =
            Mathf.Clamp01(timer / popTime);

        float scale =
            Mathf.Lerp(
                targetScale.x,
                targetScale.x * 1.5f,
                progress
            );

        transform.localScale =
            Vector3.one * scale;

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;

            color.a = 1f - progress;

            spriteRenderer.color = color;
        }

        if (timer >= popTime)
        {
            Destroy(gameObject);
        }
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f +
               c3 * Mathf.Pow(t - 1f, 3f) +
               c1 * Mathf.Pow(t - 1f, 2f);
    }
}