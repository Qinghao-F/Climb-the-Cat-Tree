using UnityEngine;
using System.Collections;

public class BreakingPlatform : MonoBehaviour
{
    [Header("Breaking Platform Settings")]
    public float breakDelay = 2f;
    public float respawnTime = 3f;
    public bool respawns = true;
    [Header("Shake Settings")]
    public float shakeIntensity = 0.2f;
    public bool shakes = true;


    private Vector3 originalPosition;
    private bool isBroken = false;
    private MeshRenderer meshRenderer;
    private Collider platformCollider;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.position;
        meshRenderer = GetComponent<MeshRenderer>();
        platformCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator BreakPlatform()
    {
        isBroken = true;
        
        if (shakes)
        {
            float elapsed = 0f;
            while (elapsed < breakDelay)
            {
                float xOffset = Random.Range(-shakeIntensity, shakeIntensity);
                float zOffset = Random.Range(-shakeIntensity, shakeIntensity);
                transform.position = originalPosition + new Vector3(xOffset, 0, zOffset);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = originalPosition; // reset position
        } else {
            yield return new WaitForSeconds(breakDelay);
        }

        Break();

        if (respawns)
        {
            yield return new WaitForSeconds(respawnTime);
            Respawn();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isBroken)
        {
            // Check if player is landing on TOP of platform
            // Normal.y <- 0.5f means collision is from above (landing on top)
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f) // Player is on top!
                {
                    StartCoroutine(BreakPlatform());
                    break;
                }
            }
        }
    }

    void Break()
    {
        // can do sound here
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false; // hide platform
        }
        if (platformCollider != null)
        {
            platformCollider.enabled = false; // disable collisions
        }
    }

    void Respawn()
    {
        transform.position = originalPosition;
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true; // show platform
        }
        if (platformCollider != null)
        {
            platformCollider.enabled = true; // enable collisions
        }
        isBroken = false;
    }
}
