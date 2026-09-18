using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip bgmClip;
    public AudioClip ingredientToSlotClip;     // Nh?y l�n bang chuy?n / slot
    public AudioClip ingredientToCauldronClip; // Nh?y v�o n?i (v?c)
    public AudioClip cauldronBoilClip;
    public AudioClip potionShootClip;
    public AudioClip potionDeliverClip;
    public AudioClip buttonClickClip;
    public AudioClip keyUnlockClip;
    public AudioClip keyUnlockFinalClip;

    [Header("Volume Adjustments")]
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float ingredientToSlotVolume = 1f;
    [Range(0f, 1f)] public float ingredientToCauldronVolume = 1f;
    [Range(0f, 1f)] public float cauldronBoilVolume = 1f;
    [Range(0f, 1f)] public float potionShootVolume = 1f;
    [Range(0f, 1f)] public float potionDeliverVolume = 1f;
    [Range(0f, 1f)] public float buttonClickVolume = 1f;
    [Range(0f, 1f)] public float keyUnlockVolume = 1f;
    [Range(0f, 1f)] public float keyUnlockFinalVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
    }

    private void Update()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    // Khi nguy�n li?u nh?y l�n slot/bang chuy?n
    public void PlayIngredientToSlot()
    {
        if (sfxSource != null && ingredientToSlotClip != null)
        {
            sfxSource.PlayOneShot(ingredientToSlotClip, ingredientToSlotVolume);
        }
    }

    // Khi nguy�n li?u nh?y v�o v?c
    public void PlayIngredientToCauldron()
    {
        if (sfxSource != null && ingredientToCauldronClip != null)
        {
            sfxSource.PlayOneShot(ingredientToCauldronClip, ingredientToCauldronVolume);
        }
    }

    public void PlayCauldronBoil()
    {
        if (sfxSource != null && cauldronBoilClip != null)
        {
            sfxSource.PlayOneShot(cauldronBoilClip, cauldronBoilVolume);
        }
    }

    public void PlayPotionShoot()
    {
        if (sfxSource != null && potionShootClip != null)
        {
            sfxSource.PlayOneShot(potionShootClip, potionShootVolume);
        }
    }

    public void PlayPotionDeliver()
    {
        if (sfxSource != null && potionDeliverClip != null)
        {
            sfxSource.PlayOneShot(potionDeliverClip, potionDeliverVolume);
        }
    }
    
    public void PlayButtonClick()
    {
        if (sfxSource != null && buttonClickClip != null)
        {
            sfxSource.PlayOneShot(buttonClickClip, buttonClickVolume);
        }
    }

    public void PlayKeyUnlock(bool isFinalKey)
    {
        if (sfxSource != null)
        {
            if (isFinalKey && keyUnlockFinalClip != null)
            {
                sfxSource.PlayOneShot(keyUnlockFinalClip, keyUnlockFinalVolume);
            }
            else if (!isFinalKey && keyUnlockClip != null)
            {
                sfxSource.PlayOneShot(keyUnlockClip, keyUnlockVolume);
            }
        }
    }
}
