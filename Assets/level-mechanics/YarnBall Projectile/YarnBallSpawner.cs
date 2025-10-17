using UnityEngine;
using System.Collections;

/// <summary>
/// Fires a yarn ball from THIS transform's position at a fixed interval,
/// always moving in one chosen horizontal direction. No alternating, no random.
/// </summary>
public class YarnBallSpawner : MonoBehaviour
{
    [Header("Prefab & Timing")]
    public GameObject yarnBallPrefab;
    [Tooltip("Seconds between shots.")]
    public float interval = 2.0f;
    [Tooltip("Initial delay before the first shot.")]
    public float initialDelay = 0.5f;

    [Header("Direction & Plane")]
    [Tooltip("If true, balls travel to the right; otherwise to the left.")]
    public bool shootRight = true;
    [Tooltip("Z depth to lock the ball to (match the player's Z).")]
    public float zPlane = 0f;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void OnEnable()
    {
        StartCoroutine(LoopSpawn());
    }

    private IEnumerator LoopSpawn()
    {
        yield return new WaitForSeconds(initialDelay);

        while (enabled)
        {
            SpawnOnce();
            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnOnce()
    {
        if (yarnBallPrefab == null) return;

        // Spawn exactly at this anchor's position (single muzzle)
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, zPlane);
        var go = Instantiate(yarnBallPrefab, spawnPos, Quaternion.identity);

        // Configure mover
        var mover = go.GetComponent<YarnBallMover>();
        if (mover != null)
        {
            // Keep whatever speed you set on the prefab, only fix direction + z plane
            mover.SetDirection(shootRight);
            mover.zPlane = zPlane;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, 0.08f);
        // A short direction arrow so placement is obvious
        Vector3 dir = (shootRight ? Vector3.right : Vector3.left) * 0.6f;
        Gizmos.DrawLine(transform.position, transform.position + dir);
    }
#endif
}
