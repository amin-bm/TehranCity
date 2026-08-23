using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// فاز A / قدم A5 — مانکن بلوکی سبک Precinct + کلیپ‌های Idle/Walk + AnimatorController.
/// کپسول قدیمی پنهان می‌شود (نه حذف)؛ کامپوننت‌ها/کولایدر دست‌نخورده. idempotent.
/// تعمیر: ساخت کنترلر با CreateAnimatorControllerAtPath (نه CreateInstance).
/// </summary>
public static class CharacterSetup
{
    private const string CharDir = "Assets/_Project/Art/Characters";
    private const string MatDir = "Assets/_Project/Art/Environment/Narmak/Materials";
    private static readonly string[] ScenePaths =
    {
        "Assets/_Project/Scenes/Interior_Room01.unity",
        "Assets/_Project/Scenes/Prototype_Block01.unity",
        "Assets/_Project/Scenes/Interior_MobileShop01.unity",
    };

    [MenuItem("TehranCity/Setup/22 Character Mannequin + Animator")]
    public static void Run()
    {
        EditorSceneManager.SaveOpenScenes();
        EnsureFolder(CharDir);
        var idle = EnsureIdleClip();
        var walk = EnsureWalkClip();
        var controller = EnsureController(idle, walk);
        AssetDatabase.SaveAssets();

        foreach (var path in ScenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path);
            var player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogWarning($"[Char] Player در {path} پیدا نشد؛ skip.");
                continue;
            }
            BuildMannequin(player.transform, controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log("[TehranCity] Character mannequin + Animator ready in all scenes (PC-10.8).");
    }

    private static void BuildMannequin(Transform player, RuntimeAnimatorController controller)
    {
        // پنهان‌کردن ظاهر قدیمی (کولایدر/کامپوننت‌ها دست‌نخورده)
        var pr = player.GetComponent<Renderer>();
        if (pr != null) pr.enabled = false;
        var nose = player.Find("FacingNose");
        if (nose != null)
        {
            var nr = nose.GetComponent<Renderer>();
            if (nr != null) nr.enabled = false;
        }

        var old = player.Find("Mannequin");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var matSkin = GetMat("MATC_Skin", new Color(0.85f, 0.60f, 0.45f));
        var matHair = GetMat("MATC_Hair", new Color(0.15f, 0.12f, 0.10f));
        var matShirt = ArtKit.Mat("MATS_Room_Blanket");
        var matPants = ArtKit.Mat("MATS_Sign_Blue");

        // ریشه‌ی Mannequin روی مرکز کپسول ( feet = local -1 )
        var man = new GameObject("Mannequin");
        man.transform.SetParent(player, false);
        man.transform.localPosition = Vector3.zero;
        man.transform.localRotation = Quaternion.identity;

        var pelvis = new GameObject("Pelvis");
        pelvis.transform.SetParent(man.transform, false);
        pelvis.transform.localPosition = new Vector3(0f, -0.05f, 0f);

        Box(pelvis.transform, "Torso", new Vector3(0.5f, 0.55f, 0.28f), new Vector3(0f, 0.30f, 0f), matShirt);
        Box(pelvis.transform, "Head", new Vector3(0.32f, 0.34f, 0.32f), new Vector3(0f, 0.72f, 0f), matSkin);
        Box(pelvis.transform, "Hair", new Vector3(0.34f, 0.12f, 0.34f), new Vector3(0f, 0.88f, -0.02f), matHair);

        Limb(pelvis.transform, "Pivot_Arm_L", new Vector3(-0.31f, 0.48f, 0f), "Arm_L", new Vector3(0.12f, 0.6f, 0.16f), matShirt);
        Limb(pelvis.transform, "Pivot_Arm_R", new Vector3(0.31f, 0.48f, 0f), "Arm_R", new Vector3(0.12f, 0.6f, 0.16f), matShirt);
        Limb(pelvis.transform, "Pivot_Leg_L", new Vector3(-0.14f, 0f, 0f), "Leg_L", new Vector3(0.16f, 0.95f, 0.2f), matPants);
        Limb(pelvis.transform, "Pivot_Leg_R", new Vector3(0.14f, 0f, 0f), "Leg_R", new Vector3(0.16f, 0.95f, 0.2f), matPants);

        var anim = man.AddComponent<Animator>();
        anim.runtimeAnimatorController = controller;
        anim.applyRootMotion = false;
    }

    private static void Limb(Transform parent, string pivotName, Vector3 pivotPos, string meshName, Vector3 size, Material mat)
    {
        var pivot = new GameObject(pivotName);
        pivot.transform.SetParent(parent, false);
        pivot.transform.localPosition = pivotPos;
        Box(pivot.transform, meshName, size, new Vector3(0f, -size.y / 2f + 0.05f, 0f), mat);
    }

    private static void Box(Transform parent, string name, Vector3 scale, Vector3 localPos, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localScale = scale;
        go.transform.localPosition = localPos;
        Object.DestroyImmediate(go.GetComponent<Collider>()); // بدون کولایدر (فیزیک = CharacterController)
        go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    // ---------- کلیپ‌ها ----------

    private static AnimationClip EnsureWalkClip()
    {
        string path = $"{CharDir}/AC_Arash_Walk.anim";
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null) return existing;
        var clip = new AnimationClip();
        clip.name = "AC_Arash_Walk";
        clip.wrapMode = WrapMode.Loop;
        const float L = 0.6f;
        SetEulerX(clip, "Pelvis/Pivot_Arm_L", L, new[] { 35f, 0f, -35f, 0f, 35f });
        SetEulerX(clip, "Pelvis/Pivot_Arm_R", L, new[] { -35f, 0f, 35f, 0f, -35f });
        SetEulerX(clip, "Pelvis/Pivot_Leg_L", L, new[] { -30f, 0f, 30f, 0f, -30f });
        SetEulerX(clip, "Pelvis/Pivot_Leg_R", L, new[] { 30f, 0f, -30f, 0f, 30f });
        var s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimationClip EnsureIdleClip()
    {
        string path = $"{CharDir}/AC_Arash_Idle.anim";
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null) return existing;
        var clip = new AnimationClip();
        clip.name = "AC_Arash_Idle";
        clip.wrapMode = WrapMode.Loop;
        const float L = 1.2f;
        SetEulerX(clip, "Pelvis/Pivot_Arm_L", L, new[] { 2f, 0f, -2f, 0f, 2f });
        SetEulerX(clip, "Pelvis/Pivot_Arm_R", L, new[] { -2f, 0f, 2f, 0f, -2f });
        var s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static void SetEulerX(AnimationClip clip, string path, float length, float[] vals5)
    {
        var keys = new Keyframe[5];
        for (int i = 0; i < 5; i++)
            keys[i] = new Keyframe(length * i / 4f, vals5[i]);
        clip.SetCurve(path, typeof(Transform), "localEulerAngles.x", new AnimationCurve(keys));
    }

    private static AnimatorController EnsureController(AnimationClip idle, AnimationClip walk)
    {
        string path = $"{CharDir}/AC_Arash.controller";
        var existing = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path) as AnimatorController;
        if (existing != null) return existing;

        // مسیر رسمی ساخت کنترلر در کد (AnimatorController ScriptableObject نیست)
        var c = AnimatorController.CreateAnimatorControllerAtPath(path);
        c.name = "AC_Arash";
        c.AddParameter("Speed", AnimatorControllerParameterType.Float);
        if (c.layers == null || c.layers.Length == 0)
            c.AddLayer("Base Layer");
        var sm = c.layers[0].stateMachine;
        var sIdle = sm.AddState("Idle"); sIdle.motion = idle;
        var sWalk = sm.AddState("Walk"); sWalk.motion = walk;
        sm.defaultState = sIdle;
        var tW = sIdle.AddTransition(sWalk);
        tW.hasExitTime = false; tW.duration = 0.12f;
        tW.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        var tI = sWalk.AddTransition(sIdle);
        tI.hasExitTime = false; tI.duration = 0.12f;
        tI.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        EditorUtility.SetDirty(c);
        return c;
    }

    private static Material GetMat(string name, Color color)
    {
        Directory.CreateDirectory(MatDir);
        string p = $"{MatDir}/{name}.mat";
        var ex = AssetDatabase.LoadAssetAtPath<Material>(p);
        if (ex != null) return ex;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.name = name;
        mat.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(mat, p);
        return mat;
    }

    private static void EnsureFolder(string folderPath)
    {
        folderPath = folderPath.Replace("\\", "/");
        if (folderPath == "Assets" || AssetDatabase.IsValidFolder(folderPath)) return;
        var parent = Path.GetDirectoryName(folderPath);
        if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent.Replace("\\", "/"));
        AssetDatabase.CreateFolder(Path.GetDirectoryName(folderPath), Path.GetFileName(folderPath));
    }
}