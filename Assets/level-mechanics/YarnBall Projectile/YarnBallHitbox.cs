using UnityEngine;

/// <summary>
/// Minimal hitbox: on touching something that has a PlayerInfo component,
/// calls TakeDamage once. Keeps out of the way of general physics.
/// </summary>
public class YarnBallHitbox : MonoBehaviour
{
    [Tooltip("Damage dealt to the player on touch.")]
    public int damage = 1;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInfo playerInfo = other.GetComponent<PlayerInfo>();
            if (playerInfo != null)
            {
                playerInfo.TakeDamage(damage);
                Debug.Log($"Yarn Ball hit player for {damage} damage");
            }
            else
            {
                Debug.LogError("Player has no PlayerInfo component");
            }
        }
    }
}
