using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PhoneCleanup
{
    [MenuItem("TehranCity/Setup/16) Cleanup Duplicate Phones")]
    public static void Cleanup()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        foreach (var path in new[]
        {
            "Assets/_Project/Scenes/Bootstrap.unity",
            "Assets/_Project/Scenes/Interior_Room01.unity",
            "Assets/_Project/Scenes/Prototype_Block01.unity",
            "Assets/_Project/Scenes/Interior_MobileShop01.unity",
        })
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var controllers = Object.FindObjectsByType<PhoneController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var pc in controllers)
            {
                Debug.Log($"[Cleanup] Phone in '{scene.name}': '{pc.gameObject.name}' activeSelf={pc.gameObject.activeSelf}");
                if (!pc.gameObject.activeSelf)
                {
                    Debug.Log($"[Cleanup] destroying duplicate '{pc.gameObject.name}' in {scene.name}");
                    Object.DestroyImmediate(pc.gameObject);
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }

            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("[Cleanup] done.");
    }
}