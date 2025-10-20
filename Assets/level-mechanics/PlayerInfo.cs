using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerInfo : MonoBehaviour
{
    private Animator anim;

    // Singleton instance
    public static PlayerInfo Instance;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    private bool isInvincible = false;

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
        // Don't take damage if invincible or already dead
        if (isInvincible || currentHealth <= 0)
        {
            Debug.Log("Player is invincible or dead - no damage taken");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        OnHealthChanged?.Invoke(currentHealth);

        // Play hurt animation
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Activate invincibility frames
            StartCoroutine(BecomeInvincible());
        }

        Debug.Log($"Health: {currentHealth}");
    }

    IEnumerator BecomeInvincible()
    {
        isInvincible = true;
        Debug.Log("Invincibility started");

        // Wait for invincibility duration
        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
        Debug.Log("Invincibility ended");
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
        isInvincible = true; // Prevent multiple death calls
        OnDeath?.Invoke();
    }

    // Helper methods
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetScore() => score;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0;
    public bool IsInvincible() => isInvincible;
}