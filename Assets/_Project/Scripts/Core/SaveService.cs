using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

public interface ISaveService
{
    bool HasSave { get; }
    void Save();
    bool TryLoad();
}

/* DTO دقیقاً طبق Save Data Structure سند */
[Serializable]
public class SaveData
{
    public int saveVersion = 1;
    public TimeBlock time = new TimeBlock();
    public PlayerBlock player = new PlayerBlock();
    public FlagsBlock flags = new FlagsBlock();
}
[Serializable] public class TimeBlock { public int day = 1; public int hour = 7; public int minute = 30; }
[Serializable] public class PlayerBlock { public long cash = 350000; public float energy = 70; public float stress = 40; public float hunger = 55; public float social = 45; }
[Serializable] public class FlagsBlock { public bool landlordMessageSeen; public bool jobAccepted; public bool firstShiftCompleted; }

public class SaveService : ISaveService
{
    private string Path => System.IO.Path.Combine(Application.persistentDataPath, "tehran_city_save.json");
    public bool HasSave => File.Exists(Path);

    public void Save()
    {
        var d = new SaveData();
        d.time.day = ServiceLocator.TimeService.Day;
        d.time.hour = ServiceLocator.TimeService.Hour;
        d.time.minute = ServiceLocator.TimeService.Minute;
        d.player.cash = ServiceBridge.GetBalance();
        d.player.energy = GetNeed("Energy");
        d.player.stress = GetNeed("Stress");
        d.player.hunger = GetNeed("Hunger");
        d.player.social = GetNeed("Social");
        d.flags.landlordMessageSeen = ServiceBridge.GetFlag("landlordMessageSeen");
        d.flags.jobAccepted = ServiceBridge.GetFlag("jobAccepted");
        d.flags.firstShiftCompleted = ServiceBridge.GetFlag("firstShiftCompleted");

        File.WriteAllText(Path, JsonConvert.SerializeObject(d, Formatting.Indented));
        Debug.Log($"[Save] saved @ day={d.time.day} cash={d.player.cash} job={d.flags.jobAccepted} shift={d.flags.firstShiftCompleted}");
    }

    public bool TryLoad()
    {
        if (!HasSave) return false;

        SaveData d;
        try { d = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(Path)); }
        catch (Exception e) { Debug.LogWarning($"[Save] corrupted file: {e.Message}"); return false; }

        if (d == null || d.saveVersion != 1)
        {
            Debug.LogWarning("[Save] incompatible saveVersion; ignored.");
            return false;
        }

        ServiceLocator.TimeService.SetDateTime(d.time.day, d.time.hour, d.time.minute);

        // پول فقط از EconomyService: اعمال دلتا
        long delta = d.player.cash - ServiceBridge.GetBalance();
        if (delta > 0) ServiceLocator.Economy.AddMoney(delta, "SaveLoad");
        else if (delta < 0) ServiceLocator.Economy.TrySpend(-delta, "SaveLoad");

        SetNeed("Energy", d.player.energy);
        SetNeed("Stress", d.player.stress);
        SetNeed("Hunger", d.player.hunger);
        SetNeed("Social", d.player.social);

        ServiceBridge.SetFlag("landlordMessageSeen", d.flags.landlordMessageSeen);
        ServiceBridge.SetFlag("jobAccepted", d.flags.jobAccepted);
        ServiceBridge.SetFlag("firstShiftCompleted", d.flags.firstShiftCompleted);

        Debug.Log($"[Save] loaded @ day={d.time.day} cash={d.player.cash} job={d.flags.jobAccepted} shift={d.flags.firstShiftCompleted}");
        return true;
    }

    /* نیازها: خواندن/نوشتن تایپ‌محور (بدون وابستگی به امضای INeedsService) */

    private static float GetNeed(string name)
    {
        var svc = ServiceLocator.Needs;
        if (svc == null) return 0f;
        var type = svc.GetType();
        var p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(float))
        {
            try { return (float)p.GetValue(svc); } catch { }
        }
        string camel = char.ToLower(name[0]) + name.Substring(1);
        foreach (var fn in new[] { name, "_" + camel, camel })
        {
            var f = type.GetField(fn, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(float))
            {
                try { return (float)f.GetValue(svc); } catch { }
            }
        }
        return 0f;
    }

    private static void SetNeed(string name, float value)
    {
        var svc = ServiceLocator.Needs;
        if (svc == null) return;
        var v = Mathf.Clamp(value, 0f, 100f);
        var type = svc.GetType();
        var p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(float))
        {
            try { p.SetValue(svc, v); return; } catch { }
        }
        string camel = char.ToLower(name[0]) + name.Substring(1);
        foreach (var fn in new[] { name, "_" + camel, camel })
        {
            var f = type.GetField(fn, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(float))
            {
                try { f.SetValue(svc, v); return; } catch { }
            }
        }
    }
}