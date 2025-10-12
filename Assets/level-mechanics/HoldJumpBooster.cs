using UnityEngine;

/// <summary>
/// Player add-on:
/// While the player is STANDING on a platform that has LongPressBoostPlatform,
/// leaving the ground will arm a short "hold-to-boost" window. During this window,
/// if the jump key is held, the script maintains a higher upward speed so the jump
/// is visibly higher. This does NOT replace your ground jump (your controller still
/// performs take-off); it only augments the ascent when holding the key.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class HoldJumpBooster : MonoBehaviour
{
    [Header("Ground check (use the SAME point/layer as your controller)")]
    public Transform groundCheck;           // Your existing GroundCheck point
    public float groundCheckRadius = 0.2f;  // Small, precise probe
    public LayerMask groundLayer;           // Set to your 'ground' layer

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Tuning")]
    [Tooltip("Match your controller's normal jump force (e.g., 60).")]
    public float normalJumpForce = 60f;

    [Tooltip("Target upward speed multiplier while the key is held during the boost window."
           + " Theoretical 2× height ≈ ×1.414; with heavy extra gravity in your controller,"
           + " start around 1.5–1.6 and tweak.")]
    public float targetSpeedMultiplier = 1.55f;

    [Tooltip("Duration of the boost window AFTER you leave the ground.")]
    public float maxHoldTime = 0.35f;

    [Tooltip("Extra leeway: allow the window to start a tiny bit BEFORE the exact unground frame.")]
    public float preLeaveCoyoteTime = 0.03f;

    [Header("Debug")]
    public bool showHUD = false;

    // --- Runtime ---
    private Rigidbody rb;
    private bool isGrounded;
    private bool wasGrounded;
    private bool standingOnBoostPlatform;      // true if any overlapped ground has LongPressBoostPlatform
    private bool boostWindowActive;            // active only for this jump
    private float holdTimeLeft;                // remaining time to sustain higher speed
    private float timeSinceLeftGround;         // used for robustness

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 1) Grounded state
        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(
            groundCheck.position, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);

        // 2) Are we currently standing on a "marked" platform?
        standingOnBoostPlatform = false;
        var hits = Physics.OverlapSphere(
            groundCheck.position, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].GetComponentInParent<LongPressBoostPlatform>() != null)
            {
                standingOnBoostPlatform = true;
                break;
            }
        }

        // 3) Arm the window when we are (or are about to be) leaving the ground FROM a marked platform.
        //    This avoids any order-of-execution issues with GetKeyDown in other scripts.
        if (standingOnBoostPlatform)
        {
            // Start / refresh the window while still grounded (pre-leave coyote time)
            if (isGrounded)
            {
                boostWindowActive = true;
                holdTimeLeft = maxHoldTime + preLeaveCoyoteTime;
                timeSinceLeftGround = 0f;
            }
        }

        // 4) Track airborne time and close the window when finished
        if (!isGrounded)
        {
            timeSinceLeftGround += Time.deltaTime;

            // While airborne, if the window is active and the key is held, maintain higher upward speed
            if (boostWindowActive && holdTimeLeft > 0f && Input.GetKey(jumpKey))
            {
                var v = rb.linearVelocity; // Unity 6+: linearVelocity (replaces velocity)
                float targetVy = normalJumpForce * targetSpeedMultiplier;

                // Only ever lift upwards; never pull velocity down.
                v.y = Mathf.Max(v.y, targetVy);
                rb.linearVelocity = v;

                holdTimeLeft -= Time.deltaTime;
            }

            // Stop conditions
            if (holdTimeLeft <= 0f || Input.GetKeyUp(jumpKey))
            {
                boostWindowActive = false;
            }
        }

        // 5) Landing fully resets
        if (isGrounded && !wasGrounded)
        {
            boostWindowActive = false;
            holdTimeLeft = 0f;
            timeSinceLeftGround = 0f;
        }
    }

    // Tiny on-screen HUD (optional)
    void OnGUI()
    {
        if (!showHUD) return;
        GUI.Label(new Rect(10, 10, 380, 20), $"grounded={isGrounded}  onBoost={standingOnBoostPlatform}");
        GUI.Label(new Rect(10, 28, 380, 20), $"active={boostWindowActive}  left={holdTimeLeft:F2}  vy={rb.linearVelocity.y:F1}");
    }
}
