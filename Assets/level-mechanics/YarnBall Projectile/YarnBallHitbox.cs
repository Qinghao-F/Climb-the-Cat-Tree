using UnityEngine;

/// <summary>
/// Minimal hitbox: on touching something that has a PlayerHealth component,
/// calls TakeDamage once. Keeps out of the way of general physics.
/// </summary>
public class YarnBallHitbox : MonoBehaviour
{
    [Tooltip("Damage dealt to the player on touch.")]
    public int damage = 1;

    private void OnTriggerEnter(Collider other)
    {
        var hp = other.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
            // Optional: the ball could continue or be removed on hit. We keep it simple: continue flying.
            // If you prefer to remove on hit, uncomment the next line:
            // Destroy(gameObject);
        }
    }
}
