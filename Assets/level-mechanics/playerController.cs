
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings:")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float jumpForce = 7f;

    [Header("Ground Check Settings:")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float extraGravity = 20f;

    [Header("flip:")]
    [SerializeField] private Transform spriteTransform;

    [Header("Footstep SFX (Flip Trigger)")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private Vector2 pitchJitter = new Vector2(0.95f, 1.05f);
    [SerializeField] private float footstepVolume = 0.8f;

    private Rigidbody rb;
    private float xAxis;
    private Animator anim;
    private int facingDir = 1;
    private PlayerRopeClimb ropeClimb;

    public static PlayerController Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        anim = GetComponentInChildren<Animator>();
        ropeClimb = GetComponent<PlayerRopeClimb>();

        facingDir = (spriteTransform != null && spriteTransform.localScale.x < 0f) ? -1 : 1;
    }

    void Update()
    {
        if (ropeClimb != null && ropeClimb.IsClimbing)
        {
            // Only update vertical input if you want manual movement
            //ropeClimb.HandleVerticalInput();
            return; // Skip regular movement
        }

        GetInputs();
        Move();
        Jump();
        Flip();
    }


    void FixedUpdate()
    {
        // Skip gravity when climbing
        if (ropeClimb != null && ropeClimb.IsClimbing) return;

        if (!Grounded())
        {
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
    }

    void Flip()
    {
        if (xAxis < 0 && facingDir != -1)
        {
            spriteTransform.localScale = new Vector3(-Mathf.Abs(spriteTransform.localScale.x),
                                                    spriteTransform.localScale.y,
                                                    spriteTransform.localScale.z);
            facingDir = -1;
            TryPlayFlipFootstep();
        }
        else if (xAxis > 0 && facingDir != 1)
        {
            spriteTransform.localScale = new Vector3(Mathf.Abs(spriteTransform.localScale.x),
                                                    spriteTransform.localScale.y,
                                                    spriteTransform.localScale.z);
            facingDir = 1;
            TryPlayFlipFootstep();
        }
    }

    void Move()
    {
        rb.linearVelocity = new Vector3(walkSpeed * xAxis, rb.linearVelocity.y, 0f);
        anim.SetBool("Walking", rb.linearVelocity.x != 0 && Grounded());
    }

    public bool Grounded()
    {
        Debug.DrawRay(groundCheckPoint.position, Vector3.down * 0.01f, Color.green);
        return Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, whatIsGround);
    }

    void Jump()
    {
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }

        if (Input.GetButtonDown("Jump") && Grounded())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }

        anim.SetBool("Jumping", !Grounded());
    }

    void TryPlayFlipFootstep()
    {
        bool grounded = Grounded();
        bool movingHorizontally = Mathf.Abs(rb.linearVelocity.x) > 0.05f;

        if (!grounded || !movingHorizontally) return;

        PlayFootstepOneShot();
    }

    void PlayFootstepOneShot()
    {
        if (footstepSource == null || footstepClips == null || footstepClips.Length == 0) return;

        var clip = footstepClips[Random.Range(0, footstepClips.Length)];
        footstepSource.pitch = Random.Range(pitchJitter.x, pitchJitter.y);
        footstepSource.PlayOneShot(clip, footstepVolume);
    }
}