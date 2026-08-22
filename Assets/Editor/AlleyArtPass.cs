using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// فاز A / قدم A3 — Art Pass کوچه نارمک به سبک Precinct.
/// کولایدرهای AlleyRoot دست‌نخورده؛ فقط ظاهر. کاملاً idempotent.
/// </summary>
public static class AlleyArtPass
{
    private const string AlleyPath = "Assets/_Project/Scenes/Prototype_Block01.unity";

    [MenuItem("TehranCity/Setup/20 Alley Art Pass (Precinct Style)")]
    public static void Run()
    {
        EditorSceneManager.SaveOpenScenes();
        var scene = EditorSceneManager.OpenScene(AlleyPath);
        var rootGo = GameObject.Find("AlleyRoot");
        if (rootGo == null)
        {
            Debug.LogError("[AlleyArt] AlleyRoot پیدا نشد؛ اول Setup 11 را اجرا کن.");
            return;
        }
        var rt = rootGo.transform;

        // ---------- 1) Reskin حجم‌های موجود (کولایدرها همان‌ها) ----------
        ArtKit.ReskinByName(rt, "Ground", ArtKit.Mat("MATS_Alley_Asphalt"));
        ArtKit.ReskinByName(rt, "Building_Room", ArtKit.Mat("MATS_Alley_BuildingCream"));
        ArtKit.ReskinByName(rt, "Building_W", ArtKit.Mat("MATS_Alley_BuildingBrick"));
        ArtKit.ReskinByName(rt, "Building_E", ArtKit.Mat("MATS_Alley_BuildingBlue"));
        ArtKit.ReskinByName(rt, "Facade_Supermarket", ArtKit.Mat("MATS_Alley_BuildingSand"));
        ArtKit.ReskinByName(rt, "Facade_Passage", ArtKit.Mat("MATS_Alley_BuildingCream"));
        ArtKit.ReskinByName(rt, "Facade_TinyShop", ArtKit.Mat("MATS_Alley_BuildingBrick"));
        ArtKit.ReskinByName(rt, "Door_Back", ArtKit.Mat("MATS_Alley_DoorWood"));
        ArtKit.ReskinByName(rt, "Door_Passage", ArtKit.Mat("MATS_Alley_DoorWood"));
        ArtKit.ReskinByName(rt, "Door_Supermarket", ArtKit.Mat("MATS_Alley_Shutter"));
        ArtKit.ReskinByName(rt, "Door_TinyShop", ArtKit.Mat("MATS_Alley_Shutter"));
        ArtKit.ReskinByName(rt, "Car_Body", ArtKit.Mat("MATS_Alley_CarRed"));
        ArtKit.ReskinByName(rt, "Car_Top", ArtKit.Mat("MATS_Alley_CarGlass"));
        ArtKit.ReskinByName(rt, "Vendor_Table", ArtKit.Mat("MATS_Alley_VendorWood"));
        ArtKit.ReskinByName(rt, "Sign_Supermarket", ArtKit.Mat("MATS_Sign_Green"));
        ArtKit.ReskinByName(rt, "Sign_Passage", ArtKit.Mat("MATS_Sign_Blue"));
        ArtKit.ReskinByName(rt, "Sign_TinyShop", ArtKit.Mat("MATS_Sign_Orange"));

        var art = ArtKit.EnsureArtRoot("AlleyArt");

        // ---------- 2) پیاده‌رو + جدول + خط‌کشی ----------
        var walk = new GameObject("Art_Walks").transform; walk.SetParent(art, false);
        ArtKit.Box(walk, "Side_W", new Vector3(1.2f, 0.04f, 14f), new Vector3(-2.4f, 0.02f, -6f), ArtKit.Mat("MATS_Alley_Sidewalk"));
        ArtKit.Box(walk, "Side_E", new Vector3(1.2f, 0.04f, 14f), new Vector3(2.4f, 0.02f, -6f), ArtKit.Mat("MATS_Alley_Sidewalk"));
        ArtKit.Box(walk, "Curb_W", new Vector3(0.15f, 0.06f, 14f), new Vector3(-1.75f, 0.03f, -6f), ArtKit.Mat("MATS_Alley_Curb"));
        ArtKit.Box(walk, "Curb_E", new Vector3(0.15f, 0.06f, 14f), new Vector3(1.75f, 0.03f, -6f), ArtKit.Mat("MATS_Alley_Curb"));
        ArtKit.Box(walk, "Front", new Vector3(24f, 0.04f, 1.5f), new Vector3(0f, 0.02f, 12.25f), ArtKit.Mat("MATS_Alley_Sidewalk"));
        ArtKit.Box(walk, "FrontCurb", new Vector3(24f, 0.06f, 0.15f), new Vector3(0f, 0.03f, 11.42f), ArtKit.Mat("MATS_Alley_Curb"));
        for (int i = 0; i < 5; i++)
            ArtKit.Box(walk, $"Cross_{i}", new Vector3(0.5f, 0.02f, 1.6f), new Vector3(-2f + i, 0.015f, 10.4f), ArtKit.Mat("MATS_Shop_CounterTop"));

        // ---------- 3) پاراپت بام‌ها ----------
        var par = new GameObject("Art_Parapets").transform; par.SetParent(art, false);
        ArtKit.Box(par, "P_Room", new Vector3(6.2f, 0.3f, 6.2f), new Vector3(0f, 4.1f, -6f), ArtKit.Mat("MATS_Alley_Curb"));
        ArtKit.Box(par, "P_W", new Vector3(4.2f, 0.3f, 14.2f), new Vector3(-5f, 5.1f, -6f), ArtKit.Mat("MATS_Alley_Curb"));
        ArtKit.Box(par, "P_E", new Vector3(4.2f, 0.3f, 14.2f), new Vector3(5f, 4.1f, -6f), ArtKit.Mat("MATS_Alley_Curb"));
        ArtKit.Box(par, "P_Sup", new Vector3(6.2f, 0.3f, 4.2f), new Vector3(-8f, 3.1f, 15f), ArtKit.Mat("MATS_Alley_Curb"));
        ArtKit.Box(par, "P_Pas", new Vector3(8.2f, 0.3f, 4.2f), new Vector3(0f, 4.1f, 15f), ArtKit.Mat("MATS_Alley_Curb"));
        ArtKit.Box(par, "P_Tiny", new Vector3(5.2f, 0.3f, 4.2f), new Vector3(8f, 3.1f, 15f), ArtKit.Mat("MATS_Alley_Curb"));

        // ---------- 4) پنجره‌ها (کمی جلوتر از نما؛ بدون فاصله صفر برای پرهیز از z-fight) ----------
        var win = new GameObject("Art_Windows").transform; win.SetParent(art, false);
        // نمای سوپرمارکت / پاساژ / مغازه کوچک (رو به -z)
        ArtKit.Box(win, "W_Sup1", new Vector3(1.4f, 1.0f, 0.08f), new Vector3(-9.7f, 2.0f, 12.94f), ArtKit.Mat("MATS_Alley_WindowDark"));
        ArtKit.Box(win, "W_Sup2", new Vector3(1.4f, 1.0f, 0.08f), new Vector3(-6.3f, 2.0f, 12.94f), ArtKit.Mat("MATS_Alley_WindowDark"));
        ArtKit.Box(win, "W_Pas1", new Vector3(1.2f, 1.2f, 0.08f), new Vector3(-2.8f, 2.4f, 12.94f), ArtKit.Mat("MATS_Alley_WindowDark"));
        ArtKit.Box(win, "W_Pas2", new Vector3(1.2f, 1.2f, 0.08f), new Vector3(2.8f, 2.4f, 12.94f), ArtKit.Mat("MATS_Alley_WindowDark"));
        ArtKit.Box(win, "W_Tin1", new Vector3(1.4f, 1.0f, 0.08f), new Vector3(6.3f, 2.0f, 12.94f), ArtKit.Mat("MATS_Alley_WindowDark"));
        ArtKit.Box(win, "W_Tin2", new Vector3(1.4f, 1.0f, 0.08f), new Vector3(9.7f, 2.0f, 12.94f), ArtKit.Mat("MATS_Alley_WindowDark"));
        // دو طرف کوچه (رو به کوچه)
        foreach (var z in new[] { -10f, -6f, -2f })
        {
            ArtKit.Box(win, $"W_W_{z}", new Vector3(0.08f, 1.0f, 1.2f), new Vector3(-2.95f, 1.8f, z), ArtKit.Mat("MATS_Alley_WindowDark"));
            ArtKit.Box(win, $"W_W2_{z}", new Vector3(0.08f, 1.0f, 1.2f), new Vector3(-2.95f, 3.2f, z), ArtKit.Mat("MATS_Alley_WindowDark"));
            ArtKit.Box(win, $"W_E_{z}", new Vector3(0.08f, 1.0f, 1.2f), new Vector3(2.95f, 1.6f, z), ArtKit.Mat("MATS_Alley_WindowDark"));
            ArtKit.Box(win, $"W_E2_{z}", new Vector3(0.08f, 1.0f, 1.2f), new Vector3(2.95f, 2.9f, z), ArtKit.Mat("MATS_Alley_WindowDark"));
        }
        // نمای ساختمان اتاق + سایبان کوچک بالای درب
        ArtKit.Box(win, "W_Room1", new Vector3(1.2f, 1.0f, 0.08f), new Vector3(-1.8f, 2.2f, -2.96f), ArtKit.Mat("MATS_Alley_WindowDark"));
        ArtKit.Box(win, "W_Room2", new Vector3(1.2f, 1.0f, 0.08f), new Vector3(1.8f, 2.2f, -2.96f), ArtKit.Mat("MATS_Alley_WindowDark"));
        ArtKit.Box(win, "Awning_Room", new Vector3(1.4f, 0.08f, 0.6f), new Vector3(0f, 2.35f, -2.7f), ArtKit.Mat("MATS_Alley_VendorTarp"));

        // ---------- 5) کولرهای دیواری ----------
        var ac = new GameObject("Art_AC").transform; ac.SetParent(art, false);
        ArtKit.Box(ac, "AC_E1", new Vector3(0.5f, 0.4f, 0.6f), new Vector3(2.8f, 2.2f, -2f), ArtKit.Mat("MATS_Alley_ACUnit"));
        ArtKit.Box(ac, "AC_E2", new Vector3(0.5f, 0.4f, 0.6f), new Vector3(2.8f, 1.5f, -9f), ArtKit.Mat("MATS_Alley_ACUnit"));
        ArtKit.Box(ac, "AC_W1", new Vector3(0.5f, 0.4f, 0.6f), new Vector3(-2.8f, 2.6f, -4f), ArtKit.Mat("MATS_Alley_ACUnit"));
        ArtKit.Box(ac, "AC_W2", new Vector3(0.5f, 0.4f, 0.6f), new Vector3(-2.8f, 1.8f, -10f), ArtKit.Mat("MATS_Alley_ACUnit"));

        // ---------- 6) ماشین: چرخ + چراغ‌ها ----------
        var car = new GameObject("Art_Car").transform; car.SetParent(art, false);
        Cyl(car, "Wheel_FL", new Vector3(0.6f, 0.1f, 0.6f), new Vector3(3.1f, 0.3f, 6.4f), ArtKit.Mat("MATS_Alley_Tire"), 90f);
        Cyl(car, "Wheel_FR", new Vector3(0.6f, 0.1f, 0.6f), new Vector3(4.9f, 0.3f, 6.4f), ArtKit.Mat("MATS_Alley_Tire"), 90f);
        Cyl(car, "Wheel_BL", new Vector3(0.6f, 0.1f, 0.6f), new Vector3(3.1f, 0.3f, 3.6f), ArtKit.Mat("MATS_Alley_Tire"), 90f);
        Cyl(car, "Wheel_BR", new Vector3(0.6f, 0.1f, 0.6f), new Vector3(4.9f, 0.3f, 3.6f), ArtKit.Mat("MATS_Alley_Tire"), 90f);
        ArtKit.Box(car, "Light_F1", new Vector3(0.2f, 0.15f, 0.06f), new Vector3(3.4f, 0.45f, 7.12f), ArtKit.Mat("MATS_Shop_CounterTop"));
        ArtKit.Box(car, "Light_F2", new Vector3(0.2f, 0.15f, 0.06f), new Vector3(4.6f, 0.45f, 7.12f), ArtKit.Mat("MATS_Shop_CounterTop"));
        ArtKit.Box(car, "Light_B1", new Vector3(0.2f, 0.15f, 0.06f), new Vector3(3.4f, 0.45f, 2.88f), ArtKit.Mat("MATS_Shop_BoxRed"));
        ArtKit.Box(car, "Light_B2", new Vector3(0.2f, 0.15f, 0.06f), new Vector3(4.6f, 0.45f, 2.88f), ArtKit.Mat("MATS_Shop_BoxRed"));

        // ---------- 7) دستفروش: سایبان + جنس ----------
        var ven = new GameObject("Art_Vendor").transform; ven.SetParent(art, false);
        ArtKit.Box(ven, "Pole1", new Vector3(0.05f, 1.6f, 0.05f), new Vector3(-4.7f, 0.8f, 3.65f), ArtKit.Mat("MATS_Alley_VendorWood"));
        ArtKit.Box(ven, "Pole2", new Vector3(0.05f, 1.6f, 0.05f), new Vector3(-3.3f, 0.8f, 3.65f), ArtKit.Mat("MATS_Alley_VendorWood"));
        ArtKit.Box(ven, "Pole3", new Vector3(0.05f, 1.6f, 0.05f), new Vector3(-4.7f, 0.8f, 4.35f), ArtKit.Mat("MATS_Alley_VendorWood"));
        ArtKit.Box(ven, "Pole4", new Vector3(0.05f, 1.6f, 0.05f), new Vector3(-3.3f, 0.8f, 4.35f), ArtKit.Mat("MATS_Alley_VendorWood"));
        ArtKit.Box(ven, "Canopy", new Vector3(1.8f, 0.06f, 1.1f), new Vector3(-4f, 1.63f, 4f), ArtKit.Mat("MATS_Alley_VendorTarp"));
        ArtKit.Box(ven, "Goods1", new Vector3(0.3f, 0.2f, 0.3f), new Vector3(-4.4f, 0.9f, 4f), ArtKit.Mat("MATS_Shop_BoxGreen"));
        ArtKit.Box(ven, "Goods2", new Vector3(0.3f, 0.2f, 0.3f), new Vector3(-4.0f, 0.9f, 4.1f), ArtKit.Mat("MATS_Shop_BoxRed"));
        ArtKit.Box(ven, "Goods3", new Vector3(0.3f, 0.2f, 0.3f), new Vector3(-3.6f, 0.9f, 3.9f), ArtKit.Mat("MATS_Shop_BoxYellow"));

        // ---------- 8) درخت‌ها (تنه با کولایدر؛ شاخه بدون کولایدر) ----------
        var tree = new GameObject("Art_Trees").transform; tree.SetParent(art, false);
        Tree(tree, "Tree_L", new Vector3(-5.5f, 0f, 11.7f));
        Tree(tree, "Tree_R", new Vector3(5.5f, 0f, 11.7f));

        // ---------- 9) تیر چراغ ----------
        var lamp = new GameObject("Art_Lamps").transform; lamp.SetParent(art, false);
        StreetLamp(lamp, "Lamp_1", new Vector3(2.3f, 0f, 2f), -1f);
        StreetLamp(lamp, "Lamp_2", new Vector3(-2.3f, 0f, -4f), 1f);

        // ---------- 10) قاب دور درها ----------
        var trim = new GameObject("Art_DoorTrims").transform; trim.SetParent(art, false);
        ArtKit.Box(trim, "Pas_L", new Vector3(0.15f, 2.7f, 0.25f), new Vector3(-1.1f, 1.35f, 12.9f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(trim, "Pas_R", new Vector3(0.15f, 2.7f, 0.25f), new Vector3(1.1f, 1.35f, 12.9f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(trim, "Pas_T", new Vector3(2.4f, 0.15f, 0.25f), new Vector3(0f, 2.72f, 12.9f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(trim, "Back_L", new Vector3(0.12f, 2.3f, 0.22f), new Vector3(-0.56f, 1.15f, -2.9f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(trim, "Back_R", new Vector3(0.12f, 2.3f, 0.22f), new Vector3(0.56f, 1.15f, -2.9f), ArtKit.Mat("MATS_Room_WoodDark"));
        ArtKit.Box(trim, "Back_T", new Vector3(1.24f, 0.12f, 0.22f), new Vector3(0f, 2.31f, -2.9f), ArtKit.Mat("MATS_Room_WoodDark"));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[TehranCity] Alley art pass done (Precinct style). Colliders untouched.");
    }

    private static void Tree(Transform parent, string name, Vector3 pos)
    {
        var t = new GameObject(name).transform; t.SetParent(parent, false);
        Cyl(t, "Trunk", new Vector3(0.24f, 0.6f, 0.24f), pos + new Vector3(0f, 0.6f, 0f), ArtKit.Mat("MATS_Alley_TreeTrunk"), 0f, true);
        ArtKit.Box(t, "Leaf1", new Vector3(1.1f, 0.9f, 1.1f), pos + new Vector3(0f, 1.5f, 0f), ArtKit.Mat("MATS_Alley_TreeLeaf"));
        ArtKit.Box(t, "Leaf2", new Vector3(0.75f, 0.6f, 0.75f), pos + new Vector3(0f, 2.15f, 0f), ArtKit.Mat("MATS_Alley_TreeLeaf"));
    }

    private static void StreetLamp(Transform parent, string name, Vector3 pos, float armDirX)
    {
        var t = new GameObject(name).transform; t.SetParent(parent, false);
        ArtKit.Box(t, "Pole", new Vector3(0.08f, 3f, 0.08f), pos + new Vector3(0f, 1.5f, 0f), ArtKit.Mat("MATS_Room_Metal"), true);
        ArtKit.Box(t, "Arm", new Vector3(0.5f, 0.06f, 0.06f), pos + new Vector3(armDirX * 0.2f, 2.95f, 0f), ArtKit.Mat("MATS_Room_Metal"));
        ArtKit.Box(t, "Head", new Vector3(0.18f, 0.1f, 0.18f), pos + new Vector3(armDirX * 0.4f, 2.9f, 0f), ArtKit.Mat("MATS_Glow_LampWarm"));
    }

    private static GameObject Cyl(Transform parent, string name, Vector3 scale, Vector3 pos, Material mat, float rotZ = 0f, bool keepCollider = false)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localScale = scale;
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(0f, 0f, rotZ);
        if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = mat;
        go.isStatic = true;
        return go;
    }
}