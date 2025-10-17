using UnityEngine;

/// <summary>
/// Billboard: keeps the sprite facing the camera so it looks correct in 3D.
/// </summary>
public class FaceCamera : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;
        // Face the camera's forward (works for perspective & orthographic)
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }
}
