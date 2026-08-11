using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PlayerSetup
{
    private const string ScenePath = "Assets/_Project/Scenes/Interior_Room01.unity";
    private const string MatDir = "Assets/_Project/Art/Environment/Narmak/Materials";

    [MenuItem("TehranCity/Setup/5) Setup Player (Interior_Room01)")]
    public static void Setup()
    {
        EditorSceneManager.SaveOpenScenes();
        var scene = EditorSceneManager.OpenScene(ScenePath);

        var old = GameObject.Find("Player");
        if (old != null) Object.DestroyImmediate(old);

        var spawn = GameObject.Find("PlayerSpawn");
        Vector3 spawnPos = spawn != null ? spawn.transform.position : Vector3.zero;

        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = spawnPos + new Vector3(0f, 1f, 0f); // پاها روی کف

        // CharacterController جای CapsuleCollider پیش‌فرض می‌نشیند
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        var controller = player.AddComponent<CharacterController>();
        controller.center = Vector3.zero;
        controller.height = 2f;
        controller.radius = 0.4f;
        controller.stepOffset = 0.3f;

        // "بین" برای دیدن جهت کاراکتر در greybox
        var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nose.name = "FacingNose";
        Object.DestroyImmediate(nose.GetComponent<Collider>());
        nose.transform.SetParent(player.transform);
        nose.transform.localPosition = new Vector3(0f, 0.35f, 0.45f);
        nose.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        var mat = GetMat("MAT_Greybox_Player", new Color(0.95f, 0.55f, 0.15f));
        player.GetComponent<Renderer>().sharedMaterial = mat;
        nose.GetComponent<Renderer>().sharedMaterial = mat;

        player.AddComponent<PlayerController>();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[TehranCity] Player (Arash placeholder) ready in Interior_Room01.");
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