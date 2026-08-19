using UnityEngine;

public class MonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Wobble Settings")]
    [SerializeField] private float speed = 2f;

    [SerializeField] private float scaleAmount = 0.02f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        float wave = Mathf.Sin(Time.time * speed);

        float scaleX = 1f + wave * scaleAmount;
        float scaleY = 1f - wave * scaleAmount;

        transform.localScale = new Vector3(
            originalScale.x * scaleX,
            originalScale.y * scaleY,
            originalScale.z
        );
    }
}
