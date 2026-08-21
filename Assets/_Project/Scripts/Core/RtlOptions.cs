using System;
using System.Reflection;
using TMPro;

/// <summary>روشن‌کردن runtime گزینه‌ی Preserve Numbers روی RTLTMPro (ساعت/ارقام درست بمانند).</summary>
public static class RtlOptions
{
    public static void SetPreserveNumbers(TMP_Text tmp, bool value)
    {
        if (tmp == null) return;
        var type = tmp.GetType();
        foreach (var name in new[] { "preserveNumbers", "PreserveNumbers" })
        {
            var f = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(bool))
            {
                f.SetValue(tmp, value);
                Refix(tmp);
                return;
            }
            var p = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanWrite)
            {
                p.SetValue(tmp, value);
                Refix(tmp);
                return;
            }
        }
    }

    private static void Refix(TMP_Text tmp)
    {
        var type = tmp.GetType();
        foreach (var mname in new[] { "Fix", "FixText", "ReFix" })
        {
            var m = type.GetMethod(mname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (m != null) { try { m.Invoke(tmp, null); return; } catch { } }
        }
        tmp.text = tmp.text; // fallback: انتساب دوباره‌ی متن برای اجرای Fix
    }
}