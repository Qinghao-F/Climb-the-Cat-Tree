using UnityEngine;

/// <summary>
/// Camera that follows the player vertically but never goes below the rising cat.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform target;
    
    [Header("Follow Settings")] // How smoothly camera follows (lower = smoother, 0 = instant)
    [SerializeField] private float smoothSpeed = 5f;
    
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -10); // offset from player position
    
    [Header("Starting Height")]
    [SerializeField] private float startingHeightBonus = 10f;

    [Header("Vertical Bounds")] // How far above the cat the camera can go at minimum
    [SerializeField] private float minHeightAboveCat = 5f;
    // to lock the camera's X and Z position:
    [SerializeField] private bool lockX = true; 
    [SerializeField] private bool lockZ = true;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
        
        // Auto-find player if not assigned
        if (target == null && PlayerController.Instance != null)
        {
            target = PlayerController.Instance.transform;
        }
        
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + startingHeightBonus,
            transform.position.z
        );
        initialPosition = transform.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // Apply axis locks
        if (lockX) desiredPosition.x = initialPosition.x;
        if (lockZ) desiredPosition.z = initialPosition.z;

        // Apply cat lower bound
        if (RisingCat.Instance != null)
        {
            float catY = RisingCat.Instance.GetCurrentY();
            float minY = catY + minHeightAboveCat;
            
            // Camera can never go below this threshold
            if (desiredPosition.y < minY)
            {
                desiredPosition.y = minY;
            }
        }

        if (smoothSpeed > 0f)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = desiredPosition;
        }
    }

}