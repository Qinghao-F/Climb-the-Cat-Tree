using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] private int value = 10;
    
    [Header("Animation Settings")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobSpeed = 1f;
    
    [Header("Effects")]
    [SerializeField] private GameObject collectVFX; // Drag your VFX_CheesePickup prefab here
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
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
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