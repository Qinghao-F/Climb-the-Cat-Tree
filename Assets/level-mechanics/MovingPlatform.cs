using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Moving Platform Settings")]
    public float speed = 10f;
    public float waitTime = 1f; // time spent waiting at each endpoint
    public float bound = 10f; // distance between point A and B
    
    [Header("Movement Axis")]
    public Vector3 movementDirection = Vector3.right; // Default moves on x-axis

    private Vector3 pointA; // start point  
    private Vector3 pointB; // end point
    private bool waiting = false;
    private bool goingToPointB = true;
    private float waitTimer = 0f;

    void Start()
    {
        pointA = transform.position;
        pointB = pointA + (movementDirection.normalized * bound);
        
        if (Vector3.Distance(pointA, pointB) < 0.01f)
        {
            Debug.LogError("MovingPlatform: Bound is too small on " + gameObject.name);
            enabled = false; // Disable this script
            return;
        }
        
        Debug.Log($"Platform '{gameObject.name}' will move between {pointA} and {pointB}");
    }

    void Update()
    {
        // If waiting, countdown timer
        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
            }
            return; // Don't move while waiting
        }
        
        // Move toward current target
        Vector3 targetPosition = goingToPointB ? pointB : pointA;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        
        // Have we reached the destination?
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            // Snap to exact position
            transform.position = targetPosition;
            
            // Start waiting and switch direction
            waiting = true;
            waitTimer = waitTime;
            goingToPointB = !goingToPointB;
        }
    }

    // Parent player when they step on platform
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(transform);
        }
    }

    // Unparent player when they leave platform
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }
}