using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// فاز A / قدم A8 — نور Baked + پولیش سایه.
/// v6: منوی 28 = روشن‌کردن سایه‌های Directional + خورشید بلندتر + shadowDistance URP.
/// </summary>
public static class LightingSetup
{
    private static readonly string[] ScenePaths =
    {
        "Assets/_Project/Scenes/Interior_Room01.unity",
        "Assets/_Project/Scenes/Prototype_Block01.unity",
        "Assets/_Project/Scenes/Interior_MobileShop01.unity",
    };

    [MenuItem("TehranCity/Setup/25 Lighting + Probes (Bake Manually)")]
    public static void Run()
    {
        EditorSceneManager.SaveOpenScenes();
        foreach (var path in ScenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path);
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                light.lightmapBakeType = LightmapBakeType.Mixed;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.6f, 0.7f, 0.8f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.45f, 0.5f);
            RenderSettings.ambientGroundColor = new Color(0.25f, 0.22f, 0.20f);
            RenderSettings.ambientIntensity = 1f;
            EnsureLightProbes(scene.name);
            if (scene.name.Contains("Room01"))
                EnsureReflectionProbe();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log("[TehranCity] Lighting + probes ready. Use Setup 26 for per-scene bake.");
    }

    [MenuItem("TehranCity/Setup/26 Bake Each Scene (Single-Scene, Sync + Save)")]
    public static void BakeEach()
    {
        foreach (var p in ScenePaths)
        {
            var scene = EditorSceneManager.OpenScene(p);
            Debug.Log($"[TehranCity] baking {scene.name} ...");
            Lightmapping.Bake();
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[TehranCity] {scene.name} baked + saved.");
        }
        var boot = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Bootstrap.unity");
        Lightmapping.Clear();
        EditorSceneManager.SaveScene(boot);
        Debug.Log("[TehranCity] Per-scene bake done. Play from Bootstrap.");
    }

    [MenuItem("TehranCity/Setup/27 Cancel Bake")]
    public static void CancelBake()
    {
        Lightmapping.Cancel();
        Debug.Log("[TehranCity] Bake canceled.");
    }

    [MenuItem("TehranCity/Setup/28 Lighting Polish (Shadows ON + Sun Up)")]
    public static void Polish()
    {
        EditorSceneManager.SaveOpenScenes();
        foreach (var path in ScenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path);
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (light.type != LightType.Directional) continue;
                light.shadows = LightShadows.Soft;      // ریشه‌ی اصلی بی‌سایگی
                light.intensity = 1.2f;
                light.transform.rotation = Quaternion.Euler(62f, -35f, 0f); // خورشید بلندتر => کوچه نور می‌بیند
                light.lightmapBakeType = LightmapBakeType.Mixed;
            }
            RenderSettings.ambientIntensity = 1.15f;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        var rp = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        if (rp != null)
        {
            rp.shadowDistance = 80f;
            EditorUtility.SetDirty(rp);
            AssetDatabase.SaveAssets();
        }
        else
        {
            Debug.LogWarning("[Lighting] URP asset پیدا نشد؛ shadowDistance دستی ست کن.");
        }

        Debug.Log("[TehranCity] Shadows ON + sun raised. همین حالا Play کن (سایه realtime فوری). برای GI پخته، Setup 26 را دوباره بزن.");
    }

    private static void EnsureLightProbes(string sceneName)
    {
        var existing = Object.FindFirstObjectByType<LightProbeGroup>();
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        var go = new GameObject("LightProbes_Global");
        var probeGroup = go.AddComponent<LightProbeGroup>();

        float spacing = sceneName.Contains("Room01") ? 1.5f : 2.5f;
        Vector3 boundsMin = sceneName.Contains("Room01")
            ? new Vector3(-2f, 0.5f, -2.5f)
            : new Vector3(-8f, 0.5f, -8f);
        Vector3 boundsMax = sceneName.Contains("Room01")
            ? new Vector3(2f, 2.5f, 2.5f)
            : new Vector3(8f, 3f, 14f);

        var positions = new System.Collections.Generic.List<Vector3>();
        for (float x = boundsMin.x; x <= boundsMax.x; x += spacing)
            for (float y = boundsMin.y; y <= boundsMax.y; y += spacing)
                for (float z = boundsMin.z; z <= boundsMax.z; z += spacing)
                    positions.Add(new Vector3(x, y, z));

        probeGroup.probePositions = positions.ToArray();
    }

    private static void EnsureReflectionProbe()
    {
        var existing = Object.FindFirstObjectByType<ReflectionProbe>();
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        var go = new GameObject("ReflectionProbe_Room");
        var probe = go.AddComponent<ReflectionProbe>();
        probe.mode = ReflectionProbeMode.Baked;
        probe.boxProjection = true;
        probe.size = new Vector3(4f, 3f, 5f);
        probe.center = new Vector3(0f, 1.5f, 0f);
        probe.blendDistance = 0.1f;
        probe.resolution = 256;
        probe.hdr = true;
    }
}