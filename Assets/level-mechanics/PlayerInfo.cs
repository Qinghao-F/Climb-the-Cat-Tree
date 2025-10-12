using UnityEngine;
using UnityEngine.Events;

public class PlayerInfo : MonoBehaviour
{
    // Singleton instance
    public static PlayerInfo Instance;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Score Settings")]
    private int score = 0;

    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent OnDeath;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // persistent?
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
        OnScoreChanged?.Invoke(score);
    }

    void Update()
    {
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); 
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return; // Can't heal if dead

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); 
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
        Debug.Log($"Score: {score}");
    }

    void Die()
    {
        Debug.Log("Player died");
        OnDeath?.Invoke();
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetScore() => score;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0;
}