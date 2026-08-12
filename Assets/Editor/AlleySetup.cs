using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AlleySetup
{
    private const string RoomPath = "Assets/_Project/Scenes/Interior_Room01.unity";
    private const string AlleyPath = "Assets/_Project/Scenes/Prototype_Block01.unity";
    private const string MatDir = "Assets/_Project/Art/Environment/Narmak/Materials";
    private const string FontPath = "Assets/_Project/Art/UI/Fonts/TMP_Tahoma_Persian.asset";

    [MenuItem("TehranCity/Setup/11) Setup Alley (Prototype_Block01) + Persistence")]
    public static void Setup()
    {
        EditorSceneManager.SaveOpenScenes();

        // ---------- A) اتاق: ماندگاری + در خروج ----------
        var room = EditorSceneManager.OpenScene(RoomPath);

        if (GameObject.Find("CameraRig") == null)
        {
            var rig = new GameObject("CameraRig");
            foreach (var n in new[] { "Main Camera", "CameraPivot", "CM_IsometricCam" })
            {
                var o = GameObject.Find(n);
                if (o != null) o.transform.SetParent(rig.transform, true);
            }
            rig.AddComponent<PersistentObject>();
        }

        foreach (var n in new[] { "UI_Canvas", "UI_HUD", "UI_Phone" })
        {
            var o = GameObject.Find(n);
            if (o != null && o.GetComponent<PersistentObject>() == null)
                o.AddComponent<PersistentObject>();
        }

        if (GameObject.Find("Door_Exit") == null)
        {
            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door_Exit";
            door.transform.localScale = new Vector3(1f, 2.2f, 0.2f);
            door.transform.position = new Vector3(0f, 1.1f, -2.6f);
            door.GetComponent<Renderer>().sharedMaterial = GetMat("MAT_Alley_Door", new Color(0.35f, 0.22f, 0.12f));
            var sd = door.AddComponent<SceneDoor>();
            sd.prompt = "خروج از اتاق";
            sd.targetScene = "Prototype_Block01";
            sd.spawnPointName = "Spawn_Alley";
        }
        EditorSceneManager.SaveScene(room);

        // ---------- B) کوچه نارمک ----------
        var alley = EditorSceneManager.OpenScene(AlleyPath);
        foreach (var n in new[] { "AlleyRoot", "Player", "Spawn_Alley", "Directional Light_Alley" })
        {
            var o = GameObject.Find(n);
            if (o != null) Object.DestroyImmediate(o);
        }

        var root = new GameObject("AlleyRoot");
        var matGround = GetMat("MAT_Alley_Ground", new Color(0.25f, 0.25f, 0.27f));
        var matBuilding = GetMat("MAT_Alley_Building", new Color(0.75f, 0.70f, 0.60f));
        var matBuilding2 = GetMat("MAT_Alley_Building2", new Color(0.65f, 0.55f, 0.45f));
        var matSign = GetMat("MAT_Alley_Sign", new Color(0.10f, 0.35f, 0.60f));
        var matCar = GetMat("MAT_Alley_Car", new Color(0.55f, 0.10f, 0.10f));
        var matDoor = GetMat("MAT_Alley_Door", new Color(0.35f, 0.22f, 0.12f));

        // زمین کوچه + خیابان
        Box(root, "Ground", new Vector3(40f, 0.1f, 40f), new Vector3(0f, -0.05f, 4f), matGround);

        // ساختمان اتاق بازیکن + در برگشت
        Box(root, "Building_Room", new Vector3(6f, 4f, 6f), new Vector3(0f, 2f, -6f), matBuilding);
        var doorBack = Box(root, "Door_Back", new Vector3(1f, 2.2f, 0.2f), new Vector3(0f, 1.1f, -2.9f), matDoor);
        var sdBack = doorBack.AddComponent<SceneDoor>();
        sdBack.prompt = "ورود به اتاق";
        sdBack.targetScene = "Interior_Room01";
        sdBack.spawnPointName = "PlayerSpawn";

        // دو طرف کوچه
        Box(root, "Building_W", new Vector3(4f, 5f, 14f), new Vector3(-5f, 2.5f, -6f), matBuilding2);
        Box(root, "Building_E", new Vector3(4f, 4f, 14f), new Vector3(5f, 2f, -6f), matBuilding);

        // نماهای روبه‌روی خیابان (طبق World Content)
        Box(root, "Facade_Supermarket", new Vector3(6f, 3f, 4f), new Vector3(-8f, 1.5f, 15f), matBuilding2);
        Box(root, "Facade_Passage", new Vector3(8f, 4f, 4f), new Vector3(0f, 2f, 15f), matBuilding);
        Box(root, "Facade_TinyShop", new Vector3(5f, 3f, 4f), new Vector3(8f, 1.5f, 15f), matBuilding2);

        var sup = Box(root, "Door_Supermarket", new Vector3(1.2f, 2.2f, 0.2f), new Vector3(-8f, 1.1f, 12.9f), matDoor);
        sup.AddComponent<SimpleInteractable>().Configure("سوپرمارکت", "فعلاً بسته است (دمو)");
        var pas = Box(root, "Door_Passage", new Vector3(2f, 2.6f, 0.2f), new Vector3(0f, 1.3f, 12.9f), matDoor);
        pas.AddComponent<SimpleInteractable>().Configure("پاساژ موبایل‌سنتر", "قدم بعدی: مصاحبه و شیفت اول (دمو)");
        var tiny = Box(root, "Door_TinyShop", new Vector3(1.2f, 2.2f, 0.2f), new Vector3(8f, 1.1f, 12.9f), matDoor);
        tiny.AddComponent<SimpleInteractable>().Configure("مغازه خیلی کوچک",
            "برای اجاره این مغازه به پانزده میلیون تومان پس‌انداز نیاز داری."); // hook هدف ۱۵ م

        // تابلوهای فارسی (پارودی Naming Bible)
        Sign(root, "Sign_Supermarket", "رفاه‌مارکت", 5f, new Vector3(-8f, 3.4f, 12.8f), matSign);
        Sign(root, "Sign_Passage", "پاساژ موبایل‌سنتر", 7f, new Vector3(0f, 4.4f, 12.8f), matSign);
        Sign(root, "Sign_TinyShop", "مغازه خیلی کوچک - اجاره", 6f, new Vector3(8f, 3.4f, 12.8f), matSign);

        // حس تهران: ماشین پارک‌شده + دستفروش
        Box(root, "Car_Body", new Vector3(1.8f, 0.6f, 4.2f), new Vector3(4f, 0.35f, 5f), matCar);
        Box(root, "Car_Top", new Vector3(1.6f, 0.5f, 2.2f), new Vector3(4f, 0.85f, 4.8f), matCar);
        Box(root, "Vendor_Table", new Vector3(1.5f, 0.8f, 0.8f), new Vector3(-4f, 0.4f, 4f), matBuilding2);

        // اسپان + بازیکن + نور
        var spawn = new GameObject("Spawn_Alley");
        spawn.transform.position = new Vector3(0f, 0f, -1f);
        BuildPlayer(spawn.transform.position);

        var light = new GameObject("Directional Light_Alley");
        var l = light.AddComponent<Light>();
        l.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        EditorSceneManager.SaveScene(alley);
        Debug.Log("[TehranCity] Alley (Prototype_Block01) ready. Play from Bootstrap!");
    }

    private static void BuildPlayer(Vector3 pos)
    {
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = pos + new Vector3(0f, 1f, 0f);
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        var cc = player.AddComponent<CharacterController>();
        cc.center = Vector3.zero; cc.height = 2f; cc.radius = 0.4f; cc.stepOffset = 0.3f;

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
        player.AddComponent<Interactor>();
    }

    private static void Sign(GameObject parent, string name, string text, float width, Vector3 pos, Material mat)
    {
        // تخته تابلو
        Box(parent, name, new Vector3(width, 1f, 0.1f), pos, mat);

        // متن: فرزندِ والد بدون scale + بدون چرخش (تجربه: دوربین کوچه از پشت نمی‌بیند)
        var tgo = new GameObject(name + "_Text");
        tgo.transform.SetParent(parent.transform, false);
        tgo.transform.localPosition = pos + new Vector3(0f, 0f, -0.12f); // جلوی تخته
        tgo.transform.localRotation = Quaternion.identity;
        tgo.transform.localScale = Vector3.one;

        var tmp = tgo.AddComponent<RTLTextMeshPro3D>();
        tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        tmp.fontSize = 4.2f;   // تنها عددی که اگر نیاز شد، همین‌جا تغییر بده
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        var label = tgo.AddComponent<SignLabel>();
        label.text = text;
    }

    private static GameObject Box(GameObject parent, string name, Vector3 scale, Vector3 pos, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localScale = scale;
        go.transform.localPosition = pos;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        go.isStatic = true;
        return go;
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