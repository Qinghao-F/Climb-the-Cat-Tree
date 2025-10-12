using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;  
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -5);

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // Smoothly move camera towards target
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}*/

public class CameraFollowVertical : MonoBehaviour
{
    [SerializeField] private Transform target;  // Drag your player here
    [SerializeField] private float smoothTime = 0.2f; // How smoothly the camera follows
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f); // Position offset

    private Vector3 velocity = Vector3.zero;
    private float baseY; // The lowest point the camera starts at

    void Start()
    {
        if (target != null)
            baseY = transform.position.y; // Optional: prevent camera from going below starting point
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Target position only moves up and down
        Vector3 targetPosition = new Vector3(
            transform.position.x,                // Lock X
            target.position.y + offset.y,        // Follow Y
            transform.position.z                 // Lock Z (or use offset.z if desired)
        );

        // Prevent camera from going below starting Y position (optional)
        targetPosition.y = Mathf.Max(targetPosition.y, baseY);

        // Smoothly move camera to new Y position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}


