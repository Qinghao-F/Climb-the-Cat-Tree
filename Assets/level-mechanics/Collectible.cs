using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] private int value = 1;
    
    [Header("Animation Settings")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobSpeed = 1f;
    
    [Header("Effects")]
    [SerializeField] private GameObject collectVFX; 
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float destroyDelay = 0.1f;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;
    [SerializeField] private bool use2DSound = true;
    [SerializeField] private Vector2 pitchRandom = new Vector2(0.98f, 1.02f);


    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotate around Y axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add score to player
            PlayerInfo.Instance?.AddScore(value);
            
            // Spawn VFX at cheese position
            if (collectVFX != null)
            {
                Instantiate(collectVFX, transform.position, Quaternion.identity);
            }
            
            // Play sound effect
            if (collectSound != null)
            {
                var go = new GameObject("CheeseSFX");
                go.transform.position = transform.position;
                var src = go.AddComponent<AudioSource>();

                // 2D/3D
                src.spatialBlend = use2DSound ? 0f : 1f;
                src.rolloffMode = AudioRolloffMode.Linear;
                src.minDistance = 2f;
                src.maxDistance = 20f;

                // slight random pitch
                src.pitch = Mathf.Clamp(Random.Range(pitchRandom.x, pitchRandom.y), 0.5f, 2f);

                src.PlayOneShot(collectSound, soundVolume);
                Destroy(go, collectSound.length / Mathf.Max(src.pitch, 0.01f) + 0.05f);
            }

            // Log for debugging
            Debug.Log($"Collected cheese! Value: {value}");
            
            // UI part: cheese counter
            CheeseUI.Instance?.Add(1);
            
            // Destroy the cheese after short delay
            Destroy(gameObject, destroyDelay);
        }
    }
}