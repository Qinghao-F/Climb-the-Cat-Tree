using UnityEngine;

/// <summary>
/// Place this on the bottom kill zone collider child object.
/// Don't think it is neccessary and the player will probably
/// die before reaching it, but just in case.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CatKillZone : MonoBehaviour
{
    private RisingCat parentCat;

    void Start()
    {
        parentCat = GetComponentInParent<RisingCat>();
        
        // safety check
        Collider col = GetComponent<Collider>();
        if (col.isTrigger)
        {
            Debug.LogWarning("CatKillZone: Setting collider to non-trigger mode");
            col.isTrigger = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (parentCat != null)
        {
            parentCat.OnKillZoneCollisionEnter(collision);
        }
    }
}