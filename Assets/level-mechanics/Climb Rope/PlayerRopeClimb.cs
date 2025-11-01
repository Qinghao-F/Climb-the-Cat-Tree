using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rope climbing behaviour.
/// Player is snapped to the centre of the rope collider (X only) while keeping its own Z position unchanged.
/// Allows only vertical movement when climbing. Press Space to detach and temporarily disable rope collision.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerRopeClimb : MonoBehaviour
{
    [Header("Climb")]
    [Tooltip("Vertical speed whilst attached.")]
    public float climbSpeed = 2.6f;

    [Header("Detach")]
    [Tooltip("Seconds to ignore rope collisions after detaching.")]
    public float ignoreTime = 0.25f;

    // Internal state
    Rigidbody rb;
    bool isClimbing;
    public bool IsClimbing => isClimbing;
    
    // Track ALL rope colliders we're currently touching
    readonly HashSet<Collider> activeRopeColliders = new();
    Transform currentRopeTransform;   // Root transform of current rope
    float lockedZ;                    // Player's preserved Z position

    readonly List<Collider> playerCols = new();
    readonly List<Collider> allRopeCols = new();

    // Saved physics properties
    bool savedUseGravity;
    bool savedIsKinematic;
    RigidbodyConstraints savedConstraints;
    private Animator anim;

    // Input caching
    private float verticalInput;
    
    // Collision ignore tracking
    private Coroutine ignoreCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCols.AddRange(GetComponentsInChildren<Collider>(true));
        anim = GetComponentInChildren<Animator>();
    }

    // Checks if the object hit behaves like a rope
    static bool LooksLikeRope(Transform t)
    {
        if (t.CompareTag("Rope")) return true;

        // Accept ancestors containing components named "RopeMaker" or "RopeMarker"
        var comps = t.GetComponentsInParent<Component>(true);
        foreach (var c in comps)
        {
            var n = c.GetType().Name;
            if (n == "RopeMaker" || n == "RopeMarker") return true;
        }
        return false;
    }

    // Start climbing when entering a rope trigger
    void OnTriggerEnter(Collider other) => TryBegin(other);
    void OnTriggerStay(Collider other) 
    { 
        if (LooksLikeRope(other.transform))
        {
            activeRopeColliders.Add(other);
            if (!isClimbing) TryBegin(other);
        }
    }

    void TryBegin(Collider hit)
    {
        if (!LooksLikeRope(hit.transform)) return;
        
        // Stop any ongoing collision ignore coroutine
        if (ignoreCoroutine != null)
        {
            StopCoroutine(ignoreCoroutine);
            ignoreCoroutine = null;
            RestoreAllCollisions();
        }

        activeRopeColliders.Add(hit);
        
        if (isClimbing) return; // Already climbing

        // Get the root rope transform
        currentRopeTransform = GetRopeRoot(hit.transform);
        
        // Collect all colliders from this rope system
        allRopeCols.Clear();
        currentRopeTransform.GetComponentsInChildren(true, allRopeCols);

        BeginClimb();
    }

    // Find the root rope transform (the one with RopeMaker/RopeMarker or tagged Rope)
    Transform GetRopeRoot(Transform t)
    {
        Transform root = t;
        Transform current = t;
        
        while (current != null)
        {
            if (current.CompareTag("Rope"))
            {
                root = current;
            }
            
            var comps = current.GetComponents<Component>();
            foreach (var c in comps)
            {
                var n = c.GetType().Name;
                if (n == "RopeMaker" || n == "RopeMarker")
                {
                    root = current;
                    break;
                }
            }
            
            current = current.parent;
        }
        
        return root;
    }

    // Activates climbing state
    void BeginClimb()
    {
        isClimbing = true;
        lockedZ = transform.position.z;

        // Save physics state and restrict movement
        savedUseGravity = rb.useGravity;
        savedIsKinematic = rb.isKinematic;
        savedConstraints = rb.constraints;

        // Set Kinematic and disable gravity for pure position control
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        if (anim) anim.SetBool("isClimbing", true);

        ZeroVelocity();
    }

    // Exit climbing state when leaving the rope trigger
    void OnTriggerExit(Collider other)
    {
        if (!LooksLikeRope(other.transform)) return;
        
        activeRopeColliders.Remove(other);

        // Only stop climbing if we've exited ALL rope colliders
        if (isClimbing && activeRopeColliders.Count == 0)
        {
            StopClimb();
        }
    }

    void Update()
    {
        if (!isClimbing) return;

        // Cache input in Update
        verticalInput = Input.GetAxisRaw("Vertical");

        // Set Animator speed based on movement
        if (anim)
            anim.speed = Mathf.Abs(verticalInput); // 0 pauses animation, >0 plays

        // Detach from rope with Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopClimb();
            ignoreCoroutine = StartCoroutine(TemporarilyIgnoreRope(ignoreTime));
        }
    }

    void FixedUpdate()
    {
        if (!isClimbing || activeRopeColliders.Count == 0) return;

        // Apply movement using cached input
        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            transform.position += Vector3.up * (verticalInput * climbSpeed * Time.fixedDeltaTime);
        }

        // Snap X to the center of any active rope collider (use the first one)
        Collider snapTarget = null;
        foreach (var col in activeRopeColliders)
        {
            if (col != null && col.enabled)
            {
                snapTarget = col;
                break;
            }
        }

        if (snapTarget != null)
        {
            float centreX = snapTarget.bounds.center.x;
            Vector3 p = transform.position;
            transform.position = new Vector3(centreX, p.y, lockedZ);
        }
    }

    // Restores normal physics after climbing
    void StopClimb()
    {
        isClimbing = false;
        activeRopeColliders.Clear();

        // Restore saved physics properties
        rb.isKinematic = savedIsKinematic;
        rb.useGravity = savedUseGravity;
        rb.constraints = savedConstraints;

        if (anim)
        {
            anim.SetBool("isClimbing", false);
            anim.speed = 1f;
        }

        currentRopeTransform = null;
    }

    // Clears any residual velocity
    void ZeroVelocity()
    {
        if (!rb.isKinematic)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
        }
    }

    // Temporarily disables rope collisions after detaching
    IEnumerator TemporarilyIgnoreRope(float seconds)
    {
        if (seconds <= 0f || allRopeCols.Count == 0) yield break;

        // Ignore collisions
        foreach (var pc in playerCols)
        {
            if (pc && pc.enabled)
            {
                foreach (var rc in allRopeCols)
                {
                    if (rc && rc.enabled)
                    {
                        Physics.IgnoreCollision(pc, rc, true);
                    }
                }
            }
        }

        yield return new WaitForSeconds(seconds);

        // Re-enable collisions
        RestoreAllCollisions();
        
        ignoreCoroutine = null;
    }

    void RestoreAllCollisions()
    {
        foreach (var pc in playerCols)
        {
            if (pc)
            {
                foreach (var rc in allRopeCols)
                {
                    if (rc)
                    {
                        Physics.IgnoreCollision(pc, rc, false);
                    }
                }
            }
        }
    }
}