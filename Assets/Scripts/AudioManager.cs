using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource boilSource;

    [Header("Audio Clips")]
    public AudioClip bgmClip;
    public AudioClip ingredientJumpClip;
    public AudioClip cauldronBoilClip;
    public AudioClip potionDeliverClip;

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
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlayIngredientJump()
    {
        if (sfxSource != null && ingredientJumpClip != null)
        {
            sfxSource.PlayOneShot(ingredientJumpClip);
        }
    }

    public void PlayCauldronBoil()
    {
        if (boilSource != null && cauldronBoilClip != null)
        {
            boilSource.clip = cauldronBoilClip;
            boilSource.loop = true; // Lặp lại tiếng sôi
            if (!boilSource.isPlaying)
            {
                boilSource.Play();
            }
        }
    }

    public void StopCauldronBoil()
    {
        if (boilSource != null && boilSource.isPlaying)
        {
            boilSource.Stop();
        }
    }

    public void PlayPotionDeliver()
    {
        if (sfxSource != null && potionDeliverClip != null)
        {
            sfxSource.PlayOneShot(potionDeliverClip);
        }
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
        if (sfxSource != null) sfxSource.mute = !isOn;
        if (boilSource != null) boilSource.mute = !isOn;
    }
}
