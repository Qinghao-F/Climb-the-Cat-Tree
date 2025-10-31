using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("------- Audio Source -------")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;

    [Header("------- Audio Clip -------")]
    public AudioClip background;
    public AudioClip death;
    public AudioClip walk;
    public AudioClip jump;

    [Header("------- Volume -------")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f; // global SFX volume slider

    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // background music
        musicSource.clip = background;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    private void Update()
    {
        // optional: adjust music volume dynamically
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Play a sound effect with optional individual volume
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || SFXSource == null) return;

        // scale by global SFX volume
        SFXSource.PlayOneShot(clip, Mathf.Clamp01(volume * sfxVolume));
    }
}
