using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource boilSource; // Nguồn phát âm thanh sủi bọt (loop)

    [Header("Audio Clips")]
    public AudioClip bgmClip;
    [Range(0f, 1f)] public float bgmVolume = 1f;

    public AudioClip ingredientJumpClip;
    [Range(0f, 1f)] public float ingredientJumpVolume = 1f;

    public AudioClip cauldronBoilClip;
    [Range(0f, 1f)] public float cauldronBoilVolume = 1f;

    public AudioClip potionDeliverClip;
    [Range(0f, 1f)] public float potionDeliverVolume = 1f;

    public AudioClip ingredientAbsorbClip;
    [Range(0f, 1f)] public float ingredientAbsorbVolume = 1f;

    public AudioClip potionSpawnClip;
    [Range(0f, 1f)] public float potionSpawnVolume = 1f;

    public AudioClip keyHitClip;
    [Range(0f, 1f)] public float keyHitVolume = 1f;

    public AudioClip keyUnlockClip;
    [Range(0f, 1f)] public float keyUnlockVolume = 1f;

    [Header("UI Sounds")]
    public AudioClip buttonClickClip;
    [Range(0f, 1f)] public float buttonClickVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Bỏ comment nếu muốn giữ Audio Manager giữa các màn chơi
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
            bgmSource.volume = bgmVolume;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlayIngredientJump()
    {
        if (sfxSource != null && ingredientJumpClip != null) sfxSource.PlayOneShot(ingredientJumpClip, ingredientJumpVolume);
    }

    public void PlayCauldronBoil()
    {
        // Sử dụng boilSource riêng biệt để có thể loop và stop
        if (boilSource != null && cauldronBoilClip != null)
        {
            boilSource.clip = cauldronBoilClip;
            boilSource.volume = cauldronBoilVolume;
            boilSource.loop = true;
            boilSource.Play();
        }
        else if (sfxSource != null && cauldronBoilClip != null)
        {
            sfxSource.PlayOneShot(cauldronBoilClip, cauldronBoilVolume);
        }
    }

    public void StopCauldronBoil()
    {
        if (boilSource != null)
        {
            boilSource.Stop();
        }
    }

    public void PlayPotionDeliver()
    {
        if (sfxSource != null && potionDeliverClip != null) sfxSource.PlayOneShot(potionDeliverClip, potionDeliverVolume);
    }

    public void PlayIngredientAbsorb()
    {
        if (sfxSource != null && ingredientAbsorbClip != null) sfxSource.PlayOneShot(ingredientAbsorbClip, ingredientAbsorbVolume);
    }

    public void PlayPotionSpawn()
    {
        if (sfxSource != null && potionSpawnClip != null) sfxSource.PlayOneShot(potionSpawnClip, potionSpawnVolume);
    }

    public void PlayKeyHit()
    {
        if (sfxSource != null && keyHitClip != null) sfxSource.PlayOneShot(keyHitClip, keyHitVolume);
    }

    public void PlayKeyUnlock()
    {
        if (sfxSource != null && keyUnlockClip != null) sfxSource.PlayOneShot(keyUnlockClip, keyUnlockVolume);
    }

    public void PlayButtonClick()
    {
        if (sfxSource != null && buttonClickClip != null) sfxSource.PlayOneShot(buttonClickClip, buttonClickVolume);
    }

    public void ToggleMusic(bool isOn)
    {
        if (bgmSource != null)
        {
            bgmSource.mute = !isOn;
        }
    }

    public void ToggleSound(bool isOn)
    {
        if (sfxSource != null)
        {
            sfxSource.mute = !isOn;
        }
        if (boilSource != null)
        {
            boilSource.mute = !isOn;
        }
    }
}
