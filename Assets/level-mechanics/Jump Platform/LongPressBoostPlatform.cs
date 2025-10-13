using UnityEngine;

/// <summary>
/// Marker component for platforms that should grant “hold to jump higher”.
/// Attach this to the SOLID platform object (with a non-trigger collider).
/// No tag, no trigger, no code needed – merely being present marks the platform.
/// </summary>
public class LongPressBoostPlatform : MonoBehaviour
{
    // Intentionally empty – acts purely as a marker.
}
