using UnityEngine;
using System.Collections;

public class BreakingPlatform : MonoBehaviour
{
    [Header("Breaking Platform Settings")]
    public float breakDelay = 2f;
    public float respawnTime = 3f;
    public bool respawns = true;
    [Header("Shake Settings")]
    public float shakeIntensity = 0.2f;
    public bool shakes = true;
    
    [Header("Audio")]
    [SerializeField] private AudioClip breakSFX;
    [SerializeField, Range(0f,1f)] private float sfxVolume = 1f;
    [SerializeField] private bool use2DSound = false;
    [SerializeField] private Vector2 pitchRandom = new Vector2(0.98f, 1.02f);


    private Vector3 originalPosition;
    private bool isBroken = false;
    private MeshRenderer meshRenderer;
    private Collider platformCollider;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.position;
        meshRenderer = GetComponent<MeshRenderer>();
        platformCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator BreakPlatform()
    {
        isBroken = true;
        
        if (shakes)
        {
            float elapsed = 0f;
            while (elapsed < breakDelay)
            {
                float xOffset = Random.Range(-shakeIntensity, shakeIntensity);
                float zOffset = Random.Range(-shakeIntensity, shakeIntensity);
                transform.position = originalPosition + new Vector3(xOffset, 0, zOffset);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = originalPosition; // reset position
        } else {
            yield return new WaitForSeconds(breakDelay);
        }

        Break();

        if (respawns)
        {
            yield return new WaitForSeconds(respawnTime);
            Respawn();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isBroken)
        {
            // Check if player is landing on TOP of platform
            // Normal.y <- 0.5f means collision is from above (landing on top)
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f) // Player is on top!
                {
                    PlaySFXAt(breakSFX, transform.position);
                    StartCoroutine(BreakPlatform());
                    break;
                }
            }
        }
    }

    void Break()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false; // hide platform
        }
        if (platformCollider != null)
        {
            platformCollider.enabled = false; // disable collisions
        }
    }

    void Respawn()
    {
        transform.position = originalPosition;
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true; // show platform
        }
        if (platformCollider != null)
        {
            platformCollider.enabled = true; // enable collisions
        }
        isBroken = false;
    }

    private void PlaySFXAt(AudioClip clip, Vector3 pos)
    {
        if (clip == null) return;

        var go = new GameObject($"OneShot_{clip.name}");
        go.transform.position = pos;

        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = use2DSound ? 0f : 1f;   // 0=2D, 1=3D
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 2f;
        src.maxDistance = 20f;

        src.pitch = Mathf.Clamp(Random.Range(pitchRandom.x, pitchRandom.y), 0.5f, 2f);

        src.PlayOneShot(clip, sfxVolume);

        Destroy(go, clip.length / Mathf.Max(src.pitch, 0.01f) + 0.05f);
    }
}

