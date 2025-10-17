using UnityEngine;
using UnityEngine.Events;

public class PlayerInfo : MonoBehaviour
{
    private Animator anim;

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
        anim = GetComponentInChildren<Animator>();

        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        OnHealthChanged ??= new UnityEvent<int>();
        OnScoreChanged ??= new UnityEvent<int>();
        OnDeath ??= new UnityEvent();
        currentHealth = maxHealth;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth);
        OnScoreChanged?.Invoke(score);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        OnHealthChanged?.Invoke(currentHealth);

        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        Debug.Log($"Health: {currentHealth}");
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
        Debug.Log($"Health: {currentHealth}");
    }

    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
        Debug.Log($"Score: {score}");
    }

    private void Die()
    {
        Debug.Log("Player died");
        OnDeath?.Invoke();
    }

    // helper methods
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetScore() => score;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0;
}