using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Cinemachine;

public static class CameraSetup
{
    private const string ScenePath = "Assets/_Project/Scenes/Interior_Room01.unity";

    [MenuItem("TehranCity/Setup/6) Setup Isometric Camera (Cinemachine)")]
    public static void Setup()
    {
        EditorSceneManager.SaveOpenScenes();
        var scene = EditorSceneManager.OpenScene(ScenePath);

        var player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("[TehranCity] Player not found. Run Setup 5 first.");
            return;
        }

        // Main Camera + CinemachineBrain
        var camGo = GameObject.Find("Main Camera");
        if (camGo == null)
        {
            camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
        }
        if (camGo.GetComponent<CinemachineBrain>() == null)
            camGo.AddComponent<CinemachineBrain>();

        // Pivot بدون چرخش
        var pivot = GameObject.Find("CameraPivot");
        if (pivot == null) pivot = new GameObject("CameraPivot");

        // دوربین مجازی ایزومتریک
        var old = GameObject.Find("CM_IsometricCam");
        if (old != null) Object.DestroyImmediate(old);
        var vcamGo = new GameObject("CM_IsometricCam");

        var vcam = vcamGo.AddComponent<CinemachineCamera>();
        vcam.Follow = pivot.transform;
        vcam.LookAt = pivot.transform;

        var follow = vcamGo.AddComponent<CinemachineFollow>();
        follow.FollowOffset = new Vector3(0f, 7.07f, -7.07f); // فاصله ۱۰، زاویه ۴۵

        vcamGo.AddComponent<CinemachineHardLookAt>();

        var controller = vcamGo.AddComponent<IsometricCameraController>();
        controller.player = player.transform;
        controller.pivot = pivot.transform;

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[TehranCity] Isometric Cinemachine camera ready.");
    }
}