using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private Transform bubbleContainer;

    [Header("Water Area")]
    [SerializeField] private Transform water;

    [Header("Ellipse")]
    [SerializeField] private float width = 2.5f;
    [SerializeField] private float height = 1.3f;

    [Header("Spawn")]
    [SerializeField] private float minSpawnTime = 0.15f;
    [SerializeField] private float maxSpawnTime = 0.6f;

    [SerializeField] private int maxBubbles = 8;

    private float timer;
    private float nextSpawnTime;

    private void Start()
    {
        SetNextSpawnTime();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            SpawnBubble();

            timer = 0f;

            SetNextSpawnTime();
        }
    }

    private void SetNextSpawnTime()
    {
        nextSpawnTime =
            Random.Range(
                minSpawnTime,
                maxSpawnTime
            );
    }

    private void SpawnBubble()
    {
        if (bubblePrefab == null)
            return;

        if (bubbleContainer != null &&
            bubbleContainer.childCount >= maxBubbles)
            return;

        Vector2 randomPoint = Random.insideUnitCircle * 0.45f;

        Vector3 localPosition =
            new Vector3(
                randomPoint.x * width * 0.5f,
                randomPoint.y * height * 0.5f,
                0f
            );

        Vector3 worldPosition =
            water.TransformPoint(localPosition);

        GameObject bubble =
            Instantiate(
                bubblePrefab,
                worldPosition,
                Quaternion.identity,
                bubbleContainer
            );

        Bubble bubbleScript =
            bubble.GetComponent<Bubble>();

        if (bubbleScript != null)
        {
            bubbleScript.Initialize();
        }
    }
}