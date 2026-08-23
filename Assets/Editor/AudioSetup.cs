using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// فاز A / قدم A6 — WAVهای placeholder در ساختار مصوب + AudioBank + AudioListener.
/// v4: پسوند .wav روی همه مسیرهای Emit + پاکسازی فایل‌های بدون‌پسوند قبلی.
/// </summary>
public static class AudioSetup
{
    private const string SfxDir = "Assets/_Project/Audio/SFX";
    private const string AmbDir = "Assets/_Project/Audio/Ambience";
    private const string BankPath = "Assets/_Project/Resources/AudioBank.asset";
    private const string LegacyDir = "Assets/_Project/Resources/Audio";
    private const string RoomPath = "Assets/_Project/Scenes/Interior_Room01.unity";
    private const int Hz = 22050;

    private static readonly string[] SfxNames = { "SFX_Footstep", "SFX_Interact", "SFX_Coin", "SFX_Spend", "SFX_Whoosh", "SFX_Notif" };
    private static readonly string[] AmbNames = { "AMB_Room", "AMB_Alley", "AMB_Shop" };

    [MenuItem("TehranCity/Setup/23 Audio Placeholder (Procedural WAVs)")]
    public static void Run()
    {
        // 0) پاکسازی مسیر اشتباه قبلی (پوشه + متای سرگردان)
        if (AssetDatabase.IsValidFolder(LegacyDir))
            AssetDatabase.DeleteAsset(LegacyDir);
        if (Directory.Exists(LegacyDir))
            FileUtil.DeleteFileOrDirectory(LegacyDir);
        if (File.Exists(LegacyDir + ".meta"))
            File.Delete(LegacyDir + ".meta");

        // 0.5) حذف فایل‌های بدون‌پسوندِ ساخته‌شده توسط نسخه‌های باگ‌دار
        foreach (var n in SfxNames) DeleteStray($"{SfxDir}/{n}");
        foreach (var n in AmbNames) DeleteStray($"{AmbDir}/{n}");

        Directory.CreateDirectory(SfxDir);
        Directory.CreateDirectory(AmbDir);

        int made = 0, skipped = 0;
        made += Emit($"{SfxDir}/SFX_Footstep.wav", GenFootstep(), ref skipped);
        made += Emit($"{SfxDir}/SFX_Interact.wav", GenTone(0.07f, (t, u) => Mathf.Sin(2f * Mathf.PI * (650f + 900f * t) * t) * Mathf.Exp(-t * 45f) * 0.5f), ref skipped);
        made += Emit($"{SfxDir}/SFX_Coin.wav", GenCoin(), ref skipped);
        made += Emit($"{SfxDir}/SFX_Spend.wav", GenTone(0.18f, (t, u) => Mathf.Sin(2f * Mathf.PI * (320f - 120f * u) * t) * Mathf.Exp(-t * 22f) * 0.5f), ref skipped);
        made += Emit($"{SfxDir}/SFX_Whoosh.wav", GenTone(0.35f, (t, u) => White(u * 999f) * Mathf.Sin(Mathf.PI * u) * 0.35f), ref skipped);
        made += Emit($"{SfxDir}/SFX_Notif.wav", GenNotif(), ref skipped);
        made += Emit($"{AmbDir}/AMB_Room.wav", MakeLoop(GenAmb(2f, (t, b) => b * 0.05f)), ref skipped);
        made += Emit($"{AmbDir}/AMB_Alley.wav", MakeLoop(GenAmb(4f, (t, b) => b * (0.09f + 0.04f * Mathf.Sin(2f * Mathf.PI * 0.25f * t)) + 0.008f * Mathf.Sin(2f * Mathf.PI * 55f * t))), ref skipped);
        made += Emit($"{AmbDir}/AMB_Shop.wav", MakeLoop(GenAmb(3f, (t, b) => b * 0.045f + 0.012f * Mathf.Sin(2f * Mathf.PI * 100f * t))), ref skipped);

        AssetDatabase.Refresh();

        // 1) AudioBank — تنها asset داخل Resources
        EnsureFolder("Assets/_Project/Resources");
        var bank = AssetDatabase.LoadAssetAtPath<AudioBank>(BankPath);
        if (bank == null)
        {
            bank = ScriptableObject.CreateInstance<AudioBank>();
            AssetDatabase.CreateAsset(bank, BankPath);
        }
        bank.entries.Clear();
        foreach (var n in SfxNames) AddEntry(bank, $"{SfxDir}/{n}.wav", n);
        foreach (var n in AmbNames) AddEntry(bank, $"{AmbDir}/{n}.wav", n);
        EditorUtility.SetDirty(bank);
        AssetDatabase.SaveAssets();

        // 2) تضمین AudioListener روی دوربین ماندگار
        var roomScene = EditorSceneManager.OpenScene(RoomPath);
        var cam = GameObject.Find("Main Camera");
        if (cam != null && cam.GetComponent<AudioListener>() == null)
        {
            cam.AddComponent<AudioListener>();
            EditorSceneManager.MarkSceneDirty(roomScene);
            EditorSceneManager.SaveScene(roomScene);
            Debug.Log("[Audio] AudioListener به Main Camera (ماندگار) اضافه شد.");
        }

        Debug.Log($"[TehranCity] Audio placeholder ready: {made} created, {skipped} existing; bank entries={bank.entries.Count}.");
    }

    private static void DeleteStray(string pathNoExt)
    {
        if (File.Exists(pathNoExt)) File.Delete(pathNoExt);
        if (File.Exists(pathNoExt + ".meta")) File.Delete(pathNoExt + ".meta");
    }

    private static void AddEntry(AudioBank bank, string path, string key)
    {
        var clip = EnsureImported(path);
        bank.entries.Add(new AudioBank.Entry { key = key, clip = clip });
    }

    private static AudioClip EnsureImported(string path)
    {
        var c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (c != null) return c;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (c == null)
        {
            var fi = new FileInfo(path);
            Debug.LogError($"[Audio] ایمپورت ناموفق: {path} (exists={fi.Exists}, bytes={(fi.Exists ? fi.Length : 0)})");
        }
        return c;
    }

    private static int Emit(string path, float[] data, ref int skipped)
    {
        if (File.Exists(path)) { skipped++; return 0; }
        WriteWav(path, data);
        return 1;
    }

    // ---------- سنتز ----------
    private static float White(float seed)
    {
        float x = Mathf.Sin(seed * 12.9898f) * 43758.5453f;
        return (x - Mathf.Floor(x)) * 2f - 1f;
    }

    private static float[] GenFootstep()
    {
        int n = (int)(Hz * 0.09f);
        var a = new float[n];
        float prev = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Hz;
            prev = (prev + White(i * 1.7f) * 0.5f) * 0.7f;
            a[i] = prev * Mathf.Exp(-t * 55f) * 0.8f;
        }
        return a;
    }

    private static float[] GenCoin()
    {
        int n = (int)(Hz * 0.2f);
        var a = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Hz;
            a[i] = t < 0.09f
                ? Mathf.Sin(2f * Mathf.PI * 1319f * t) * Mathf.Exp(-t * 30f) * 0.4f
                : Mathf.Sin(2f * Mathf.PI * 1760f * (t - 0.09f)) * Mathf.Exp(-(t - 0.09f) * 30f) * 0.4f;
        }
        return a;
    }

    private static float[] GenNotif()
    {
        int n = (int)(Hz * 0.24f);
        var a = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Hz;
            a[i] = t < 0.1f
                ? Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.35f
                : Mathf.Sin(2f * Mathf.PI * 1174f * (t - 0.1f)) * Mathf.Exp(-(t - 0.1f) * 20f) * 0.35f;
        }
        return a;
    }

    private static float[] GenTone(float dur, System.Func<float, float, float> fn)
    {
        int n = (int)(Hz * dur);
        var a = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Hz;
            a[i] = fn(t, t / dur);
        }
        return a;
    }

    private static float[] GenAmb(float dur, System.Func<float, float, float> fn)
    {
        int n = (int)(Hz * dur);
        var a = new float[n];
        float b = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Hz;
            b = (b + 0.02f * White(i * 0.37f)) / 1.02f;
            a[i] = fn(t, b * 3.2f);
        }
        return a;
    }

    private static float[] MakeLoop(float[] a)
    {
        int n = a.Length;
        int fade = Mathf.Min(n / 4, (int)(Hz * 0.15f));
        for (int i = 0; i < fade; i++)
        {
            float u = i / (float)fade;
            a[i] = Mathf.Lerp(a[n - fade + i], a[i], u);
        }
        return a;
    }

    // ---------- WAV (16-bit PCM mono) ----------
    private static void WriteWav(string path, float[] data)
    {
        int n = data.Length;
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);
        bw.Write(Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(36 + n * 2);
        bw.Write(Encoding.ASCII.GetBytes("WAVE"));
        bw.Write(Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);
        bw.Write((short)1);
        bw.Write((short)1);
        bw.Write(Hz);
        bw.Write(Hz * 2);
        bw.Write((short)2);
        bw.Write((short)16);
        bw.Write(Encoding.ASCII.GetBytes("data"));
        bw.Write(n * 2);
        for (int i = 0; i < n; i++)
            bw.Write((short)(Mathf.Clamp(data[i], -1f, 1f) * 32767));
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