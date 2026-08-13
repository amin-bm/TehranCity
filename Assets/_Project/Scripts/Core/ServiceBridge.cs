using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// پل ارتباطی runtime با سرویس‌ها.
/// Flags: سیگنیچر-محور (هر متد (string)->bool یا (string,bool)->void یا ایندکسر رشته‌ای).
/// Economy: تایپ‌دار (ServiceLocator.Economy) + GetBalance رفلکسیونی.
/// Time: تایپ‌محور روی enum.
/// </summary>
public static class ServiceBridge
{
    private static readonly Stack<object> _timeModeStack = new Stack<object>();
    private static readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();
    private static bool _warnedFlags;
    private static bool _warnedTime;
    private static bool _warnedEconomy;
    private static bool _warnedNeeds;
    private static readonly Dictionary<string, bool> _flagMirror = new Dictionary<string, bool>();

    /* ================= Flags (سیگنیچر-محور) ================= */

    public static bool GetFlag(string key)
    {
        var svc = FindService("FlagsService", "IFlagsService", "GameFlagsService");
        if (svc != null)
        {
            var type = svc.GetType();

            var named = Invoke(svc, new[] { "GetFlag", "Get", "GetBool", "HasFlag", "Has", "IsSet", "Contains", "GetValue", "Read" }, key);
            if (named is bool nb) { _flagMirror[key] = nb; return nb; }

            foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var ps = m.GetParameters();
                if (ps.Length != 1 || ps[0].ParameterType != typeof(string) || m.IsGenericMethod) continue;
                try
                {
                    var r = m.Invoke(svc, new object[] { key });
                    if (r is bool b) { _flagMirror[key] = b; return b; }
                    if (r is int i) { var v = i != 0; _flagMirror[key] = v; return v; }
                }
                catch { }
            }

            foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var ix = p.GetIndexParameters();
                if (ix.Length == 1 && ix[0].ParameterType == typeof(string))
                {
                    try { var r = p.GetValue(svc, new object[] { key }); if (r is bool b2) { _flagMirror[key] = b2; return b2; } } catch { }
                }
                else if (ix.Length == 0 && p.PropertyType == typeof(bool) &&
                         string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase))
                {
                    try { var v = (bool)p.GetValue(svc); _flagMirror[key] = v; return v; } catch { }
                }
            }

            foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (f.FieldType == typeof(bool) && string.Equals(f.Name, key, StringComparison.OrdinalIgnoreCase))
                {
                    try { var v = (bool)f.GetValue(svc); _flagMirror[key] = v; return v; } catch { }
                }
                if (f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var ga = f.FieldType.GetGenericArguments();
                    if (ga[0] == typeof(string) && ga[1] == typeof(bool))
                    {
                        try
                        {
                            var dict = (Dictionary<string, bool>)f.GetValue(svc);
                            if (dict != null && dict.TryGetValue(key, out var dv)) { _flagMirror[key] = dv; return dv; }
                        }
                        catch { }
                    }
                }
            }
        }

        // سنگر آخر: آینه داخلی (تضمین سازگاری Set/Get در سشن و Save/Load)
        return _flagMirror.TryGetValue(key, out var mv) && mv;
    }

    public static void SetFlag(string key, bool value)
    {
        _flagMirror[key] = value; // همیشه

        var svc = FindService("FlagsService", "IFlagsService", "GameFlagsService");
        if (svc == null) return;
        if (TryInvoke(svc, new[] { "SetFlag", "Set", "SetBool", "SetValue", "AddFlag" }, key, value)) return;

        var type = svc.GetType();
        foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var ps = m.GetParameters();
            if (ps.Length != 2 || ps[0].ParameterType != typeof(string) ||
                ps[1].ParameterType != typeof(bool) || m.ReturnType != typeof(void) || m.IsGenericMethod) continue;
            try { m.Invoke(svc, new object[] { key, value }); return; } catch { }
        }

        foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var ix = p.GetIndexParameters();
            if (ix.Length == 1 && ix[0].ParameterType == typeof(string) && p.CanWrite && p.PropertyType == typeof(bool))
            { try { p.SetValue(svc, value, new object[] { key }); return; } catch { } }
            if (ix.Length == 0 && p.CanWrite && p.PropertyType == typeof(bool) &&
                string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase))
            { try { p.SetValue(svc, value); return; } catch { } }
        }

        foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (f.FieldType == typeof(bool) && string.Equals(f.Name, key, StringComparison.OrdinalIgnoreCase))
            { try { f.SetValue(svc, value); return; } catch { } }
            if (f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var ga = f.FieldType.GetGenericArguments();
                if (ga[0] == typeof(string) && ga[1] == typeof(bool))
                {
                    try { var dict = (Dictionary<string, bool>)f.GetValue(svc); if (dict != null) { dict[key] = value; return; } } catch { }
                }
            }
        }
    }

    /* ================= TimeMode ================= */

    public static void PushTimeMode(string modeName)
    {
        var svc = FindService("TimeService", "ITimeService", "GameTimeService", "TimeManager");
        _timeModeStack.Push(GetTimeModeValue(svc));
        if (svc == null) { WarnTime(); return; }
        SetTimeMode(svc, modeName);
    }

    public static void PopTimeMode()
    {
        if (_timeModeStack.Count == 0) return;
        var previous = _timeModeStack.Pop();
        if (previous == null) return;
        var svc = FindService("TimeService", "ITimeService", "GameTimeService", "TimeManager");
        if (svc == null) { WarnTime(); return; }
        SetTimeModeObject(svc, previous);
    }

    /* ================= Economy ================= */

    public static bool AddMoney(long amount, string reason = "Salary")
    {
        ServiceLocator.Ensure();
        var eco = ServiceLocator.Economy;
        if (eco == null) { WarnEconomy(); return false; }
        try { eco.AddMoney(amount, reason); return true; }
        catch (Exception e) { Debug.LogWarning($"[TehranCity] AddMoney({amount}) threw: {e}"); return false; }
    }

    public static bool TrySpend(long amount, string reason = "Purchase")
    {
        ServiceLocator.Ensure();
        var eco = ServiceLocator.Economy;
        if (eco == null) { WarnEconomy(); return false; }
        try { return eco.TrySpend(amount, reason); }
        catch (Exception e) { Debug.LogWarning($"[TehranCity] TrySpend({amount}) threw: {e}"); return false; }
    }

    public static long GetBalance()
    {
        var svc = FindService("EconomyService", "IEconomyService");
        if (svc == null) { WarnEconomy(); return 0L; }
        var type = svc.GetType();

        foreach (var n in new[] { "GetBalance", "GetCash", "ReadBalance" })
        {
            var m = type.GetMethod(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (m != null) { try { var r = m.Invoke(svc, null); if (r is long l) return l; } catch { } }
        }
        foreach (var n in new[] { "Balance", "Cash", "CurrentBalance", "Money" })
        {
            var p = type.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null) { try { var r = p.GetValue(svc); if (r is long l) return l; } catch { } }
        }
        foreach (var n in new[] { "_balance", "balance", "_cash", "cash" })
        {
            var f = type.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null) { try { var r = f.GetValue(svc); if (r is long l) return l; } catch { } }
        }

        Debug.LogWarning("[TehranCity] ServiceBridge: balance read failed. (EconomyService.cs را بفرست.)");
        return 0L;
    }

    /* ================= Needs ================= */

    public static void AddNeed(string needName, float delta)
    {
        var svc = FindService("NeedsService", "INeedsService");
        if (svc == null) { WarnNeeds(); return; }
        var type = svc.GetType();

        var p = type.GetProperty(needName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(float))
        {
            try { float cur = (float)p.GetValue(svc); p.SetValue(svc, Mathf.Clamp(cur + delta, 0f, 100f)); return; } catch { }
        }

        string camel = char.ToLower(needName[0]) + needName.Substring(1);
        foreach (var fn in new[] { needName, "_" + camel, camel })
        {
            var f = type.GetField(fn, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null || f.FieldType != typeof(float)) continue;
            try { float cur = (float)f.GetValue(svc); f.SetValue(svc, Mathf.Clamp(cur + delta, 0f, 100f)); return; } catch { }
        }

        WarnNeeds();
    }

    /* ================= یافتن سرویس ================= */

    private static object FindService(params string[] typeNames)
    {
        var locatorType = FindType("ServiceLocator");
        if (locatorType != null)
        {
            TryStaticVoid(locatorType, "Ensure");
            foreach (var typeName in typeNames)
            {
                var serviceType = FindType(typeName);
                if (serviceType == null) continue;

                foreach (var prop in locatorType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!serviceType.IsAssignableFrom(prop.PropertyType)) continue;
                    try { var v = prop.GetValue(null); if (v != null) return v; } catch { }
                }
                foreach (var field in locatorType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!serviceType.IsAssignableFrom(field.FieldType)) continue;
                    try { var v = field.GetValue(null); if (v != null) return v; } catch { }
                }
                foreach (var method in locatorType.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.Name == "Get"))
                {
                    try
                    {
                        if (method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 1)
                        {
                            var result = method.MakeGenericMethod(serviceType).Invoke(null, null);
                            if (result != null) return result;
                        }
                        else if (method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(Type))
                        {
                            var result = method.Invoke(null, new object[] { serviceType });
                            if (result != null) return result;
                        }
                    }
                    catch { }
                }
            }
        }

        foreach (var typeName in typeNames)
        {
            var serviceType = FindType(typeName);
            if (serviceType == null) continue;
            var staticInstance = GetStaticInstance(serviceType);
            if (staticInstance != null) return staticInstance;
        }
        return null;
    }

    /* ================= TimeMode: تایپ‌محور ================= */

    private static object GetTimeModeValue(object svc)
    {
        if (svc == null) return null;
        foreach (var p in svc.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!p.CanRead || p.GetIndexParameters().Length > 0 || !p.PropertyType.IsEnum) continue;
            if (!LooksTimeRelated(p.Name) && !LooksTimeRelated(p.PropertyType.Name)) continue;
            try { return p.GetValue(svc); } catch { }
        }
        foreach (var f in svc.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!f.FieldType.IsEnum) continue;
            if (!LooksTimeRelated(f.Name) && !LooksTimeRelated(f.FieldType.Name)) continue;
            try { return f.GetValue(svc); } catch { }
        }
        return null;
    }

    private static void SetTimeMode(object svc, string modeName)
    {
        var type = svc.GetType();

        foreach (var pass in new[] { true, false })
        {
            foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!p.CanWrite || p.GetIndexParameters().Length > 0 || !p.PropertyType.IsEnum) continue;
                if (pass && !LooksTimeRelated(p.Name) && !LooksTimeRelated(p.PropertyType.Name)) continue;
                var v = SafeParseEnum(p.PropertyType, modeName);
                if (v == null) continue;
                try { p.SetValue(svc, v); return; } catch { }
            }
        }

        foreach (var pass in new[] { true, false })
        {
            foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!f.FieldType.IsEnum) continue;
                if (pass && !LooksTimeRelated(f.Name) && !LooksTimeRelated(f.FieldType.Name)) continue;
                var v = SafeParseEnum(f.FieldType, modeName);
                if (v == null) continue;
                try { f.SetValue(svc, v); return; } catch { }
            }
        }

        foreach (var pass in new[] { true, false })
        {
            foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var ps = m.GetParameters();
                if (ps.Length != 1 || m.IsGenericMethodDefinition) continue;
                var pt = ps[0].ParameterType;
                try
                {
                    if (pt.IsEnum)
                    {
                        if (pass && !LooksTimeRelated(m.Name) && !LooksTimeRelated(pt.Name)) continue;
                        var v = SafeParseEnum(pt, modeName);
                        if (v != null) { m.Invoke(svc, new[] { v }); return; }
                    }
                    else if (pt == typeof(string))
                    {
                        if (pass && !LooksTimeRelated(m.Name)) continue;
                        m.Invoke(svc, new object[] { modeName }); return;
                    }
                }
                catch { }
            }
        }

        WarnTime();
    }

    private static void SetTimeModeObject(object svc, object modeValue)
    {
        if (modeValue == null) return;
        var type = svc.GetType();

        foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!p.CanWrite || p.GetIndexParameters().Length > 0) continue;
            try { if (p.PropertyType.IsInstanceOfType(modeValue)) { p.SetValue(svc, modeValue); return; } } catch { }
        }
        foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            try { if (f.FieldType.IsInstanceOfType(modeValue)) { f.SetValue(svc, modeValue); return; } } catch { }
        }
        foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var ps = m.GetParameters();
            if (ps.Length != 1 || m.IsGenericMethodDefinition) continue;
            var pt = ps[0].ParameterType;
            try
            {
                if (pt.IsInstanceOfType(modeValue)) { m.Invoke(svc, new[] { modeValue }); return; }
                if (pt.IsEnum && modeValue is string s)
                {
                    var v = SafeParseEnum(pt, s);
                    if (v != null) { m.Invoke(svc, new[] { v }); return; }
                }
            }
            catch { }
        }

        WarnTime();
    }

    private static bool LooksTimeRelated(string name) =>
        name.Contains("Time") || name.Contains("Mode") || name.Contains("State");

    private static object SafeParseEnum(Type enumType, string value)
    {
        try { return Enum.Parse(enumType, value, true); }
        catch { return null; }
    }

    /* ================= ابزارهای Invoke ================= */

    private static bool TryInvoke(object target, string[] methodNames, params object[] args)
    {
        if (target == null) return false;
        foreach (var method in GetMethods(target, methodNames, args.Length))
        {
            var converted = ConvertArgs(method, args);
            if (converted == null) continue;
            try { method.Invoke(target, converted); return true; } catch { }
        }
        return false;
    }

    private static object Invoke(object target, string[] methodNames, params object[] args)
    {
        if (target == null) return null;
        foreach (var method in GetMethods(target, methodNames, args.Length))
        {
            var converted = ConvertArgs(method, args);
            if (converted == null) continue;
            try { return method.Invoke(target, converted); } catch { }
        }
        return null;
    }

    private static IEnumerable<MethodInfo> GetMethods(object target, string[] names, int argCount)
    {
        return target.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => (names.Contains(m.Name) || names.Any(n => m.Name.EndsWith("." + n)))
                        && m.GetParameters().Length == argCount && !m.IsGenericMethod);
    }

    private static object[] ConvertArgs(MethodInfo method, object[] args)
    {
        var parameters = method.GetParameters();
        if (parameters.Length != args.Length) return null;
        var converted = new object[args.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var targetType = parameters[i].ParameterType;
            var arg = args[i];
            if (arg == null)
            {
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null) return null;
                converted[i] = null; continue;
            }
            if (targetType.IsInstanceOfType(arg)) { converted[i] = arg; continue; }
            if (arg is string s)
            {
                if (targetType.IsEnum)
                {
                    try { converted[i] = Enum.Parse(targetType, s, true); continue; }
                    catch { return null; }
                }
                if (targetType == typeof(bool))
                {
                    if (bool.TryParse(s, out var b)) { converted[i] = b; continue; }
                    return null;
                }
            }
            if (targetType == typeof(string)) { converted[i] = arg.ToString(); continue; }
            if (targetType == typeof(object)) { converted[i] = arg; continue; }
            try { converted[i] = Convert.ChangeType(arg, targetType); }
            catch { return null; }
        }
        return converted;
    }

    /* ================= ابزارهای Type ================= */

    private static Type FindType(string name)
    {
        if (_typeCache.TryGetValue(name, out var cached)) return cached;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var type = asm.GetTypes().FirstOrDefault(t => t != null && t.Name == name);
                if (type != null) { _typeCache[name] = type; return type; }
            }
            catch (ReflectionTypeLoadException e)
            {
                var type = e.Types.FirstOrDefault(t => t != null && t.Name == name);
                if (type != null) { _typeCache[name] = type; return type; }
            }
            catch { }
        }
        _typeCache[name] = null;
        return null;
    }

    private static object GetStaticInstance(Type type)
    {
        foreach (var propertyName in new[] { "Instance", "Current", "Main", "Service" })
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null) { try { var value = prop.GetValue(null); if (value != null) return value; } catch { } }
        }
        foreach (var fieldName in new[] { "instance", "_instance", "current", "_current" })
        {
            var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) { try { var value = field.GetValue(null); if (value != null) return value; } catch { } }
        }
        return null;
    }

    private static void TryStaticVoid(Type type, string methodName)
    {
        try
        {
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null && method.GetParameters().Length == 0) method.Invoke(null, null);
        }
        catch { }
    }

    /* ================= هشدارها ================= */

    private static void WarnFlags()
    {
        if (_warnedFlags) return;
        Debug.LogWarning("[TehranCity] ServiceBridge: FlagsService getter/setter not found. (FlagsService.cs را بفرست.)");
        _warnedFlags = true;
    }
    private static void WarnTime()
    {
        if (_warnedTime) return;
        Debug.LogWarning("[TehranCity] ServiceBridge: TimeService API not found.");
        _warnedTime = true;
    }
    private static void WarnEconomy()
    {
        if (_warnedEconomy) return;
        Debug.LogWarning("[TehranCity] ServiceBridge: EconomyService API not found.");
        _warnedEconomy = true;
    }
    private static void WarnNeeds()
    {
        if (_warnedNeeds) return;
        Debug.LogWarning("[TehranCity] ServiceBridge: NeedsService API not found.");
        _warnedNeeds = true;
    }
}