using UnityEngine;

public class CollectibleHealth : MonoBehaviour
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
            // Add health to player
            if (other.CompareTag("Player"))
        {
            PlayerInfo playerInfo = other.GetComponent<PlayerInfo>();
            if (playerInfo != null)
            {
                playerInfo.Heal(value);
                Debug.Log($"Health pickup healed player for {value} health!");
            }
            else
            {
                Debug.LogError("Player has no PlayerInfo component");
            }
            }
            
            // Spawn VFX at cheese position
            if (collectVFX != null)
            {
                Instantiate(collectVFX, transform.position, Quaternion.identity);
            }

            // Play sound effect
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }
            
            // Destroy the pickup after short delay
            Destroy(gameObject, destroyDelay);
        }
    }
}