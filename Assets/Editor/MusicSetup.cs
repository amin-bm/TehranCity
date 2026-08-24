using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// فاز A / قدم A7 — لوپ موسیقی placeholder در Audio/Music (ساختار مصوب).
/// سواپ بعدی: فایل واقعی با نام MUS_Main.wav جایگزین شود + اجرای دوباره منو.
/// </summary>
public static class MusicSetup
{
    private const string MusDir = "Assets/_Project/Audio/Music";
    private const string BankPath = "Assets/_Project/Resources/AudioBank.asset";
    private const int Hz = 22050;

    [MenuItem("TehranCity/Setup/24 Music Placeholder (Procedural Loop)")]
    public static void Run()
    {
        Directory.CreateDirectory(MusDir);
        string path = $"{MusDir}/MUS_Main.wav";
        if (!File.Exists(path))
            WriteWav(path, GenMusic());

        AssetDatabase.Refresh();
        var clip = EnsureImported(path);
        if (clip == null) return;

        // upsert در AudioBank
        var bank = AssetDatabase.LoadAssetAtPath<AudioBank>(BankPath);
        if (bank == null)
        {
            bank = ScriptableObject.CreateInstance<AudioBank>();
            AssetDatabase.CreateAsset(bank, BankPath);
        }
        var entry = bank.entries.Find(e => e.key == "MUS_Main");
        if (entry != null) entry.clip = clip;
        else bank.entries.Add(new AudioBank.Entry { key = "MUS_Main", clip = clip });
        EditorUtility.SetDirty(bank);
        AssetDatabase.SaveAssets();

        Debug.Log($"[TehranCity] Music placeholder ready: {path} (bank entries={bank.entries.Count}).");
    }

    /// <summary>پد آرام ۱۲ ثانیه‌ای: Am - F - C - G؛ انتهاها صفر => لوپ بدون درز.</summary>
    private static float[] GenMusic()
    {
        const float dur = 12f;
        int n = (int)(Hz * dur);
        var a = new float[n];
        float[][] chords =
        {
            new[] { 110f, 220f, 261.63f, 329.63f },  // Am
            new[] { 87.31f, 174.61f, 220f, 261.63f },// F
            new[] { 130.81f, 261.63f, 329.63f, 392f },// C
            new[] { 98f, 196f, 246.94f, 293.66f },   // G
        };
        float seg = dur / chords.Length;
        for (int c = 0; c < chords.Length; c++)
        {
            int i0 = (int)(c * seg * Hz);
            int i1 = (int)((c + 1) * seg * Hz);
            for (int i = i0; i < i1 && i < n; i++)
            {
                float t = (i - i0) / (float)Hz;
                float env = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.7f)) *
                            Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((seg - t) / 0.7f));
                float v = 0f;
                for (int k = 0; k < chords[c].Length; k++)
                {
                    float f = chords[c][k];
                    float amp = k == 0 ? 0.10f : 0.055f;
                    v += Mathf.Sin(2f * Mathf.PI * f * t) * amp;
                    v += Mathf.Sin(2f * Mathf.PI * f * 1.003f * t) * amp * 0.4f;
                }
                a[i] += v * env;
            }
        }
        return a;
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
            Debug.LogError($"[Music] ایمپورت ناموفق: {path} (exists={fi.Exists}, bytes={(fi.Exists ? fi.Length : 0)})");
        }
        return c;
    }

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
}