using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        ScaleBackground();
    }

    void Update()
    {
        ScaleBackground();
    }

    void ScaleBackground()
    {
        if (sr == null || sr.sprite == null || Camera.main == null) return;

        float cameraHeight = Camera.main.orthographicSize * 2f;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        if (spriteWidth <= 0f || spriteHeight <= 0f) return;

        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        float maxScale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(maxScale, maxScale, 1f);
    }
}
