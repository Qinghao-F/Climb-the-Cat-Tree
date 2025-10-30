using UnityEngine;
using System.Collections;

public class CatMeow : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] meows; 
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Timing")]
    public float intervalSeconds = 20f;
    public bool useRealtime = false; 

    [Header("Optional Randomness")]
    public Vector2 pitchRange = new Vector2(1f, 1f);
    public Vector2 jitter = new Vector2(0f, 0f);

    Coroutine loop;

    void Reset()
    {
        // 自动找 AudioSource
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        loop = StartCoroutine(MeowLoop());
    }

    void OnDisable()
    {
        if (loop != null) StopCoroutine(loop);
    }

    IEnumerator MeowLoop()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        while (true)
        {
            AudioClip clip = null;
            if (meows != null && meows.Length > 0)
                clip = meows[Random.Range(0, meows.Length)];
            else
                clip = audioSource.clip;

            if (clip != null)
            {
                float oldPitch = audioSource.pitch;
                audioSource.pitch = Mathf.Clamp(Random.Range(pitchRange.x, pitchRange.y), 0.5f, 3f);

                audioSource.PlayOneShot(clip, volume);

                audioSource.pitch = oldPitch;
            }

            float jitterSec = (jitter == Vector2.zero) ? 0f : Random.Range(jitter.x, jitter.y);
            float wait = Mathf.Max(0.05f, intervalSeconds + jitterSec);

            if (useRealtime) yield return new WaitForSecondsRealtime(wait);
            else yield return new WaitForSeconds(wait);
        }
    }
}

