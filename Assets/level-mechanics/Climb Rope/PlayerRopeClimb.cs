using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player rope climbing (Space-only exit) with a short post-jump no-collision window.
/// - Touch RopeMarker trigger -> enter climb mode (RB kinematic, gravity off, snap beside rope).
/// - While climbing -> only Vertical input; Left/Right switches the hanging side.
/// - Press Space -> jump off forward+up, nudge outward, and temporarily ignore
///   collisions with the rope to avoid getting stuck.
/// Unity 6 safe: never sets velocity while RB is kinematic.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerRopeClimb : MonoBehaviour
{
    [Header("Climb")]
    [Min(0f)] public float climbSpeed = 2.5f;
    [Min(0f)] public float jumpOffForce = 6f;
    [Range(0f, 1f)] public float xzSnapStrength = 1f;

    [Header("Grip Offset")]
    [Min(0f)] public float sideOffset = 0.25f;                 // hang beside the rope
    [Range(0.01f, 20f)] public float sideBlendSpeed = 10f;     // side switch smoothness

    [Header("Jump Direction")]
    public Vector3 jumpDirectionLocal = new Vector3(0f, 1f, 0.75f);

    [Header("Post-jump Unstick")]
    [Min(0f)] public float postJumpNoCollideTime = 0.25f;      // seconds
    [Min(0f)] public float outwardNudge = 0.12f;               // metres

    [Header("Optional Booster")]
    public Behaviour holdJumpBooster; // e.g. HoldJumpBooster (will be disabled while climbing)

    // runtime
    private Rigidbody rb;
    private bool isClimbing, wantJump;
    private Transform currentRope;
    private Vector3 ropeAxisXZ, desiredSnapXZ, currentSnapXZ;
    private int gripSide = +1; // -1 left, +1 right

    // saved physics
    private bool savedUseGravity, savedIsKinematic;
    private RigidbodyConstraints savedConstraints;

    // colliders cache for ignore-collision
    private readonly List<Collider> playerCols = new();
    private readonly List<Collider> ropeCols   = new();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        GetComponentsInChildren(true, playerCols);
    }

    void Start()
    {
        if (holdJumpBooster == null)
        {
            var maybe = GetComponent("HoldJumpBooster") as Behaviour;
            if (maybe != null) holdJumpBooster = maybe;
        }
    }

    // -------- marker-driven detection --------
    void OnTriggerEnter(Collider other) { TryBegin(other); }
    void OnTriggerStay(Collider other)  { if (!isClimbing) TryBegin(other); }
    void OnTriggerExit(Collider other)
    {
        if (!isClimbing || currentRope == null) return;
        if (other.transform == currentRope || other.transform.IsChildOf(currentRope))
            ExitClimb(false);
    }

    void Update()
    {
        if (!isClimbing) return;

        if (Input.GetKeyDown(KeyCode.Space)) wantJump = true;

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) gripSide = +1;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))  gripSide = -1;
    }

    void FixedUpdate()
    {
        if (!isClimbing) return;

        Vector3 ropeRight = currentRope ? currentRope.right : Vector3.right;
        Vector3 offsetXZ  = new Vector3(ropeRight.x, 0f, ropeRight.z).normalized * (sideOffset * gripSide);
        desiredSnapXZ     = ropeAxisXZ + offsetXZ;

        currentSnapXZ = Vector3.Lerp(
            currentSnapXZ, desiredSnapXZ,
            1f - Mathf.Exp(-sideBlendSpeed * Time.fixedDeltaTime)
        );

        // 1) stick to rope XZ
        Vector3 p = transform.position;
        Vector3 target = new Vector3(currentSnapXZ.x, p.y, currentSnapXZ.z);
        transform.position = Vector3.Lerp(p, target, Mathf.Clamp01(xzSnapStrength));

        // 2) vertical-only motion
        float v = Input.GetAxis("Vertical");
        if (Mathf.Abs(v) > 0.0001f)
            transform.position += Vector3.up * (v * climbSpeed * Time.fixedDeltaTime);

        // 3) jump off
        if (wantJump) ExitClimb(true);
    }

    // -------------- enter / exit --------------

    private void TryBegin(Collider other)
    {
        var marker = other.GetComponentInParent<RopeMarker>();
        if (marker == null || isClimbing) return;

        currentRope = marker.transform;

        // save physics
        savedUseGravity  = rb.useGravity;
        savedIsKinematic = rb.isKinematic;
        savedConstraints = rb.constraints;

        // stop motion BEFORE making kinematic
        rb.useGravity = false;
        SafeZeroVelocity();              // <<< never writes when kinematic

        // take over
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // rope axis (world XZ)
        Vector3 axis = currentRope.position;
        if (axis == Vector3.zero && other != null) axis = other.bounds.center;
        ropeAxisXZ = new Vector3(axis.x, 0f, axis.z);

        // initialise snap & hard stick once
        currentSnapXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 rRightXZ = new Vector3(currentRope.right.x, 0f, currentRope.right.z).normalized;
        desiredSnapXZ = ropeAxisXZ + rRightXZ * (sideOffset * gripSide);
        transform.position = new Vector3(desiredSnapXZ.x, transform.position.y, desiredSnapXZ.z);

        // pause booster
        if (holdJumpBooster) holdJumpBooster.enabled = false;

        isClimbing = true;
        wantJump = false;
    }

    private void ExitClimb(bool withImpulse)
    {
        // outward direction from rope to player (XZ)
        Vector3 outwardXZ = (currentSnapXZ - ropeAxisXZ);
        if (outwardXZ.sqrMagnitude < 1e-6f) outwardXZ = Vector3.right;
        outwardXZ.y = 0f; outwardXZ.Normalize();

        // restore physics first
        rb.isKinematic = savedIsKinematic;
        rb.useGravity  = savedUseGravity;
        rb.constraints = savedConstraints;

        // resume booster
        if (holdJumpBooster) holdJumpBooster.enabled = true;

        if (withImpulse)
        {
            SafeZeroVelocity();                         // <<< now dynamic, safe to zero
            transform.position += outwardXZ * outwardNudge;

            Vector3 dir = transform.TransformDirection(jumpDirectionLocal.normalized);
            rb.AddForce(dir * jumpOffForce, ForceMode.VelocityChange);

            StartCoroutine(TemporarilyIgnoreRope(postJumpNoCollideTime));
        }

        isClimbing = false;
        wantJump = false;
        // keep currentRope for coroutine to finish ignore toggles
    }

    // ---- helpers ----

    private void SafeZeroVelocity()
    {
#if UNITY_6000_0_OR_NEWER
        if (!rb.isKinematic) rb.linearVelocity = Vector3.zero;
#else
        if (!rb.isKinematic) rb.velocity = Vector3.zero;
#endif
    }

    private IEnumerator TemporarilyIgnoreRope(float seconds)
    {
        if (currentRope == null || seconds <= 0f) { currentRope = null; yield break; }

        ropeCols.Clear();
        currentRope.GetComponentsInChildren(true, ropeCols);
        if (ropeCols.Count == 0) { currentRope = null; yield break; }

        playerCols.Clear();
        GetComponentsInChildren(true, playerCols);

        foreach (var pc in playerCols)
            if (pc && pc.enabled)
                foreach (var rc in ropeCols)
                    if (rc && rc.enabled)
                        Physics.IgnoreCollision(pc, rc, true);

        float t0 = Time.unscaledTime;
        while (Time.unscaledTime - t0 < seconds) yield return null;

        foreach (var pc in playerCols)
            if (pc)
                foreach (var rc in ropeCols)
                    if (rc)
                        Physics.IgnoreCollision(pc, rc, false);

        currentRope = null; // fully detached
    }
}
