using UnityEngine;

/// <summary>
/// Jump booster add-on:
/// When the player stands on a platform with LongPressBoostPlatform,
/// holding the jump key shortly after leaving the ground extends the jump height.
/// Does not replace normal jumping; only enhances the upward phase.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class HoldJumpBooster : MonoBehaviour
{
    [Header("Ground Check")]
    [Tooltip("Reference point used for checking ground contact.")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Jump Boost")]
    [Tooltip("Normal jump force from the main controller.")]
    public float normalJumpForce = 60f;

    [Tooltip("Multiplier for upward speed while holding jump.")]
    public float targetSpeedMultiplier = 1.55f;

    [Tooltip("Boost duration after leaving the ground.")]
    public float maxHoldTime = 0.35f;

    [Tooltip("Grace period before leaving the ground.")]
    public float preLeaveCoyoteTime = 0.03f;

    [Header("Debug")]
    public bool showHUD = false;

    // --- Runtime state ---
    private Rigidbody rb;
    private bool isGrounded;
    private bool wasGrounded;
    private bool standingOnBoostPlatform;
    private bool boostWindowActive;
    private float holdTimeLeft;
    private float timeSinceLeftGround;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Check if grounded
        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(
            groundCheck.position, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);

        // Detect if standing on a boost platform
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

        // Arm the boost window while still grounded on a valid platform
        if (standingOnBoostPlatform)
        {
            if (isGrounded)
            {
                boostWindowActive = true;
                holdTimeLeft = maxHoldTime + preLeaveCoyoteTime;
                timeSinceLeftGround = 0f;
            }
        }

        // Handle airborne boost logic
        if (!isGrounded)
        {
            timeSinceLeftGround += Time.deltaTime;

            // Maintain higher upward speed while holding jump
            if (boostWindowActive && holdTimeLeft > 0f && Input.GetKey(jumpKey))
            {
                var v = rb.linearVelocity;
                float targetVy = normalJumpForce * targetSpeedMultiplier;

                v.y = Mathf.Max(v.y, targetVy);
                rb.linearVelocity = v;

                holdTimeLeft -= Time.deltaTime;
            }

            // End conditions for the boost window
            if (holdTimeLeft <= 0f || Input.GetKeyUp(jumpKey))
            {
                boostWindowActive = false;
            }
        }

        // Reset when landing
        if (isGrounded && !wasGrounded)
        {
            boostWindowActive = false;
            holdTimeLeft = 0f;
            timeSinceLeftGround = 0f;
        }
    }

    // Optional on-screen display for debugging
    void OnGUI()
    {
        if (!showHUD) return;
        GUI.Label(new Rect(10, 10, 380, 20), $"grounded={isGrounded}  onBoost={standingOnBoostPlatform}");
        GUI.Label(new Rect(10, 28, 380, 20), $"active={boostWindowActive}  left={holdTimeLeft:F2}  vy={rb.linearVelocity.y:F1}");
    }
}

