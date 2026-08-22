using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// فاز A / قدم A2 — Art Pass اتاق به سبک Precinct (رنگ تخت، Low-Poly).
/// کولایدرهای greybox دست‌نخورده می‌مانند؛ فقط ظاهر عوض می‌شود.
/// کاملاً idempotent.
/// </summary>
public static class RoomArtPass
{
    private const string ScenePath = "Assets/_Project/Scenes/Interior_Room01.unity";

    [MenuItem("TehranCity/Setup/19 Room Art Pass (Precinct Style)")]
    public static void Run()
    {
        EditorSceneManager.SaveOpenScenes();
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var grey = GameObject.Find("GreyboxRoom");
        if (grey == null)
        {
            Debug.LogError("[RoomArt] GreyboxRoom پیدا نشد؛ اول Setup 4 را اجرا کن.");
            return;
        }
        var gt = grey.transform;

        // ---------- 1) Reskin پوسته (کولایدرها همان‌ها می‌مانند) ----------
        ArtKit.ReskinByName(gt, "Floor", ArtKit.Mat("MATS_Room_FloorWood"));
        ArtKit.ReskinByName(gt, "Wall_N", ArtKit.Mat("MATS_Room_WallAccent"));
        foreach (var n in new[] { "Wall_E", "Wall_W", "Wall_S_A", "Wall_S_B", "Wall_S_DoorTop" })
            ArtKit.ReskinByName(gt, n, ArtKit.Mat("MATS_Room_Wall"));

        var doorExit = GameObject.Find("Door_Exit");
        if (doorExit != null)
        {
            var dr = doorExit.GetComponent<Renderer>();
            if (dr != null) dr.sharedMaterial = ArtKit.Mat("MATS_Alley_DoorWood");
        }
        ArtKit.ReskinByName(gt, "Interact_Phone", ArtKit.Mat("MATS_Room_Metal"));

        // ---------- 2) Hide مبلمان تک‌مکعبی؛ نسخه استایلیزه جایگزین ----------
        ArtKit.HideRenderersByName(gt, "Bed", "Pillow", "Table", "Wardrobe", "Mirror");

        var art = ArtKit.EnsureArtRoot("RoomArt");

        // ---- تخت ----
        var bed = new GameObject("Art_Bed").transform;
        bed.SetParent(art, false);
        ArtKit.Box(bed, "Frame", new Vector3(0.95f, 0.25f, 2.05f), new Vector3(-1.6f, 0.125f, 1.5f), ArtKit.Mat("MATS_Room_BedFrame"));
        ArtKit.Box(bed, "Headboard", new Vector3(0.95f, 0.55f, 0.08f), new Vector3(-1.6f, 0.45f, 2.52f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(bed, "Mattress", new Vector3(0.85f, 0.22f, 1.90f), new Vector3(-1.6f, 0.36f, 1.45f), ArtKit.Mat("MATS_Room_Mattress"));
        ArtKit.Box(bed, "Pillow", new Vector3(0.60f, 0.14f, 0.35f), new Vector3(-1.6f, 0.52f, 2.15f), ArtKit.Mat("MATS_Room_Pillow"));
        ArtKit.Box(bed, "Blanket", new Vector3(0.87f, 0.12f, 1.10f), new Vector3(-1.6f, 0.46f, 1.00f), ArtKit.Mat("MATS_Room_Blanket"));

        // ---- میز + لامپ رومیزی ----
        var table = new GameObject("Art_Table").transform;
        table.SetParent(art, false);
        ArtKit.Box(table, "Top", new Vector3(1.2f, 0.06f, 0.6f), new Vector3(1.4f, 0.71f, -1.8f), ArtKit.Mat("MATS_Room_Wood"));
        ArtKit.Box(table, "Leg1", new Vector3(0.08f, 0.68f, 0.08f), new Vector3(0.9f, 0.34f, -2.02f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(table, "Leg2", new Vector3(0.08f, 0.68f, 0.08f), new Vector3(1.9f, 0.34f, -2.02f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(table, "Leg3", new Vector3(0.08f, 0.68f, 0.08f), new Vector3(0.9f, 0.34f, -1.58f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(table, "Leg4", new Vector3(0.08f, 0.68f, 0.08f), new Vector3(1.9f, 0.34f, -1.58f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(table, "LampPost", new Vector3(0.05f, 0.22f, 0.05f), new Vector3(1.85f, 0.85f, -1.95f), ArtKit.Mat("MATS_Room_Metal"));
        ArtKit.Box(table, "LampHead", new Vector3(0.14f, 0.09f, 0.14f), new Vector3(1.85f, 1.00f, -1.95f), ArtKit.Mat("MATS_Glow_LampWarm"));

        // ---- صفحه روشن گوشی (فرزندِ والد بدون scale تا ابعاد واقعی بماند) ----
        ArtKit.Box(art, "Art_PhoneScreen", new Vector3(0.07f, 0.012f, 0.12f), new Vector3(1.4f, 0.776f, -1.8f), ArtKit.Mat("MATS_Glow_Screen"));

        // ---- کمد لباس ----
        var ward = new GameObject("Art_Wardrobe").transform;
        ward.SetParent(art, false);
        ArtKit.Box(ward, "Body", new Vector3(0.8f, 1.8f, 0.6f), new Vector3(1.6f, 0.9f, 1.9f), ArtKit.Mat("MATS_Room_Wood"));
        ArtKit.Box(ward, "Panel", new Vector3(0.74f, 1.66f, 0.04f), new Vector3(1.6f, 0.9f, 1.58f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(ward, "Handle1", new Vector3(0.04f, 0.18f, 0.04f), new Vector3(1.52f, 1.0f, 1.55f), ArtKit.Mat("MATS_Room_Metal"));
        ArtKit.Box(ward, "Handle2", new Vector3(0.04f, 0.18f, 0.04f), new Vector3(1.68f, 1.0f, 1.55f), ArtKit.Mat("MATS_Room_Metal"));

        // ---- آینه قاب‌دار ----
        var mir = new GameObject("Art_Mirror").transform;
        mir.SetParent(art, false);
        ArtKit.Box(mir, "Frame", new Vector3(0.6f, 1.3f, 0.05f), new Vector3(-0.8f, 1.6f, 2.61f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(mir, "Glass", new Vector3(0.5f, 1.2f, 0.05f), new Vector3(-0.8f, 1.6f, 2.58f), ArtKit.Mat("MATS_Room_Mirror"));

        // ---- فرش ----
        ArtKit.Box(art, "Art_RugBorder", new Vector3(1.8f, 0.02f, 2.4f), new Vector3(0f, 0.010f, 0.2f), ArtKit.Mat("MATS_Room_RugBorder"));
        ArtKit.Box(art, "Art_Rug", new Vector3(1.6f, 0.03f, 2.2f), new Vector3(0f, 0.016f, 0.2f), ArtKit.Mat("MATS_Room_Rug"));

        // ---- پنجره دیوار غربی ----
        var win = new GameObject("Art_Window").transform;
        win.SetParent(art, false);
        ArtKit.Box(win, "Frame", new Vector3(0.06f, 1.4f, 1.6f), new Vector3(-2.08f, 1.7f, 0.3f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(win, "Glass", new Vector3(0.05f, 1.2f, 1.4f), new Vector3(-2.06f, 1.7f, 0.3f), ArtKit.Mat("MATS_Room_Mirror"));
        ArtKit.Box(win, "BarV", new Vector3(0.07f, 1.2f, 0.06f), new Vector3(-2.05f, 1.7f, 0.3f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(win, "BarH", new Vector3(0.07f, 0.06f, 1.4f), new Vector3(-2.05f, 1.7f, 0.3f), ArtKit.Mat("MATS_Room_WoodDark"));

        // ---- پوسترها (دیوار شرقی) ----
        ArtKit.Box(art, "Art_Poster1", new Vector3(0.04f, 0.7f, 0.5f), new Vector3(2.08f, 1.8f, 0.2f), ArtKit.Mat("MATS_Sign_Orange"));
        ArtKit.Box(art, "Art_Poster2", new Vector3(0.04f, 0.5f, 0.4f), new Vector3(2.08f, 1.7f, 1.0f), ArtKit.Mat("MATS_Sign_Green"));

        // ---- قاب دور Door_Exit ----
        var dtrim = new GameObject("Art_DoorTrim").transform;
        dtrim.SetParent(art, false);
        ArtKit.Box(dtrim, "Left", new Vector3(0.12f, 2.3f, 0.22f), new Vector3(-0.56f, 1.15f, -2.7f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(dtrim, "Right", new Vector3(0.12f, 2.3f, 0.22f), new Vector3(0.56f, 1.15f, -2.7f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(dtrim, "Top", new Vector3(1.24f, 0.12f, 0.22f), new Vector3(0f, 2.31f, -2.7f), ArtKit.Mat("MATS_Room_WoodDark"));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[TehranCity] Room art pass done (Precinct style). Colliders untouched.");
    }
}