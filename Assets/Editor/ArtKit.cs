using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// فاز A — کیت هنری به سبک Precinct (Low-Poly + رنگ تخت اشباع).
/// Setup 18 فقط پالت متریال می‌سازد؛ helperها برای Setup 19-21 استفاده می‌شوند.
/// کاملاً idempotent. صحنه‌ها را دست نمی‌زند.
/// </summary>
public static class ArtKit
{
    public const string MatDir = "Assets/_Project/Art/Environment/Narmak/Materials/Stylized";

    [MenuItem("TehranCity/Setup/18 Art Kit (Palette)")]
    public static void BuildPalette()
    {
        EnsureFolder(MatDir);
        int created = 0, existing = 0;
        foreach (var def in Palette())
        {
            string path = $"{MatDir}/{def.name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) { existing++; continue; }
            var mat = MakeMat(def);
            AssetDatabase.CreateAsset(mat, path);
            created++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[TehranCity] ArtKit palette ready: {created} created, {existing} existing. dir={MatDir}");
    }

    // ---------- helperها برای Setup 19-21 ----------

    public static Material Mat(string name)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>($"{MatDir}/{name}.mat");
        if (m == null)
            Debug.LogError($"[ArtKit] متریال پیدا نشد: {name} — اول Setup 18 را اجرا کن.");
        return m;
    }

    /// <summary>مکعب تزئینی: بدون کولایدر (فیزیک از greybox)، static برای بیک نور.</summary>
    public static GameObject Box(Transform parent, string name, Vector3 scale, Vector3 localPos, Material mat, bool keepCollider = false)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localScale = scale;
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = mat;
        go.isStatic = true;
        return go;
    }

    /// <summary>خاموش‌کردن رندرِ آبجکت‌های greybox (کولایدر می‌ماند).</summary>
    public static void HideRenderersByName(Transform root, params string[] names)
    {
        foreach (var n in names)
        {
            var t = FindChild(root, n);
            if (t == null) { Debug.LogWarning($"[ArtKit] Hide: '{n}' زیر {root.name} پیدا نشد."); continue; }
            foreach (var r in t.GetComponentsInChildren<Renderer>(true))
                r.enabled = false;
        }
    }

    /// <summary>تعویض متریال آبجکت موجود (درِ دارای SceneDoor، تابلوها و...).</summary>
    public static void ReskinByName(Transform root, string name, Material mat)
    {
        var t = FindChild(root, name);
        if (t == null) { Debug.LogWarning($"[ArtKit] Reskin: '{name}' زیر {root.name} پیدا نشد."); return; }
        var r = t.GetComponent<Renderer>();
        if (r != null && mat != null) r.sharedMaterial = mat;
    }

    /// <summary>ریشه آبجکت‌های تزئینی؛ اجرای قبلی را حذف می‌کند (idempotent).</summary>
    public static Transform EnsureArtRoot(string rootName)
    {
        var old = GameObject.Find(rootName);
        if (old != null) Object.DestroyImmediate(old);
        return new GameObject(rootName).transform;
    }

    public static Transform FindChild(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t)
        {
            var r = FindChild(c, name);
            if (r != null) return r;
        }
        return null;
    }

    // ---------- پالت ----------

    private struct MatDef
    {
        public string name; public Color color; public float smoothness;
        public Color emissive; public float emissiveIntensity;
        public MatDef(string n, Color c, float s = 0.05f)
        { name = n; color = c; smoothness = s; emissive = Color.black; emissiveIntensity = 0f; }
        public static MatDef Glow(string n, Color c, float intensity)
        { return new MatDef { name = n, color = c, smoothness = 0.2f, emissive = c, emissiveIntensity = intensity }; }
    }

    private static MatDef[] Palette() => new[]
    {
        // ---- اتاق ----
        new MatDef("MATS_Room_FloorWood",  new Color(0.55f,0.38f,0.24f)),
        new MatDef("MATS_Room_Wall",        new Color(0.93f,0.88f,0.78f)),
        new MatDef("MATS_Room_WallAccent",  new Color(0.35f,0.55f,0.55f)),
        new MatDef("MATS_Room_Ceiling",     new Color(0.95f,0.95f,0.92f)),
        new MatDef("MATS_Room_BedFrame",    new Color(0.35f,0.22f,0.14f)),
        new MatDef("MATS_Room_Mattress",    new Color(0.92f,0.92f,0.88f)),
        new MatDef("MATS_Room_Blanket",     new Color(0.10f,0.48f,0.55f)),
        new MatDef("MATS_Room_Pillow",      new Color(0.95f,0.95f,0.95f)),
        new MatDef("MATS_Room_Rug",         new Color(0.55f,0.13f,0.15f)),
        new MatDef("MATS_Room_RugBorder",   new Color(0.20f,0.10f,0.10f)),
        new MatDef("MATS_Room_Wood",        new Color(0.48f,0.33f,0.20f)),
        new MatDef("MATS_Room_WoodDark",    new Color(0.30f,0.20f,0.12f)),
        new MatDef("MATS_Room_Metal",       new Color(0.25f,0.25f,0.28f), 0.4f),
        new MatDef("MATS_Room_Mirror",      new Color(0.75f,0.88f,0.92f), 0.85f),
        MatDef.Glow("MATS_Glow_Screen",     new Color(0.35f,0.75f,0.90f), 1.5f),
        MatDef.Glow("MATS_Glow_LampWarm",   new Color(1.00f,0.80f,0.50f), 1.2f),

        // ---- کوچه ----
        new MatDef("MATS_Alley_Asphalt",      new Color(0.20f,0.20f,0.23f)),
        new MatDef("MATS_Alley_Sidewalk",     new Color(0.52f,0.50f,0.46f)),
        new MatDef("MATS_Alley_Curb",         new Color(0.62f,0.60f,0.55f)),
        new MatDef("MATS_Alley_BuildingCream",new Color(0.80f,0.72f,0.58f)),
        new MatDef("MATS_Alley_BuildingBrick",new Color(0.66f,0.40f,0.32f)),
        new MatDef("MATS_Alley_BuildingBlue", new Color(0.45f,0.55f,0.62f)),
        new MatDef("MATS_Alley_BuildingSand", new Color(0.72f,0.62f,0.45f)),
        new MatDef("MATS_Alley_DoorWood",     new Color(0.33f,0.20f,0.12f)),
        new MatDef("MATS_Alley_Shutter",      new Color(0.55f,0.57f,0.60f), 0.5f),
        new MatDef("MATS_Alley_WindowDark",   new Color(0.10f,0.15f,0.20f), 0.7f),
        new MatDef("MATS_Alley_CarRed",       new Color(0.70f,0.12f,0.12f), 0.6f),
        new MatDef("MATS_Alley_CarGlass",     new Color(0.10f,0.15f,0.22f), 0.8f),
        new MatDef("MATS_Alley_Tire",         new Color(0.10f,0.10f,0.10f)),
        new MatDef("MATS_Alley_VendorTarp",   new Color(0.12f,0.35f,0.60f)),
        new MatDef("MATS_Alley_VendorWood",   new Color(0.45f,0.32f,0.20f)),
        new MatDef("MATS_Alley_TreeLeaf",     new Color(0.20f,0.48f,0.24f)),
        new MatDef("MATS_Alley_TreeTrunk",    new Color(0.35f,0.25f,0.15f)),
        new MatDef("MATS_Alley_ACUnit",       new Color(0.75f,0.75f,0.75f), 0.4f),
        new MatDef("MATS_Sign_Green",         new Color(0.08f,0.42f,0.22f)),
        new MatDef("MATS_Sign_Blue",          new Color(0.10f,0.28f,0.60f)),
        new MatDef("MATS_Sign_Orange",        new Color(0.85f,0.45f,0.08f)),

        // ---- پاساژ/مغازه ----
        new MatDef("MATS_Shop_FloorTile",     new Color(0.72f,0.72f,0.76f), 0.3f),
        new MatDef("MATS_Shop_Wall",          new Color(0.90f,0.90f,0.87f)),
        new MatDef("MATS_Shop_CounterFront",  new Color(0.12f,0.35f,0.65f)),
        new MatDef("MATS_Shop_CounterTop",    new Color(0.92f,0.92f,0.92f), 0.4f),
        new MatDef("MATS_Shop_ShelfWhite",    new Color(0.88f,0.88f,0.90f)),
        new MatDef("MATS_Shop_BoxGreen",      new Color(0.15f,0.55f,0.25f)),
        new MatDef("MATS_Shop_BoxBlue",       new Color(0.15f,0.35f,0.70f)),
        new MatDef("MATS_Shop_BoxRed",        new Color(0.70f,0.18f,0.15f)),
        new MatDef("MATS_Shop_BoxYellow",     new Color(0.85f,0.65f,0.10f)),
        new MatDef("MATS_Shop_Glass",         new Color(0.60f,0.80f,0.85f), 0.9f),
    };

    private static Material MakeMat(MatDef d)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.name = d.name;
        mat.SetColor("_BaseColor", d.color);
        mat.SetFloat("_Smoothness", d.smoothness);
        if (d.emissiveIntensity > 0f)
        {
            mat.SetColor("_EmissionColor", d.emissive * d.emissiveIntensity);
            mat.EnableKeyword("_EMISSION");
        }
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