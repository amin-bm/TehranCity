using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// فاز A / قدم A4 — Art Pass داخلی مغازه موبایل + نمای بیرونی پاساژ.
/// کولایدرها دست‌نخورده؛ فقط ظاهر. کاملاً idempotent.
/// تعمیر: ذخیره صریح هر صحنه با MarkSceneDirty + SaveScene (نه SaveOpenScenes).
/// </summary>
public static class ShopArtPass
{
    private const string ShopPath = "Assets/_Project/Scenes/Interior_MobileShop01.unity";
    private const string AlleyPath = "Assets/_Project/Scenes/Prototype_Block01.unity";

    [MenuItem("TehranCity/Setup/21 Shop + Facade Art Pass (Precinct Style)")]
    public static void Run()
    {
        EditorSceneManager.SaveOpenScenes();
        var shopScene = EditorSceneManager.OpenScene(ShopPath);
        var rootGo = GameObject.Find("MobileShopRoot");
        if (rootGo == null)
        {
            Debug.LogError("[ShopArt] MobileShopRoot پیدا نشد؛ اول Setup 12 را اجرا کن.");
            return;
        }
        var rt = rootGo.transform;

        // ---------- 1) Reskin پوسته ----------
        ArtKit.ReskinByName(rt, "Floor", ArtKit.Mat("MATS_Shop_FloorTile"));
        foreach (var n in new[] { "Wall_Back", "Wall_Left", "Wall_Right", "Wall_Front_Left", "Wall_Front_Right", "Wall_Front_Top" })
            ArtKit.ReskinByName(rt, n, ArtKit.Mat("MATS_Shop_Wall"));
        ArtKit.ReskinByName(rt, "Door_ToAlley", ArtKit.Mat("MATS_Alley_DoorWood"));
        ArtKit.ReskinByName(rt, "Employer", ArtKit.Mat("MATS_Sign_Blue"));

        // ---------- 2) Hide کانتر/قفسه‌ها؛ نسخه استایلیزه ----------
        ArtKit.HideRenderersByName(rt, "Counter", "Shelf_Left", "Shelf_Right", "Shelf_Back");

        var art = ArtKit.EnsureArtRoot("ShopArt");

        // ---- کانتر ----
        var counter = new GameObject("Art_Counter").transform; counter.SetParent(art, false);
        ArtKit.Box(counter, "Front", new Vector3(3.2f, 0.9f, 0.65f), new Vector3(0f, 0.45f, -1.2f), ArtKit.Mat("MATS_Shop_CounterFront"));
        ArtKit.Box(counter, "Top", new Vector3(3.4f, 0.08f, 0.8f), new Vector3(0f, 0.94f, -1.2f), ArtKit.Mat("MATS_Shop_CounterTop"));
        ArtKit.Box(counter, "Glass", new Vector3(1.4f, 0.35f, 0.5f), new Vector3(0.7f, 1.16f, -1.2f), ArtKit.Mat("MATS_Shop_Glass"));
        ArtKit.Box(counter, "Register", new Vector3(0.3f, 0.25f, 0.3f), new Vector3(-1.2f, 1.1f, -1.25f), ArtKit.Mat("MATS_Room_Metal"));
        ArtKit.Box(counter, "RegScreen", new Vector3(0.22f, 0.16f, 0.03f), new Vector3(-1.2f, 1.28f, -1.1f), ArtKit.Mat("MATS_Glow_Screen"));

        // ---- قفسه‌ها ----
        ShelfUnit(art, "Art_Shelf_L", new Vector3(-4f, 0.75f, -2f), new Vector3(2f, 1.5f, 0.7f));
        ShelfUnit(art, "Art_Shelf_R", new Vector3(4f, 0.75f, -2f), new Vector3(2f, 1.5f, 0.7f));
        ShelfUnit(art, "Art_Shelf_B", new Vector3(0f, 0.75f, -4.2f), new Vector3(4f, 1.5f, 0.6f));

        // ---- دکور داخلی ----
        ArtKit.Box(art, "Stripe", new Vector3(13.8f, 0.15f, 0.06f), new Vector3(0f, 2.3f, -4.86f), ArtKit.Mat("MATS_Shop_CounterFront"));
        ArtKit.Box(art, "Poster1", new Vector3(1.2f, 1.6f, 0.04f), new Vector3(-3f, 1.8f, -4.86f), ArtKit.Mat("MATS_Sign_Orange"));
        ArtKit.Box(art, "Poster2", new Vector3(1.2f, 1.6f, 0.04f), new Vector3(3f, 1.8f, -4.86f), ArtKit.Mat("MATS_Sign_Green"));
        ArtKit.Box(art, "EntryMat", new Vector3(1.8f, 0.02f, 1.0f), new Vector3(0f, 0.011f, 4.2f), ArtKit.Mat("MATS_Room_Rug"));

        EditorSceneManager.MarkSceneDirty(shopScene);
        bool okShop = EditorSceneManager.SaveScene(shopScene);

        // ---------- 3) نمای بیرونی پاساژ (صحنه کوچه، ریشه جدا) ----------
        var alleyScene = OpenAdditive(AlleyPath);
        EditorSceneManager.SetActiveScene(alleyScene);
        var fac = ArtKit.EnsureArtRoot("ShopFacadeArt");
        var awning = ArtKit.Box(fac, "Awning_Passage", new Vector3(3f, 0.08f, 1.2f), new Vector3(0f, 2.95f, 12.5f), ArtKit.Mat("MATS_Alley_VendorTarp"));
        awning.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
        ArtKit.Box(fac, "Blade_Sign", new Vector3(0.12f, 1.8f, 0.7f), new Vector3(4.2f, 2.6f, 12.7f), ArtKit.Mat("MATS_Sign_Blue"));
        ArtKit.Box(fac, "DoorMat", new Vector3(2.4f, 0.02f, 1f), new Vector3(0f, 0.055f, 12.3f), ArtKit.Mat("MATS_Room_Rug"));

        EditorSceneManager.MarkSceneDirty(alleyScene);
        bool okAlley = EditorSceneManager.SaveScene(alleyScene);

        Debug.Log($"[TehranCity] Shop + facade art pass done. save shop={okShop} alley={okAlley} | dirtyAfter shop={shopScene.isDirty} alley={alleyScene.isDirty}");
        if (!okShop || !okAlley || shopScene.isDirty || alleyScene.isDirty)
            Debug.LogError("[ShopArt] ذخیره صحنه ناموفق بود! لاگ بالا را بررسی کن.");
    }

    private static void ShelfUnit(Transform parent, string name, Vector3 center, Vector3 size)
    {
        var t = new GameObject(name).transform; t.SetParent(parent, false);
        var frame = ArtKit.Mat("MATS_Shop_ShelfWhite");
        float w = size.x, d = size.z;
        ArtKit.Box(t, "SideL", new Vector3(0.06f, 1.5f, d), center + new Vector3(-(w / 2 - 0.03f), 0f, 0f), frame);
        ArtKit.Box(t, "SideR", new Vector3(0.06f, 1.5f, d), center + new Vector3((w / 2 - 0.03f), 0f, 0f), frame);
        ArtKit.Box(t, "Back", new Vector3(w, 1.5f, 0.05f), center + new Vector3(0f, 0f, -d / 2 + 0.025f), frame);
        float[] ys = { 0.25f, 0.75f, 1.25f };
        for (int i = 0; i < 3; i++)
            ArtKit.Box(t, $"Board{i}", new Vector3(w - 0.1f, 0.06f, d - 0.08f), center + new Vector3(0f, ys[i] - 0.72f, 0f), frame);
        string[] mats = { "MATS_Shop_BoxGreen", "MATS_Shop_BoxBlue", "MATS_Shop_BoxRed", "MATS_Shop_BoxYellow" };
        int n = Mathf.Max(2, Mathf.FloorToInt((w - 0.3f) / 0.35f));
        int k = 0;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < n; j++)
            {
                float x = center.x - (w - 0.3f) / 2f + j * 0.35f + 0.05f;
                ArtKit.Box(t, $"Prod{i}_{j}", new Vector3(0.24f, 0.22f, 0.3f),
                    new Vector3(x, ys[i] + 0.14f, center.z - 0.05f), ArtKit.Mat(mats[(k++) % 4]));
            }
    }

    private static Scene OpenAdditive(string path)
    {
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var s = EditorSceneManager.GetSceneAt(i);
            if (s.path == path) return s;
        }
        return EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
    }
}