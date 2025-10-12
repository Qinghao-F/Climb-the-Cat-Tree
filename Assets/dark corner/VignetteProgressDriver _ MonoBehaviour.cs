using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class VignetteProgressDriver : MonoBehaviour
{
    public float durationSec = 600f; // Time here!
    public bool  easeIn = false;     // easing?
    public float gamma = 1.5f;       // Easing intensity (>1: slow start, fast end)
    public string propertyName = "_Progress";

    Renderer rend;
    Material mat; // Instanced material

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mat  = rend.material;
    }

    void Update()
    {
        float t = Mathf.Clamp01(Time.time / Mathf.Max(0.01f, durationSec));
        if (easeIn) t = Mathf.Pow(t, gamma);
        if (mat && mat.HasProperty(propertyName))
            mat.SetFloat(propertyName, t);
    }
}
