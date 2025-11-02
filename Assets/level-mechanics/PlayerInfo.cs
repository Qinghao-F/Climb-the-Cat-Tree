using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerInfo : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rb; 
    private Collider[] colliders;

    // Singleton instance
    public static PlayerInfo Instance;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    private bool isInvincible = false;

    [Header("Death Settings")]
    [Tooltip("Delay before scene reloads to let the death animation play")]
    [SerializeField] private float deathDelay = 2f;
    private bool isDead = false;

    [Header("Score Settings")]
    private int score = 0;

    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent OnDeath;

    // Audio SFX
    [Header("SFX")]
    [Tooltip("AudioSource for one-shot SFX (Play On Awake OFF, Loop OFF)")]
    [SerializeField] private AudioSource sfxSource;

    [Tooltip("Mouse squeaks when hurt (random one will play)")]
    [SerializeField] private AudioClip[] hurtClips;

    [Tooltip("Mouse death sounds (random one will play)")]
    [SerializeField] private AudioClip[] deathClips;

    [Tooltip("Volume for hurt squeak")]
    [Range(0f, 1f)] [SerializeField] private float hurtVolume = 0.9f;

    [Tooltip("Volume for death sound")]
    [Range(0f, 1f)] [SerializeField] private float deathVolume = 1.0f;

    [Tooltip("Random pitch range, e.g., 0.95–1.05")]
    [SerializeField] private Vector2 pitchJitter = new Vector2(0.95f, 1.05f);

    [Tooltip("Cooldown to avoid spammy hurt squeaks (seconds)")]
    [SerializeField] private float hurtSfxCooldown = 0.1f;

    private float _lastHurtSfxTime = -999f;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();  // Changed to 3D
        colliders = GetComponentsInChildren<Collider>(true);  // Changed to 3D

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
            // hurt sound
            TryPlayHurtSqueak();

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

        // Invoke death event immediately (for UI updates, etc.)
        OnDeath?.Invoke();

        if (anim)
        {
            // drive Animator conditions to force death transition from any state
            anim.SetBool("isDead", true);
            anim.ResetTrigger("Hurt");
            anim.SetBool("Walking", false);
            anim.SetBool("Jumping", false);
            anim.SetBool("isClimbing", false);
            anim.SetTrigger("death");
        }

        if (rb)
        {
            // freeze rigidbody motion to avoid sliding/physics after death
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        
        // disable colliders to avoid further interactions after death
        if (colliders != null)
        {
            foreach (var c in colliders) c.enabled = false;
        }

        // DEATH SOUND
        PlayDeathSound();

        // Reload scene after delay
        StartCoroutine(ReloadSceneAfterDelay());
    }

    private IEnumerator ReloadSceneAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        
        Debug.Log("Reloading scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Helper methods
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetScore() => score;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;
    public bool IsAlive() => currentHealth > 0;
    public bool IsInvincible() => isInvincible;

     // Audio Helpers
    private void TryPlayHurtSqueak()
    {
        if (sfxSource == null || hurtClips == null || hurtClips.Length == 0) return;
        if (Time.time - _lastHurtSfxTime < hurtSfxCooldown) return;

        PlayOneShotRandom(hurtClips, hurtVolume);
        _lastHurtSfxTime = Time.time;
    }

    private void PlayDeathSound()
    {
        if (sfxSource == null || deathClips == null || deathClips.Length == 0) return;
        PlayOneShotRandom(deathClips, deathVolume);
    }

    private void PlayOneShotRandom(AudioClip[] bank, float volume)
    {
        if (bank == null || bank.Length == 0) return;
        var clip = bank[Random.Range(0, bank.Length)];
        sfxSource.pitch = Random.Range(pitchJitter.x, pitchJitter.y);
        sfxSource.PlayOneShot(clip, volume);
    }
    
    public void AnimEvent_DeathSFX() => PlayDeathSound();
    public void AnimEvent_HurtSFX()  => TryPlayHurtSqueak();
}