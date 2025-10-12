using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Controls directional light and post-processing blending based on height.
/// Produces a smooth warm-to-cool and bright-to-dim gradient effect across the scene.
/// </summary>
public class HeightBasedLightColor : MonoBehaviour
{
    // --- Basic References ---
    public Light dirLight;        // Main directional light
    public Transform target;      // Camera or player used to sample height

    // --- Height Range ---
    public float yMin = 0f;       // Minimum scene height
    public float yMax = 500f;     // Maximum scene height

    // --- Colour and Light Settings ---
    public Color bottomColor = new Color(1.00f, 0.90f, 0.70f); // Warm tone at bottom
    public Color topColor = new Color(0.70f, 0.80f, 1.00f);    // Cool tone at top
    public float bottomIntensity = 1.2f;                       // Bright intensity at bottom
    public float topIntensity = 0.8f;                          // Dim intensity at top

    // --- Response Curve ---
    [Range(0.2f, 2f)] public float intensityCurve = 1.1f;       // Controls transition contrast

    // --- Height Band Data Structure ---
    [System.Serializable]
    public class HeightBand
    {
        public Behaviour volumeComponent;  // Linked Post-Process Volume
        public float centerY = 0f;         // Centre height of this band
        public float halfFalloff = 50f;    // Half blending range
        [HideInInspector] public float weight; // Current calculated weight
    }

    // --- Post-Processing Bands ---
    public HeightBand bottomBand = new HeightBand { centerY = 80f, halfFalloff = 60f };
    public HeightBand midBand = new HeightBand { centerY = 250f, halfFalloff = 60f };
    public HeightBand topBand = new HeightBand { centerY = 420f, halfFalloff = 60f };

    // --- Debug Option ---
    public bool showDebugInInspector = true;

    // --- Auto-assign references on reset ---
    void Reset()
    {
        if (!dirLight)
        {
            var l = FindAnyObjectByType<Light>();
            if (l && l.type == LightType.Directional) dirLight = l;
        }
        if (!target && Camera.main) target = Camera.main.transform;
    }

    void Update()
    {
        if (!target) return;

        // --- Step 1: Calculate height factor (0 → 1 based on Y position) ---
        float t = Mathf.InverseLerp(yMin, yMax, target.position.y);
        t = Mathf.Clamp01(t);
        t = Mathf.Pow(t, Mathf.Max(0.2f, intensityCurve)); // Non-linear contrast control

        // --- Step 2: Apply light colour and intensity based on height ---
        if (dirLight)
        {
            dirLight.color = Color.Lerp(bottomColor, topColor, t);
            dirLight.intensity = Mathf.Lerp(bottomIntensity, topIntensity, t);
        }

        // --- Step 3: Calculate weight for each post-processing band ---
        float y = target.position.y;
        float wBottom = BandWeight(y, bottomBand.centerY, bottomBand.halfFalloff);
        float wMid = BandWeight(y, midBand.centerY, midBand.halfFalloff);
        float wTop = BandWeight(y, topBand.centerY, topBand.halfFalloff);

        // --- Step 4: Normalise band weights so total = 1 ---
        float sum = wBottom + wMid + wTop + 1e-5f;
        wBottom /= sum; wMid /= sum; wTop /= sum;

        // --- Step 5: Apply calculated weights to post-processing volumes ---
        ApplyWeight(bottomBand, wBottom);
        ApplyWeight(midBand, wMid);
        ApplyWeight(topBand, wTop);
    }

    // --- Calculates falloff weight for each band based on distance from its centre ---
    static float BandWeight(float y, float center, float halfFalloff)
    {
        float d = Mathf.Abs(y - center);
        float edge = Mathf.Max(1e-3f, halfFalloff);
        float x = Mathf.Clamp01(d / edge);
        float w = 1f - Mathf.SmoothStep(0f, 1f, x);
        w *= w; // Keeps centre stronger and edges smoother
        return w;
    }

    // --- Applies the computed weight to a Volume (URP or Built-in via reflection) ---
    static void ApplyWeight(HeightBand band, float w)
    {
        band.weight = w;
        if (!band.volumeComponent) return;

        var vol = band.volumeComponent as Volume;
        if (vol != null)
        {
            vol.weight = w;
            return;
        }

        var t = band.volumeComponent.GetType();
        var prop = t.GetProperty("weight");
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(band.volumeComponent, w, null);
        }
    }
}
