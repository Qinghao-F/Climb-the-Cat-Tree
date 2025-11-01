using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Moving Platform Settings")]
    public float speed = 10f;
    public float waitTime = 1f; // time spent waiting at each endpoint
    public float bound = 10f; // distance between point A and B
    
    [Header("Movement Axis")]
    // NOTE: Setting this to Vector3.up (0, 1, 0) gives vertical movement.
    // Setting this to Vector3.right + Vector3.up (1, 1, 0) gives diagonal movement.
    public Vector3 movementDirection = Vector3.right; // Default moves on x-axis

    private Vector3 pointA; // start point  
    private Vector3 pointB; // end point
    private bool waiting = false;
    private bool goingToPointB = true;
    private float waitTimer = 0f;
    
    private Rigidbody rb;
    private Transform playerTransform;
    private Rigidbody playerRb;
    private Vector3 lastPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Configure Rigidbody for kinematic movement
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        if (movementDirection.sqrMagnitude < 0.001f)
        {
            Debug.LogError("MovingPlatform: movementDirection is zero on " + gameObject.name + ". Platform disabled.");
            enabled = false;
            return;
        }

        // Calculate the two endpoints (A and B)
        pointA = transform.position;
        pointB = pointA + (movementDirection.normalized * bound);
        
        if (Vector3.Distance(pointA, pointB) < 0.01f)
        {
            Debug.LogError("MovingPlatform: Bound is too small on " + gameObject.name);
            enabled = false;
            return;
        }
        
        lastPosition = transform.position;
        
        Debug.Log($"Platform '{gameObject.name}' will move between {pointA} and {pointB}");
    }

    void Update()
    {
        // Handle wait timer in Update
        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (waiting)
        {
            lastPosition = transform.position;
            return;
        }
        
        // Calculate movement
        Vector3 targetPosition = goingToPointB ? pointB : pointA;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.fixedDeltaTime);
        
        // Calculate platform delta before moving
        Vector3 platformDelta = newPosition - lastPosition;
        
        // Move using Rigidbody for proper physics integration
        rb.MovePosition(newPosition);
        
        // Move player manually if on platform
        if (playerTransform != null && playerRb != null)
        {
            // Move the player's Rigidbody to maintain physics
            Vector3 newPlayerPos = playerTransform.position + platformDelta;
            playerRb.MovePosition(newPlayerPos);
        }
        
        lastPosition = newPosition;
        
        // Check if reached destination
        if (Vector3.Distance(newPosition, targetPosition) < 0.01f)
        {
            rb.MovePosition(targetPosition);
            lastPosition = targetPosition;
            
            waiting = true;
            waitTimer = waitTime;
            goingToPointB = !goingToPointB;
        }
    }

    // Use OnCollisionStay to continuously detect player standing on platform
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Check if player is on top of platform (not hitting from side/bottom)
            bool isOnTop = false;
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f) // Normal pointing down = player is on top
                {
                    isOnTop = true;
                    break;
                }
            }

            if (isOnTop)
            {
                playerTransform = collision.transform;
                playerRb = collision.rigidbody;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playerTransform == collision.transform)
            {
                playerTransform = null;
                playerRb = null;
            }
        }
    }

}