using UnityEngine;
using UnityEngine.Events;
using System.Collections;
//using UnityEngine.SceneManagement; // reload scene

public class PlayerInfo : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private Collider2D[] colliders;

    // Singleton instance
    public static PlayerInfo Instance;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    private bool isInvincible = false;

    [Header("Death Settings")]
    [Tooltip("Delay before death callback/GameOver to let the death animation play")]
    [SerializeField] private float deathDelay = 0.1f;
    private bool isDead = false;

    [Header("Score Settings")]
    private int score = 0;

    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent OnDeath;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>(true);

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
        if (isInvincible || isDead)
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
        if (isDead) return; 
        isDead = true;
        isInvincible = true; // Prevent multiple death calls
        
        Debug.Log("Player died");

        //OnDeath?.Invoke();

        if (anim)
        {
            // drive Animator conditions to force death transition from any state
            anim.SetBool("isDead", true); // Pair this with Animator transitions (isDead == true)
            anim.ResetTrigger("Hurt");
            anim.SetBool("Walking", false);
            anim.SetBool("Jumping", false);
            anim.SetBool("isClimbing", false);
            anim.SetTrigger("death"); // Trigger the death animation
        }

        if (rb)
        {
            // freeze rigidbody motion to avoid sliding/physics after death
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        // disable colliders to avoid further interactions after death
        if (colliders != null)
        {
            foreach (var c in colliders) c.enabled = false;
        }

        //StartCoroutine(DeathCallbackAfterDelay());
    }

    private IEnumerator DeathCallbackAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        OnDeath?.Invoke();
    }

    // private void Restart() // reload scene
    // {
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    // }


    // Helper methods
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetScore() => score;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0;
    public bool IsInvincible() => isInvincible;
}