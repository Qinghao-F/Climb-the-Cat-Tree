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
    Collider ropeCollider;            // Collider of the rope currently being climbed
    float lockedZ;                    // Player's preserved Z position

    readonly List<Collider> playerCols = new();
    readonly List<Collider> ropeCols   = new();

    // Saved physics properties
    bool savedUseGravity;
    bool savedIsKinematic;
    RigidbodyConstraints savedConstraints;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCols.AddRange(GetComponentsInChildren<Collider>(true));
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
    void OnTriggerStay(Collider other) { if (!isClimbing) TryBegin(other); }

    void TryBegin(Collider hit)
    {
        if (isClimbing) return;
        if (!LooksLikeRope(hit.transform)) return;

        ropeCollider = hit;
        ropeCols.Clear();
        hit.transform.GetComponentsInChildren(true, ropeCols);

        BeginClimb();
    }

    // Activates climbing state
    void BeginClimb()
    {
        isClimbing = true;
        lockedZ = transform.position.z;

        // Save physics state and restrict movement
        savedUseGravity  = rb.useGravity;
        savedIsKinematic = rb.isKinematic;
        savedConstraints = rb.constraints;

        rb.useGravity  = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        ZeroVelocity();
    }

    // Exit climbing state when leaving the rope trigger
    void OnTriggerExit(Collider other)
    {
        if (!isClimbing || ropeCollider == null) return;

        if (other == ropeCollider ||
            other.transform == ropeCollider.transform ||
            other.transform.IsChildOf(ropeCollider.transform) ||
            ropeCollider.transform.IsChildOf(other.transform))
        {
            StopClimb();
        }
    }

    void Update()
    {
        if (!isClimbing) return;

        // Allow only vertical input
        float v = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(v) > 0.01f)
            transform.position += Vector3.up * (v * climbSpeed * Time.deltaTime);

        // Detach with Space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopClimb();
            StartCoroutine(TemporarilyIgnoreRope(ignoreTime));
        }
    }

    void FixedUpdate()
    {
        if (!isClimbing || ropeCollider == null) return;

        // Snap X to rope collider centre, keep Y as is, lock Z
        float centreX = ropeCollider.bounds.center.x;
        Vector3 p = transform.position;
        transform.position = new Vector3(centreX, p.y, lockedZ);
    }

    // Restores normal physics after climbing
    void StopClimb()
    {
        isClimbing = false;

        rb.isKinematic = savedIsKinematic;
        rb.useGravity  = savedUseGravity;
        rb.constraints = savedConstraints;

        ropeCollider = null;
    }

    // Clears any residual velocity
    void ZeroVelocity()
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector3.zero;
#else
        rb.velocity = Vector3.zero;
#endif
    }

    // Temporarily disables rope collisions after detaching
    IEnumerator TemporarilyIgnoreRope(float seconds)
    {
        if (seconds <= 0f || ropeCols.Count == 0) yield break;

        foreach (var pc in playerCols)
            if (pc && pc.enabled)
                foreach (var rc in ropeCols)
                    if (rc && rc.enabled)
                        Physics.IgnoreCollision(pc, rc, true);

        yield return new WaitForSeconds(seconds);

        foreach (var pc in playerCols)
            if (pc)
                foreach (var rc in ropeCols)
                    if (rc)
                        Physics.IgnoreCollision(pc, rc, false);
    }
}
