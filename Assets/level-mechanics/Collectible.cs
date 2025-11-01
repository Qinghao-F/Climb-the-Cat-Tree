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
    [SerializeField, Range(0f, 1f)] private float soundVolume = 0.75f; // per-item volume
    //[SerializeField] private bool use2DSound = true;
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
        if (!other.CompareTag("Player")) return;

        // Add score to player
        PlayerInfo.Instance?.AddScore(value);

        // Spawn VFX
        if (collectVFX != null)
            Instantiate(collectVFX, transform.position, Quaternion.identity);

        // Play SFX via AudioManager
        if (collectSound != null)
        {
            AudioManager.Instance?.PlaySFX(collectSound, soundVolume);
        }
        
        // Destroy collectible
        Destroy(gameObject, 0.1f);
    }
}
