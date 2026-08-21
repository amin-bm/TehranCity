using System.IO;
using UnityEditor;
using UnityEngine;

public static class ShiftDataSetup
{
    [MenuItem("TehranCity/Setup/17) Write shifts.csv (StreamingAssets)")]
    public static void Write()
    {
        var dir = Path.Combine(Application.dataPath, "StreamingAssets");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "shifts.csv");
        if (!File.Exists(path))
        {
            File.WriteAllText(path,
                "# id,salary,duration_real_sec,sequence,scripted_minutes\n" +
                "trial_shift,600000,240,register;customer;shelf;register;customer;shelf,360\n");
        }
        AssetDatabase.Refresh();
        Debug.Log($"[ShiftData] shifts.csv @ {path}");
    }
}