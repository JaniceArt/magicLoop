using UnityEngine;

public class BlinkingGlow : MonoBehaviour
{
    private GameObject glowObject;
    private SpriteRenderer glowSr;
    
    public float blinkSpeed = 2f; // Tốc độ nháy chậm lại
    public Color glowColor = new Color(1f, 1f, 1f, 0.8f); // Màu trắng tinh

    void Start()
    {
        SpriteRenderer mainSr = GetComponent<SpriteRenderer>();
        if (mainSr != null)
        {
            // Tạo một GameObject con làm viền sáng
            glowObject = new GameObject("GlowOutline");
            glowObject.transform.SetParent(transform);
            glowObject.transform.localPosition = Vector3.zero;
            
            // To hơn bản gốc một xíu (1.15 lần) để tạo thành cái viền bọc bên ngoài
            glowObject.transform.localScale = new Vector3(1.15f, 1.15f, 1f);
            
            glowSr = glowObject.AddComponent<SpriteRenderer>();
            glowSr.sprite = mainSr.sprite;
            glowSr.color = glowColor;
            
            // Ép nó thành màu trắng tinh bằng cách đổi Shader sang GUI/Text Shader
            Shader whiteShader = Shader.Find("GUI/Text Shader");
            if (whiteShader != null)
            {
                glowSr.material = new Material(whiteShader);
            }
            
            // Đặt Sorting Order sao cho nằm ngay phía SAU hình gốc
            glowSr.sortingOrder = mainSr.sortingOrder - 1; 
        }
    }

    void Update()
    {
        if (glowSr != null)
        {
            // Hiệu ứng nhấp nháy Alpha (độ trong suốt)
            Color c = glowSr.color;
            // PingPong đi từ 0.2 đến 0.9 để nháy rõ hơn
            c.a = 0.2f + Mathf.PingPong(Time.time * blinkSpeed, 0.7f);
            glowSr.color = c;
        }
    }

    void OnDestroy()
    {
        if (glowObject != null)
        {
            Destroy(glowObject);
        }
    }
}
