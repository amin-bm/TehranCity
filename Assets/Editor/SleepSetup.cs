using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SleepSetup
{
    private const string RoomPath = "Assets/_Project/Scenes/Interior_Room01.unity";
    private const string MatDir = "Assets/_Project/Art/Environment/Narmak/Materials";

    [MenuItem("TehranCity/Setup/15) Bed + Sleep (Day Cycle)")]
    public static void Setup()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        var room = EditorSceneManager.OpenScene(RoomPath);

        // 1) تخت با نام دقیق
        var bed = GameObject.Find("Bed");

        // 2) وگرنه هر آبجکتی که نامش شامل bed باشد (greybox قبلی)
        if (bed == null)
        {
            foreach (var root in room.GetRootGameObjects())
            {
                var t = FindBed(root.transform);
                if (t != null) { bed = t.gameObject; break; }
            }
        }

        // 3) وگرنه بساز
        if (bed == null)
        {
            bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bed.name = "Bed";
            bed.transform.localScale = new Vector3(1.6f, 0.6f, 2.2f);
            bed.transform.position = new Vector3(2.0f, 0.3f, 1.2f);
            bed.GetComponent<Renderer>().sharedMaterial = GetMat("MAT_Room_Bed", new Color(0.35f, 0.45f, 0.65f));
        }

        bed.isStatic = true;
        if (bed.GetComponent<Collider>() == null)
            bed.AddComponent<BoxCollider>();
        if (bed.GetComponent<Bed>() == null)
            bed.AddComponent<Bed>();

        EditorSceneManager.MarkSceneDirty(room);
        EditorSceneManager.SaveScene(room);
        Debug.Log($"[TehranCity] Bed ready @ {bed.transform.position} (name='{bed.name}'). Play from Bootstrap!");
    }

    private static Transform FindBed(Transform t)
    {
        if (t.name.ToLower().Contains("bed")) return t;
        foreach (Transform child in t)
        {
            var r = FindBed(child);
            if (r != null) return r;
        }
        return null;
    }

    private static Material GetMat(string name, Color color)
    {
        System.IO.Directory.CreateDirectory(MatDir);
        string path = $"{MatDir}/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.name = name;
        mat.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
}