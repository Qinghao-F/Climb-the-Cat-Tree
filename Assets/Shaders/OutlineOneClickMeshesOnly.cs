#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// One-click cartoon outline for static meshes only (MeshRenderer).
/// Skips animated characters (SkinnedMeshRenderer).
/// </summary>
public static class OutlineOneClickMeshesOnly
{
    // --- Default outline look ---
    const string kShaderPath     = "Toon/OutlineOnly";
    static readonly Color kColor = new Color(0.10f, 0.10f, 0.10f, 1f);
    const float kWidthPx         = 2.8f;   // <- was 2.0f
    const float kZOffset         = 0.12f;  // <- was 0.10f

    // --- Optional skip rules ---
    const string kSkipChildName  = "__Outline"; // generated child name
    const string kSkipTag        = "Player";    // do not touch player; edit if needed

    [MenuItem("Tools/Toon Outline (Meshes Only)/Add To Scene")]
    public static void AddAllInScene() => ProcessAll(add:true);

    [MenuItem("Tools/Toon Outline (Meshes Only)/Remove From Scene")]
    public static void RemoveAllInScene() => ProcessAll(add:false);

    [MenuItem("Tools/Toon Outline (Meshes Only)/Add To Selection")]
    public static void AddSelection() => ProcessSelection(add:true);

    [MenuItem("Tools/Toon Outline (Meshes Only)/Remove From Selection")]
    public static void RemoveSelection() => ProcessSelection(add:false);

    // ---- Core routines ----

    static void ProcessAll(bool add)
    {
        int count = 0;
        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in all)
            count += HandleGameObject(t.gameObject, add) ? 1 : 0;

        EditorUtility.DisplayDialog("Toon Outline",
            $"{(add ? "Added" : "Removed")} outline for {count} object(s).", "OK");
    }

    static void ProcessSelection(bool add)
    {
        var sel = Selection.gameObjects;
        if (sel == null || sel.Length == 0)
        {
            EditorUtility.DisplayDialog("Toon Outline", "Nothing selected.", "OK");
            return;
        }

        int count = 0;
        var stack = new Stack<Transform>();
        foreach (var go in sel) stack.Push(go.transform);

        while (stack.Count > 0)
        {
            var t = stack.Pop();
            if (t.name != kSkipChildName) // never re-process the generated child
                count += HandleGameObject(t.gameObject, add) ? 1 : 0;

            for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
        }

        EditorUtility.DisplayDialog("Toon Outline",
            $"{(add ? "Added" : "Removed")} outline for {count} object(s).", "OK");
    }

    // Returns true if the object was modified
    static bool HandleGameObject(GameObject go, bool add)
    {
        // Skip explicit tags and animated characters
        if (!string.IsNullOrEmpty(kSkipTag) && go.CompareTag(kSkipTag)) return false;
        if (go.GetComponent<SkinnedMeshRenderer>()) return false;

        var mr = go.GetComponent<MeshRenderer>();
        var mf = go.GetComponent<MeshFilter>();
        if (!mr || !mf || !mf.sharedMesh) return false;

        var existing = go.transform.Find(kSkipChildName);

        if (add)
        {
            if (existing) return false; // already outlined

            // Create outline child
            var childGO = new GameObject(kSkipChildName);
            Undo.RegisterCreatedObjectUndo(childGO, "Create Outline");
            childGO.transform.SetParent(go.transform, false);

            var childMF = childGO.AddComponent<MeshFilter>();
            childMF.sharedMesh = mf.sharedMesh;

            var childMR = childGO.AddComponent<MeshRenderer>();
            childMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            childMR.receiveShadows = false;
            childMR.allowOcclusionWhenDynamic = false;

            // Build material
            var sh = Shader.Find(kShaderPath);
            if (!sh) { Debug.LogError($"Outline shader not found: {kShaderPath}"); return false; }
            var mat = new Material(sh);
            mat.SetColor("_OutlineColor", kColor);
            mat.SetFloat("_OutlineWidth", kWidthPx);
            mat.SetFloat("_ZOffset", kZOffset);
            childMR.sharedMaterial = mat;

            return true;
        }
        else
        {
            if (!existing) return false;
            Undo.DestroyObjectImmediate(existing.gameObject);
            return true;
        }
    }
}
#endif
