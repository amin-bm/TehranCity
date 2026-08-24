using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// فاز A / قدم A9 — Post Processing: Bloom + Color Adjustments + Vignette.
/// v2: API واقعی URP + روشن‌کردن renderPostProcessing روی Main Camera ماندگار
/// (دروازه‌ی واقعی اعمال افکت‌ها).
/// </summary>
public static class PostProcessingSetup
{
    private const string ProfilePath = "Assets/_Project/Settings/PostProcessProfile.asset";
    private static readonly string[] ScenePaths =
    {
        "Assets/_Project/Scenes/Interior_Room01.unity",
        "Assets/_Project/Scenes/Prototype_Block01.unity",
        "Assets/_Project/Scenes/Interior_MobileShop01.unity",
    };

    [MenuItem("TehranCity/Setup/29 Post Processing (Bloom + Color + Vignette)")]
    public static void Run()
    {
        EditorSceneManager.SaveOpenScenes();

        // 1) Volume Profile
        EnsureFolder("Assets/_Project/Settings");
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
        }
        ConfigureProfile(profile);
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        // 2) Global Volume در هر صحنه + Post روی دوربین ماندگار (اتاق)
        foreach (var path in ScenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path);
            EnsureGlobalVolume(profile);
            if (scene.name.Contains("Room01"))
                EnableCameraPost();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("[TehranCity] Post Processing ready (camera renderPostProcessing=ON). Play از Bootstrap.");
    }

    private static void ConfigureProfile(VolumeProfile profile)
    {
        // Bloom (API واقعی URP)
        var bloom = GetOrCreate<Bloom>(profile);
        bloom.threshold.overrideState = true; bloom.threshold.value = 0.9f;
        bloom.intensity.overrideState = true; bloom.intensity.value = 1.3f;
        bloom.scatter.overrideState = true; bloom.scatter.value = 0.7f;
        bloom.tint.overrideState = true; bloom.tint.value = Color.white;
        bloom.highQualityFiltering.overrideState = true; bloom.highQualityFiltering.value = false;
        bloom.dirtIntensity.overrideState = true; bloom.dirtIntensity.value = 0f;

        // Color Adjustments (کمی گرم‌تر)
        var colorAdj = GetOrCreate<ColorAdjustments>(profile);
        colorAdj.postExposure.overrideState = true; colorAdj.postExposure.value = 0.2f;
        colorAdj.contrast.overrideState = true; colorAdj.contrast.value = 5f;
        colorAdj.colorFilter.overrideState = true; colorAdj.colorFilter.value = new Color(1f, 0.98f, 0.96f);
        colorAdj.hueShift.overrideState = true; colorAdj.hueShift.value = 0f;
        colorAdj.saturation.overrideState = true; colorAdj.saturation.value = 5f;

        // Vignette (API واقعی URP)
        var vignette = GetOrCreate<Vignette>(profile);
        vignette.color.overrideState = true; vignette.color.value = Color.black;
        vignette.center.overrideState = true; vignette.center.value = new Vector2(0.5f, 0.5f);
        vignette.intensity.overrideState = true; vignette.intensity.value = 0.3f;
        vignette.smoothness.overrideState = true; vignette.smoothness.value = 0.2f;
        vignette.rounded.overrideState = true; vignette.rounded.value = false;
    }

    private static T GetOrCreate<T>(VolumeProfile profile) where T : VolumeComponent
    {
        if (profile.TryGet<T>(out var existing))
            return existing;
        return profile.Add<T>(true);
    }

    private static void EnsureGlobalVolume(VolumeProfile profile)
    {
        var existing = Object.FindFirstObjectByType<Volume>();
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        var go = new GameObject("GlobalVolume_PostProcess");
        var volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 0f;
        volume.blendDistance = 0f;
        volume.weight = 1f;
        volume.sharedProfile = profile;
    }

    private static void EnableCameraPost()
    {
        var camGo = GameObject.Find("Main Camera");
        if (camGo == null)
        {
            Debug.LogWarning("[Post] Main Camera پیدا نشد.");
            return;
        }
        var cam = camGo.GetComponent<Camera>();
        if (cam == null) return;
        cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
    }

    private static void EnsureFolder(string folderPath)
    {
        folderPath = folderPath.Replace("\\", "/");
        if (folderPath == "Assets" || AssetDatabase.IsValidFolder(folderPath)) return;
        var parent = System.IO.Path.GetDirectoryName(folderPath);
        if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent.Replace("\\", "/"));
        AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(folderPath), System.IO.Path.GetFileName(folderPath));
    }
}