using UnityEngine;

public class LockVisual : MonoBehaviour
{
    public SpriteRenderer numberSprite;
    
    [Header("Animation References")]
    public Transform lockAndChain;
    public Transform popupBackground;
    public SpriteRenderer backgroundPanel;

    public void UpdateNumber(int count, GameDatabase db)
    {
        if (numberSprite != null && db != null && db.keyNumberSprites != null)
        {
            if (count > 0 && count < db.keyNumberSprites.Length)
            {
                numberSprite.gameObject.SetActive(true);
                numberSprite.sprite = db.keyNumberSprites[count];
            }
            else
            {
                numberSprite.gameObject.SetActive(false);
            }
        }
    }

    public void Unlock()
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Unlock");
            Destroy(gameObject, 1.0f);
        }
        else
        {
            StartCoroutine(BreakAnimation());
        }
    }

    private System.Collections.IEnumerator BreakAnimation()
    {
        if (numberSprite != null) numberSprite.gameObject.SetActive(false);
        if (popupBackground != null) popupBackground.gameObject.SetActive(false);

        SpriteRenderer lockSr = lockAndChain != null ? lockAndChain.GetComponent<SpriteRenderer>() : null;
        if (lockSr != null)
        {
            lockSr.enabled = false; 
            
            Sprite fragSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 10f);

            for (int i = 0; i < 20; i++)
            {
                GameObject frag = new GameObject("Fragment");
                frag.transform.position = lockAndChain.position + new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(-0.8f, 0.8f), 0);
                frag.transform.localScale = lockAndChain.localScale * Random.Range(0.2f, 0.6f);
                SpriteRenderer fragSr = frag.AddComponent<SpriteRenderer>();
                fragSr.sprite = fragSprite;
                fragSr.sortingOrder = lockSr.sortingOrder + 1;
                
                // Color shards randomly between gold, brown, and grey
                float colorType = Random.value;
                if (colorType < 0.33f) fragSr.color = new Color(1f, 0.8f, 0.2f); // Gold
                else if (colorType < 0.66f) fragSr.color = new Color(0.6f, 0.6f, 0.6f); // Grey (chain)
                else fragSr.color = new Color(0.5f, 0.3f, 0.1f); // Dark brown

                StartCoroutine(AnimateFragment(frag.transform, fragSr));
            }
        }

        float elapsed = 0;
        float duration = 1.0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            if (backgroundPanel != null)
            {
                Color c = backgroundPanel.color;
                c.a = Mathf.Lerp(1, 0, elapsed / (duration * 0.5f));
                backgroundPanel.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private System.Collections.IEnumerator AnimateFragment(Transform frag, SpriteRenderer sr)
    {
        float elapsed = 0;
        float duration = 1.0f;
        Vector3 velocity = new Vector3(Random.Range(-4f, 4f), Random.Range(2f, 6f), 0);
        float rotationSpeed = Random.Range(-500f, 500f);

        while (elapsed < duration && frag != null)
        {
            elapsed += Time.deltaTime;
            velocity += Vector3.down * 15f * Time.deltaTime; 
            frag.position += velocity * Time.deltaTime;
            frag.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            
            Color c = sr.color;
            c.a = Mathf.Lerp(1, 0, elapsed / duration);
            sr.color = c;

            yield return null;
        }

        if (frag != null) Destroy(frag.gameObject);
    }
}
