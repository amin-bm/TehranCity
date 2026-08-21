using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShiftDef
{
    public string id;
    public long salary;
    public float duration;
    public int scriptedMinutes;
    public List<ShiftStationKind> sequence = new List<ShiftStationKind>();
}

/// <summary>
/// داده‌ی شیفت‌ها از StreamingAssets/shifts.csv (Data-Driven طبق سند).
/// اگر CSV نبود، ShiftController از پیش‌فرض‌های خودش استفاده می‌کند.
/// </summary>
public static class ShiftService
{
    private static Dictionary<string, ShiftDef> _defs;

    public static ShiftDef Get(string id)
    {
        Ensure();
        return _defs != null && _defs.TryGetValue(id, out var d) ? d : null;
    }

    private static void Ensure()
    {
        if (_defs != null) return;
        _defs = new Dictionary<string, ShiftDef>();

        var path = Path.Combine(Application.streamingAssetsPath, "shifts.csv");
        if (!File.Exists(path))
        {
            Debug.LogWarning("[ShiftService] shifts.csv not found; controller defaults will be used.");
            return;
        }

        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("id,")) continue;
            var c = line.Split(',');
            if (c.Length < 5) continue;

            var d = new ShiftDef { id = c[0].Trim() };
            long salary; long.TryParse(c[1].Trim(), out salary); d.salary = salary;
            float dur; float.TryParse(c[2].Trim(), out dur); d.duration = dur;
            int sm; int.TryParse(c[4].Trim(), out sm); d.scriptedMinutes = sm;

            foreach (var s in c[3].Split(';'))
                d.sequence.Add(ParseKind(s.Trim()));

            _defs[d.id] = d;
        }

        Debug.Log($"[ShiftService] loaded {_defs.Count} shift def(s) from CSV.");
    }

    private static ShiftStationKind ParseKind(string s)
    {
        switch (s.ToLower())
        {
            case "customer": return ShiftStationKind.Customer;
            case "shelf": return ShiftStationKind.Shelf;
            default: return ShiftStationKind.Register;
        }
    }
}