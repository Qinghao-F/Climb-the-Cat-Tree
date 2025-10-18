using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rope Climbing Controller (Symmetric Final Version)
/// Supports left/right symmetry, short entry-facing hold, and full recovery after climbing.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerRopeClimb : MonoBehaviour
{
    // === Basic climbing and jump settings ===
    public float climbSpeed = 2.8f;                  // Vertical climbing speed
    public float extraSidePadding = 0.06f;           // Offset from rope centre
    public float sideBlendSpeed = 12f;               // Side smoothing
    public float sideSwitchDuration = 0.15f;         // Time to cross rope
    public float ignoreCollisionTime = 0.20f;        // Collision ignore during cross
    public float jumpOffForce = 6f;                  // Jump impulse
    public Vector3 jumpDirectionLocal = new Vector3(0f, 1f, 0.75f);
    public float outwardNudge = 0.12f;               // Push away from rope
    public float postJumpNoCollideTime = 0.25f;      // Ignore collisions after jump

    // === Facing mode control ===
    public enum ClimbFacingMode { Centre, SideOut }
    public ClimbFacingMode faceModeWhileClimbing = ClimbFacingMode.Centre; // Face rope or outward
    public bool spriteFacesRightByDefault = true;
    public float entryFacingHoldTime = 0.12f;        // Preserve entry-facing
    public Behaviour holdJumpBooster;                // Optional external jump controller

    // === Internal states ===
    Rigidbody rb;
    bool isClimbing, wantJump;
    Transform currentRope;
    Collider ropeMainCol;
    Vector3 ropeAxisWorld;
    float safeSideDistance;
    int gripSide = +1;                               // +1=right, -1=left
    bool isSwitching;
    float switchT;
    int targetSide;
    Vector3 currentXZ, desiredXZ, crossStartPos, crossEndPos;

    private Animator anim;

    bool savedUseGravity, savedIsKinematic;
    RigidbodyConstraints savedConstraints;
    SpriteRenderer sr;
    bool hadSpriteAtEntry;
    bool savedFlipX;

    bool facingHoldActive;
    float facingHoldEndTime;

    readonly List<Collider> playerCols = new();
    readonly List<Collider> ropeCols = new();

    // === Initial setup ===
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sr = GetComponentInChildren<SpriteRenderer>();

        anim = GetComponentInChildren<Animator>();

        playerCols.AddRange(GetComponentsInChildren<Collider>(true));
    }

    void Start()
    {
        if (holdJumpBooster == null)
        {
            var maybe = GetComponent("HoldJumpBooster") as Behaviour;
            if (maybe != null) holdJumpBooster = maybe;
        }
    }

    // === Detect rope contact and exit ===
    void OnTriggerEnter(Collider other) => TryBegin(other);
    void OnTriggerStay(Collider other) { if (!isClimbing) TryBegin(other); }
    void OnTriggerExit(Collider other)
    {
        if (!isClimbing || currentRope == null) return;
        if (other.transform == currentRope || other.transform.IsChildOf(currentRope))
            ExitClimb(false);
    }

    // === Handle input during climbing ===
    void Update()
    {
        if (!isClimbing) return;

        if (Input.GetKeyDown(KeyCode.Space)) wantJump = true;

        // Horizontal keys decide rope side
        float h = Input.GetAxisRaw("Horizontal");
        int desiredSide = (h > 0.1f) ? +1 : (h < -0.1f ? -1 : 0);

        if (desiredSide != 0 && !isSwitching && desiredSide != gripSide)
        {
            facingHoldActive = false;
            BeginSideSwitch(desiredSide);
        }
    }

    // === Physics and vertical movement ===
    void FixedUpdate()
    {
        if (!isClimbing) return;

        // Find rope axis by closest point
        if (ropeMainCol)
        {
            Vector3 closest = ropeMainCol.ClosestPoint(transform.position);
            ropeAxisWorld = new Vector3(closest.x, currentRope.position.y, closest.z);
        }

        Vector3 rightXZ = RopeRightXZ(currentRope);
        Vector3 axisXZ = new Vector3(ropeAxisWorld.x, 0f, ropeAxisWorld.z);
        desiredXZ = axisXZ + rightXZ * (safeSideDistance * gripSide);

        // Move to side or cross rope
        if (isSwitching)
        {
            switchT += Time.fixedDeltaTime / Mathf.Max(0.01f, sideSwitchDuration);
            float t = Mathf.SmoothStep(0f, 1f, switchT);
            transform.position = Vector3.Lerp(crossStartPos, crossEndPos, t);
            if (switchT >= 1f) { isSwitching = false; gripSide = targetSide; }
        }
        else
        {
            currentXZ = Vector3.Lerp(currentXZ, desiredXZ, 1f - Mathf.Exp(-sideBlendSpeed * Time.fixedDeltaTime));
            StickToXZ(currentXZ);
        }

        // Climb up or down
        float v = Input.GetAxis("Vertical");
        if (Mathf.Abs(v) > 0.001f)
            transform.position += Vector3.up * (v * climbSpeed * Time.fixedDeltaTime);
        
        // --- Control climbing animation speed ---
        if (anim)
        {
            if (Mathf.Abs(v) > 0.001f)
            {
                // Player is moving — play animation normally
                anim.speed = 1f;
            }
            else
            {
                // Player is stationary — freeze animation on current frame
                anim.speed = 0f;
            }
        }


        // Jump or flip after facing hold expires
        if (wantJump) ExitClimb(true);
        if (Time.time >= facingHoldEndTime) facingHoldActive = false;

        // Always face rope even while switching
        FaceWhileClimbing();

    }

    // === Enter climbing mode when touching rope ===
    void TryBegin(Collider other)
    {
        var marker = other.GetComponentInParent<RopeMarker>();
        if (marker == null || isClimbing) return;

        currentRope = marker.transform;
        ropeMainCol = other ? other : currentRope.GetComponentInChildren<Collider>();

        // Disable gravity for controlled climbing
        savedUseGravity = rb.useGravity;
        savedIsKinematic = rb.isKinematic;
        savedConstraints = rb.constraints;
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        SafeZeroVelocity();

        // Find contact axis and safe offset
        Vector3 closest = ropeMainCol ? ropeMainCol.ClosestPoint(transform.position) : currentRope.position;
        ropeAxisWorld = new Vector3(closest.x, currentRope.position.y, closest.z);
        float approxRadius = ropeMainCol ? Mathf.Max(ropeMainCol.bounds.extents.x, ropeMainCol.bounds.extents.z) : 0.05f;
        safeSideDistance = approxRadius + extraSidePadding;

        // Determine which side player touched
        Vector3 rightXZ = RopeRightXZ(currentRope);
        Vector3 axisXZ = new Vector3(ropeAxisWorld.x, 0f, ropeAxisWorld.z);
        Vector3 toPlayerXZ = new Vector3(transform.position.x - axisXZ.x, 0f, transform.position.z - axisXZ.z);
        float dot = Vector3.Dot(toPlayerXZ, rightXZ);
        if (Mathf.Abs(dot) < 1e-4f) dot = (transform.position.x - axisXZ.x) >= 0f ? +1f : -1f;
        gripSide = (dot >= 0f) ? +1 : -1;

        // Snap to correct side on contact
        Vector3 snapXZ = axisXZ + rightXZ * (safeSideDistance * gripSide);
        currentXZ = snapXZ;
        transform.position = new Vector3(snapXZ.x, transform.position.y, snapXZ.z);

        // Save facing before rope, activate hold window
        hadSpriteAtEntry = sr != null;
        if (hadSpriteAtEntry) savedFlipX = sr.flipX;
        facingHoldActive = entryFacingHoldTime > 0f;
        facingHoldEndTime = Time.time + entryFacingHoldTime;

        if (holdJumpBooster) holdJumpBooster.enabled = false;

        isClimbing = true;
        wantJump = false;
        isSwitching = false;

        if (anim) anim.SetBool("isClimbing", true);

    }

    // === Exit climbing mode ===
    void ExitClimb(bool withImpulse)
    {
        Vector3 outwardXZ = (new Vector3(transform.position.x, 0f, transform.position.z)
            - new Vector3(ropeAxisWorld.x, 0f, ropeAxisWorld.z)).normalized;

        rb.isKinematic = savedIsKinematic;
        rb.useGravity = savedUseGravity;
        rb.constraints = savedConstraints;

        if (holdJumpBooster) holdJumpBooster.enabled = true;

        // Apply jump when leaving rope
        if (withImpulse)
        {
            SafeZeroVelocity();
            transform.position += outwardXZ * outwardNudge;
            Vector3 dir = transform.TransformDirection(jumpDirectionLocal.normalized);
            rb.AddForce(dir * jumpOffForce, ForceMode.VelocityChange);
            StartCoroutine(TemporarilyIgnoreRope(postJumpNoCollideTime));
        }

        // Restore facing and reset state
        if (hadSpriteAtEntry && sr != null) sr.flipX = savedFlipX;
        isClimbing = false;
        wantJump = false;
        isSwitching = false;
        facingHoldActive = false;

        if (anim) anim.SetBool("isClimbing", false);
        if (anim) anim.speed = 1f; // restore normal playback speed


    }

    // === Switch sides around rope ===
    void BeginSideSwitch(int desiredSide)
    {
        Vector3 rightXZ = RopeRightXZ(currentRope);
        Vector3 axisXZ = new Vector3(ropeAxisWorld.x, 0f, ropeAxisWorld.z);
        Vector3 endXZ = axisXZ + rightXZ * (safeSideDistance * desiredSide);

        crossStartPos = transform.position;
        crossEndPos = new Vector3(endXZ.x, transform.position.y, endXZ.z);

        isSwitching = true;
        switchT = 0f;
        targetSide = desiredSide;
        facingHoldActive = false;

        StartCoroutine(TemporarilyIgnoreRope(ignoreCollisionTime));
        gripSide = desiredSide;
        FaceWhileClimbing(); // instantly face the rope
    }

    // === Maintain side position ===
    void StickToXZ(Vector3 xz)
    {
        Vector3 p = transform.position;
        transform.position = new Vector3(xz.x, p.y, xz.z);
    }

    // === Handle facing direction while on rope ===
    void FaceWhileClimbing()
    {
        if (!isClimbing || sr == null || currentRope == null) return;

        // Always face toward the rope's x-position
        bool wantFaceRight = (ropeAxisWorld.x > transform.position.x);

        // Adjust if your sprite faces left by default
        if (!spriteFacesRightByDefault)
            wantFaceRight = !wantFaceRight;

        sr.flipX = !wantFaceRight;
    }

    // === Utility functions ===
    static Vector3 RopeRightXZ(Transform rope)
    {
        Vector3 r = rope ? rope.right : Vector3.right;
        Vector3 xz = new Vector3(r.x, 0f, r.z);
        return (xz.sqrMagnitude < 1e-6f) ? Vector3.right : xz.normalized;
    }

    void SafeZeroVelocity()
    {
#if UNITY_6000_0_OR_NEWER
        if (!rb.isKinematic) rb.linearVelocity = Vector3.zero;
#else
        if (!rb.isKinematic) rb.velocity = Vector3.zero;
#endif
    }

    IEnumerator TemporarilyIgnoreRope(float seconds)
    {
        if (currentRope == null || seconds <= 0f) yield break;
        ropeCols.Clear();
        currentRope.GetComponentsInChildren(true, ropeCols);
        if (ropeCols.Count == 0) yield break;

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
