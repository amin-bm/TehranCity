using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TehranCitySetup
{
    private static readonly string[] Folders = new[]
    {
        "Assets/_Project/Art/Characters",
        "Assets/_Project/Art/Environment/Narmak",
        "Assets/_Project/Art/Props",
        "Assets/_Project/Art/UI",
        "Assets/_Project/Audio/Music",
        "Assets/_Project/Audio/SFX",
        "Assets/_Project/Data/ScriptableObjects",
        "Assets/_Project/Data/Balance",
        "Assets/_Project/Prefabs/Characters",
        "Assets/_Project/Prefabs/NPCs",
        "Assets/_Project/Prefabs/Interactables",
        "Assets/_Project/Prefabs/UI",
        "Assets/_Project/Scenes",
        "Assets/_Project/Scripts/Core",
        "Assets/_Project/Scripts/Gameplay",
        "Assets/_Project/Scripts/UI",
        "Assets/_Project/Scripts/Data",
    };

    private static readonly string[] SceneNames = new[]
    {
        "Bootstrap",            // باید index 0 بیلد باشد
        "Main Menu",
        "Prototype_Block01",
        "Interior_Room01",
        "Interior_MobileShop01",
    };

    [MenuItem("TehranCity/Setup/1) Create Folders")]
    public static void CreateFolders()
    {
        foreach (var f in Folders)
            Directory.CreateDirectory(f);
        AssetDatabase.Refresh();
        Debug.Log("[TehranCity] Folder structure created.");
    }

    [MenuItem("TehranCity/Setup/2) Create Scenes")]
    public static void CreateScenes()
    {
        foreach (var name in SceneNames)
        {
            string path = $"Assets/_Project/Scenes/{name}.unity";
            if (File.Exists(path))
            {
                Debug.Log($"[TehranCity] Scene already exists: {path}");
                continue;
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[TehranCity] Scene created: {path}");
        }

        // ثبت در Build Settings به ترتیب درست
        var list = new List<EditorBuildSettingsScene>();
        foreach (var name in SceneNames)
        {
            string path = $"Assets/_Project/Scenes/{name}.unity";
            if (File.Exists(path))
                list.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = list.ToArray();

        EditorSceneManager.OpenScene("Assets/_Project/Scenes/Bootstrap.unity");
        AssetDatabase.Refresh();
        Debug.Log("[TehranCity] Scenes registered in Build Settings (Bootstrap = 0).");
    }

    [MenuItem("TehranCity/Setup/3) Apply Base Project Settings")]
    public static void ApplyBaseSettings()
    {
        PlayerSettings.companyName = "AM Company";
        PlayerSettings.productName = "Tehran City";

        SetActiveInputHandling();

        Debug.Log("[TehranCity] Base project settings applied.");
    }

    private static void SetActiveInputHandling()
    {
        // در Unity 6.3 API مستقیم activeInputHandler حذف شده؛
        // از SerializedObject استفاده می‌کنیم و اگر property وجود نداشت،
        // یعنی New Input System خودش پیش‌فرض است و کاری لازم نیست.
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
        if (assets == null || assets.Length == 0)
        {
            Debug.LogWarning("[TehranCity] ProjectSettings.asset not found.");
            return;
        }

        var so = new SerializedObject(assets[0]);
        var prop = so.FindProperty("activeInputHandler");
        if (prop == null)
        {
            Debug.Log("[TehranCity] activeInputHandler در این نسخه وجود ندارد؛ Unity 6.3 به‌صورت پیش‌فرض با New Input System کار می‌کند. OK");
            return;
        }

        // 0 = Old Input Manager | 1 = Input System Package (New) | 2 = Both
        prop.intValue = 1;
        so.ApplyModifiedProperties();
        Debug.Log("[TehranCity] Active Input Handling => Input System Package (New). اگر Unity پیشنهاد Restart داد، بپذیر.");
    }
}