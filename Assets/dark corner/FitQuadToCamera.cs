using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FitQuadToCamera : MonoBehaviour
{
    public Camera cam;
    public float distance = 2f;

    void Start()
    {
        if (!cam) cam = Camera.main;

        // Parent to the camera and place it in front
        transform.SetParent(cam.transform, false);
        transform.localPosition = new Vector3(0, 0, distance);
        transform.localRotation = Quaternion.identity;

        // Compute the camera frustum width and height at this distance
        float h = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float w = h * cam.aspect;

        // set quad scale to the target width/height to fill the screen
        transform.localScale = new Vector3(w, h, 1f);
    }
}


