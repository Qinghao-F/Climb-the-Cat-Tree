using UnityEngine;

/// <summary>
/// Automatically destroys the GameObject after the ParticleSystem finishes playing.
/// Attach this script to any VFX prefab (e.g., cheese pickup spark, paw puff).
/// </summary>
public class AutoDestroyVFX : MonoBehaviour
{
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null)
        {
            Debug.LogWarning($"{name}: No ParticleSystem found on this VFX prefab.");
        }
    }

    private void Update()
    {
        // If the particle system exists and has finished playing → destroy object
        if (ps != null && !ps.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
