using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GreyboxBuilder
{
    private const string ScenePath = "Assets/_Project/Scenes/Interior_Room01.unity";
    private const string MatDir = "Assets/_Project/Art/Environment/Narmak/Materials";

    [MenuItem("TehranCity/Setup/4) Build Greybox Room (Interior_Room01)")]
    public static void Build()
    {
        EditorSceneManager.SaveOpenScenes();
        var scene = EditorSceneManager.OpenScene(ScenePath);

        // پاکسازی اجرای قبلی (idempotent)
        foreach (var n in new[] { "GreyboxRoom", "Directional Light_Greybox", "Main Camera", "PlayerSpawn" })
        {
            var old = GameObject.Find(n);
            if (old != null) Object.DestroyImmediate(old);
        }

        var root = new GameObject("GreyboxRoom");

        var matFloor = GetMat("MAT_Greybox_Floor", new Color(0.35f, 0.30f, 0.28f));
        var matWall = GetMat("MAT_Greybox_Wall", new Color(0.80f, 0.75f, 0.65f));
        var matBed = GetMat("MAT_Greybox_Bed", new Color(0.20f, 0.40f, 0.70f));
        var matWood = GetMat("MAT_Greybox_Wood", new Color(0.45f, 0.30f, 0.20f));
        var matPhone = GetMat("MAT_Greybox_Phone", new Color(0.10f, 0.80f, 0.30f));
        var matMirror = GetMat("MAT_Greybox_Mirror", new Color(0.60f, 0.80f, 0.90f));

        // ---- پوسته اتاق (4x5 متر، ارتفاع 3، درب در دیوار جنوبی) ----
        Box(root, "Floor", new Vector3(4.4f, 0.1f, 5.4f), new Vector3(0, -0.05f, 0), matFloor);
        Box(root, "Wall_N", new Vector3(4.4f, 3f, 0.2f), new Vector3(0, 1.5f, 2.7f), matWall);
        Box(root, "Wall_E", new Vector3(0.2f, 3f, 5.4f), new Vector3(2.2f, 1.5f, 0), matWall);
        Box(root, "Wall_W", new Vector3(0.2f, 3f, 5.4f), new Vector3(-2.2f, 1.5f, 0), matWall);
        Box(root, "Wall_S_A", new Vector3(1.7f, 3f, 0.2f), new Vector3(-1.35f, 1.5f, -2.7f), matWall);
        Box(root, "Wall_S_B", new Vector3(1.7f, 3f, 0.2f), new Vector3(1.35f, 1.5f, -2.7f), matWall);
        Box(root, "Wall_S_DoorTop", new Vector3(1f, 0.8f, 0.2f), new Vector3(0, 2.6f, -2.7f), matWall);

        // ---- اثاثیه طبق سند: تخت، میز، گوشی، آینه ----
        Box(root, "Bed", new Vector3(0.9f, 0.45f, 2f), new Vector3(-1.6f, 0.225f, 1.5f), matBed);
        Box(root, "Pillow", new Vector3(0.7f, 0.12f, 0.4f), new Vector3(-1.6f, 0.51f, 2.2f), matWall);
        Box(root, "Table", new Vector3(1.2f, 0.75f, 0.6f), new Vector3(1.4f, 0.375f, -1.8f), matWood);
        Box(root, "Interact_Phone", new Vector3(0.09f, 0.02f, 0.16f), new Vector3(1.4f, 0.76f, -1.8f), matPhone);
        Box(root, "Mirror", new Vector3(0.5f, 1.2f, 0.05f), new Vector3(-0.8f, 1.6f, 2.57f), matMirror);
        Box(root, "Wardrobe", new Vector3(0.8f, 1.8f, 0.6f), new Vector3(1.6f, 0.9f, 1.9f), matWood);

        // ---- نقطه اسپان بازیکن (قدم ۴ استفاده می‌شود) ----
        var spawn = new GameObject("PlayerSpawn");
        spawn.transform.position = new Vector3(0, 0, -1.5f);

        // ---- نور + دوربین موقت (قدم ۵ با Cinemachine جایگزین می‌شود) ----
        var light = new GameObject("Directional Light_Greybox");
        var l = light.AddComponent<Light>();
        l.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        var cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        cam.AddComponent<Camera>();
        cam.transform.position = new Vector3(0, 6f, -6f);
        cam.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[TehranCity] Greybox room built in Interior_Room01.");
    }

    private static void Box(GameObject parent, string name, Vector3 scale, Vector3 pos, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.parent = parent.transform;
        go.transform.localScale = scale;
        go.transform.localPosition = pos;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        go.isStatic = true;
    }

    private static Material GetMat(string name, Color color)
    {
        Directory.CreateDirectory(MatDir);
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