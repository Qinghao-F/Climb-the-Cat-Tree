using UnityEngine;

/// <summary>
/// Marker component to indicate a climbable rope.
/// Attach this to the rope root object that also has a trigger collider.
/// No behaviour here; the player script detects this marker at runtime.
/// </summary>
[DisallowMultipleComponent]
public class RopeMarker : MonoBehaviour
{
    // Intentionally empty – serves purely as a semantic marker.
}
