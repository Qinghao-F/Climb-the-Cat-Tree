// COMP30019 - Graphics and Interaction
// (c) University of Melbourne, 2022

using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class CustomMVP : MonoBehaviour
{
    [SerializeField] private new Camera camera;

    private Material _material;

    private void Start()
    {
        this._material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        // Let's construct MVP ourselves... for our sanity we'll still let Unity
        // do a bit of the heavy lifting (e.g. converting quaternions to matrix
        // rotations using Matrix4x4.TRS).

        // Extract M out of this object's transform, fairly straightforward.
        // Note "lossyScale" is the closest thing we have to a "world space"
        // scale (that takes parent transforms into account). In very select
        // cases this may not reflect the exact transform if there is a parent.
        var M = Matrix4x4.TRS(
            this.transform.position, 
            this.transform.rotation, 
            this.transform.lossyScale);

        // V is a bit trickier to get. Turns out Unity uses a right-handed
        // coordinate system for view coordinates internally (OpenGL style), so
        // we need to negate the z-axis. We can do this via the scale component,
        // as it isn't utilised by the camera (this is just a numerical trick).
        // 
        // Finally note, it's the *inverse* of the camera's transform in the
        // world we want, as the camera's transform takes us from camera space
        // to world space, but here we want world space to camera space.
        var V = Matrix4x4.TRS(
            this.camera.transform.position, 
            this.camera.transform.rotation, 
            new Vector3(1, 1, -1)).inverse;
        
        // This still isn't *perfect* though: we probably want to re-compute
        // this on a per-camera basis. For example, the scene view won't render
        // the object with the correct V matrix for its camera. If you enter
        // play mode, notice how the game view camera transform is also being
        // used in the scene view. Looks rather weird!
        
        // Now onto P...
        //
        // First, we can construct it via camera component properties and
        // another constructor method in Matrix4x4 called 'Perspective', which
        // generates a perspective projection matrix for us.
        var P = Matrix4x4.Perspective(
            this.camera.fieldOfView,
            this.camera.aspect,
            this.camera.nearClipPlane,
            this.camera.farClipPlane);
        
        // This only works in some cases though. Further complexity arises out
        // of the fact that Unity is cross platform, and again uses OpenGL
        // (right-handed) conventions for the internal projection matrix. The
        // GPU side will use certain handedness conventions for matrices,
        // separate to Unity. So, if DirectX is the underlying API on your
        // system, then we need to flip to left-handed conventions!
        //
        // Depending on the platform you were using when solving this question,
        // you might not have noticed this issue in the first place (you were
        // probably *not* using Windows).
        //
        // To save us the effort of checking the underlying graphics API, Unity
        // provides a method GL.GetGPUProjectionMatrix to normalise this for us.
        P = GL.GetGPUProjectionMatrix(P, true);

        // Remember, we might say MVP but really, we multiply right to left :)
        var MVP = P * V * M; 
        
        // Finally, make it available in shader.
        this._material.SetMatrix("_CustomMVP", MVP);
        
        // Phew, wouldn't you rather just use UNITY_MATRIX_MVP?! 
    }
}
