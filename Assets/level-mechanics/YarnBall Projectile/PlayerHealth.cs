using UnityEngine;

/// <summary>
/// A tiny health component so hazards can call TakeDamage.
/// Replace with your team's full system when ready.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth = 3;

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log($"[Health] Player took {amount}, now {currentHealth}");
        if (currentHealth <= 0)
        {
            // TODO: plug into your game-over flow
            Debug.Log("[Health] Player dead (hook up your game over).");
        }
    }
}
