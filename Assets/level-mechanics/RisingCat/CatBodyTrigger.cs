using UnityEngine;

/// <summary>
/// Place this on the main body trigger collider child object.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CatBodyTrigger : MonoBehaviour
{
    private RisingCat parentCat;

    void Start()
    {
        parentCat = GetComponentInParent<RisingCat>();
        
        // safety check
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("CatBodyTrigger: Setting collider to trigger mode");
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (parentCat != null)
        {
            parentCat.OnBodyTriggerEnter(other);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (parentCat != null)
        {
            parentCat.OnBodyTriggerStay(other);
        }
    }
}