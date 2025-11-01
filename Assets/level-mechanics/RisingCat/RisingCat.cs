using UnityEngine;

/// <summary>
/// Rising cat that constantly moves upward, creating a lower bound for the level.
/// Main body (trigger) deals 1 damage over time. Bottom collider instantly kills player.
/// 
/// SETUP INSTRUCTIONS:
/// 1. Add TWO child objects to the cat:
///    - "CatBodyTrigger" with a Box Collider (Is Trigger = true) - covers most of cat
///    - "CatBottomKillZone" with a Box Collider (Is Trigger = false) - at very bottom of cat
/// 2. Both children should have this script's tag methods called via their colliders
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RisingCat : MonoBehaviour
{
    public static RisingCat Instance { get; private set; }

    [Header("Movement")]
    public float riseSpeed = 2f;

    [Header("Damage")]
    public int bodyDamage = 1;
    
    public float damageCooldown = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip bodyDamageSound;
    [SerializeField] private AudioClip instantDeathSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 0.7f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private float lastDamageTime = -999f;
    private Rigidbody rb;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        rb = GetComponent<Rigidbody>();
        
        rb.isKinematic = true;
        rb.useGravity = false;
        
        if (showDebugLogs)
            Debug.Log("RisingCat initialized");
    }

    void Update()
    {
        rb.MovePosition(transform.position + Vector3.up * (riseSpeed * Time.deltaTime));
    }

    // Called by CatBodyTrigger collider
    public void OnBodyTriggerEnter(Collider other)
    {
        if (showDebugLogs)
            Debug.Log($"Body trigger enter: {other.gameObject.name}");
            
        if (other.CompareTag("Player"))
        {
            DamagePlayer();
        }
    }

    // Called by CatBodyTrigger collider
    public void OnBodyTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                DamagePlayer();
            }
        }
    }

    // Called by CatBottomKillZone collider
    public void OnKillZoneCollisionEnter(Collision collision)
    {
        if (showDebugLogs)
            Debug.Log($"Kill zone collision: {collision.gameObject.name}");
            
        if (collision.gameObject.CompareTag("Player"))
        {
            InstantKillPlayer();
        }
    }

    void DamagePlayer()
    {
        if (Time.time < lastDamageTime + damageCooldown) return;

        if (PlayerInfo.Instance != null)
        {
            if (showDebugLogs)
                Debug.Log($"Dealing {bodyDamage} damage to player");
                
            PlayerInfo.Instance.TakeDamage(bodyDamage);
            lastDamageTime = Time.time;

            if (bodyDamageSound != null)
                AudioManager.Instance?.PlaySFX(bodyDamageSound, soundVolume);
        }
    }

    void InstantKillPlayer()
    {
        if (PlayerInfo.Instance != null && PlayerInfo.Instance.GetCurrentHealth() > 0)
        {
            if (showDebugLogs)
                Debug.Log("Player hit kill zone - instant death!");
                
            PlayerInfo.Instance.TakeDamage(999);
            
            if (instantDeathSound != null)
                AudioManager.Instance?.PlaySFX(instantDeathSound, soundVolume);
        }
    }

    public float GetCurrentY()
    {
        return transform.position.y;
    }
}