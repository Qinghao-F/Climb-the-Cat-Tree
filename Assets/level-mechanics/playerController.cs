using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings:")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float jumpForce = 7f;

[Header("Ground Check Settings:")]
[SerializeField] private Transform groundCheckPoint;
[SerializeField] private float groundCheckRadius = 0.25f; // instead of distance
[SerializeField] private LayerMask whatIsGround;
[SerializeField] private float extraGravity = 20f;

    [Header("flip:")]
    [SerializeField] private Transform spriteTransform; // assign in inspector

    private Rigidbody rb;
    private float xAxis;
    private Animator anim;

    public static PlayerController Instance;

    private void Awake()
    {
        if(Instance != null && Instance != this)
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
        // Freeze Z so player stays in 2.5D lane
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        GetInputs();
        Move();
        Jump();
        Flip();
    }
    void FixedUpdate()
    {
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
        if (xAxis < 0)
        {
            spriteTransform.localScale = new Vector3(-Mathf.Abs(spriteTransform.localScale.x),
                                                    spriteTransform.localScale.y,
                                                    spriteTransform.localScale.z);
        }
        else if (xAxis > 0)
        {
            spriteTransform.localScale = new Vector3(Mathf.Abs(spriteTransform.localScale.x),
                                                    spriteTransform.localScale.y,
                                                    spriteTransform.localScale.z);
        }
    }

    void Move()
    {
        rb.linearVelocity = new Vector3(walkSpeed * xAxis, rb.linearVelocity.y, 0f);
        
        anim.SetBool("Walking", rb.linearVelocity.x != 0 && Grounded());
    }

    public bool Grounded()
    {
        // Draw sphere for debugging (optional, only in Editor)
        Debug.DrawRay(groundCheckPoint.position, Vector3.down * 0.01f, Color.green);
        
        return Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, whatIsGround);
    }

    void Jump()
    {
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            // Short hop if player releases jump early
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }

        if (Input.GetButtonDown("Jump") && Grounded())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }

        anim.SetBool("Jumping", !Grounded());
    }
}
