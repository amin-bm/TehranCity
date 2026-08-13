using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SaveSetup
{
    private const string AlleyPath = "Assets/_Project/Scenes/Prototype_Block01.unity";

    [MenuItem("TehranCity/Setup/14) Save System + Tiny Shop Hook")]
    public static void Setup()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        var alley = EditorSceneManager.OpenScene(AlleyPath);

        var tiny = GameObject.Find("Door_TinyShop");
        if (tiny != null)
        {
            foreach (var mb in tiny.GetComponents<MonoBehaviour>())
                if (mb != null && mb.GetType().Name.Contains("SimpleInteractable"))
                    Object.DestroyImmediate(mb);

            if (tiny.GetComponent<TinyShopHook>() == null)
                tiny.AddComponent<TinyShopHook>();
        }

        EditorSceneManager.SaveScene(alley);
        Debug.Log("[TehranCity] Save system + TinyShop hook ready. Play from Bootstrap!");
    }
}