using UnityEngine;

/// <summary>
/// Moves the yarn ball horizontally across the screen and cleans it up
/// once it is safely outside the camera view. Uses no physics forces,
/// ensuring it can ghost through level geometry.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class YarnBallMover : MonoBehaviour
{
    [Tooltip("Units per second along +X (right) or -X (left).")]
    public float speed = 2.5f;

    [Tooltip("How far beyond the screen (in normalised viewport units) we allow before despawn. 0.1 = 10% of screen width.")]
    public float viewportMargin = 0.12f;

    [Tooltip("Z position (depth) the ball should stick to so it matches the player's 2.5D plane.")]
    public float zPlane = 0f;

    public Transform zPlaneAnchor; // drag Player here if you like

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;

        // Automatically detect the player if not manually specified
        if (zPlaneAnchor == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                zPlaneAnchor = playerObj.transform;
        }

        // Collider/Rigidbody setup
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    private void Update()
    {
        // Constant horizontal drift
        transform.position += Vector3.right * speed * Time.deltaTime;

        // Lock to chosen Z plane (from anchor if provided)
        float z = zPlaneAnchor ? zPlaneAnchor.position.z : zPlane;
        var p = transform.position;
        p.z = z;
        transform.position = p;

        // Despawn when fully off-screen (+margin)
        if (cam != null)
        {
            Vector3 vp = cam.WorldToViewportPoint(transform.position);
            if (vp.x < -viewportMargin || vp.x > 1f + viewportMargin)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Helper for spawner to set direction neatly.
    /// Positive speed shoots right; negative shoots left.
    /// </summary>
    public void SetDirection(bool toRight)
    {
        speed = Mathf.Abs(speed) * (toRight ? 1f : -1f);
    }
}
