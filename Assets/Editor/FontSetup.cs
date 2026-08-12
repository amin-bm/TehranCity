using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class FontSetup
{
    private const string FontDir = "Assets/_Project/Art/UI/Fonts";
    private const string FontAssetPath = FontDir + "/TMP_Tahoma_Persian.asset";

    [MenuItem("TehranCity/Setup/7) Setup Persian TMP Font (Dynamic)")]
    public static void Setup()
    {
        Directory.CreateDirectory(FontDir);

        string dst = FontDir + "/tahoma.ttf";
        if (!File.Exists(dst))
        {
            const string src = @"C:\Windows\Fonts\tahoma.ttf";
            if (!File.Exists(src)) { Debug.LogError("[TehranCity] tahoma.ttf not found on Windows."); return; }
            File.Copy(src, dst);
        }
        AssetDatabase.ImportAsset(dst);
        var font = AssetDatabase.LoadAssetAtPath<Font>(dst);
        if (font == null) { Debug.LogError("[TehranCity] Font import failed."); return; }

        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath) != null)
        {
            Debug.Log("[TehranCity] Font asset already exists.");
            return;
        }

        var fa = TMP_FontAsset.CreateFontAsset(font);
        fa.atlasPopulationMode = AtlasPopulationMode.Dynamic; // گلیف‌های فارسی on-the-fly ساخته می‌شوند
        AssetDatabase.CreateAsset(fa, FontAssetPath);
        if (fa.material != null) AssetDatabase.AddObjectToAsset(fa.material, fa);
        if (fa.atlasTextures != null && fa.atlasTextures.Length > 0)
            AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
        AssetDatabase.SaveAssets();
        Debug.Log("[TehranCity] Persian dynamic TMP font ready: " + FontAssetPath);
    }
}