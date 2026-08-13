using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class ServiceBridge
{
    private static readonly Stack<object> _timeModeStack = new Stack<object>();
    private static readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

    private static bool _warnedFlags;
    private static bool _warnedTime;

    public static bool GetFlag(string key)
    {
        var svc = FindService("FlagsService", "IFlagsService", "GameFlagsService");
        if (svc == null)
        {
            WarnFlags();
            return false;
        }

        var result = Invoke(svc, new[] { "GetFlag", "Get", "GetBool", "HasFlag", "Has" }, key);
        return result is bool b && b;
    }

    public static void SetFlag(string key, bool value)
    {
        var svc = FindService("FlagsService", "IFlagsService", "GameFlagsService");
        if (svc == null)
        {
            WarnFlags();
            return;
        }

        if (!TryInvoke(svc, new[] { "SetFlag", "Set", "SetBool", "SetValue", "AddFlag" }, key, value))
        {
            WarnFlags();
        }
    }

    public static void PushTimeMode(string modeName)
    {
        var svc = FindService("TimeService", "ITimeService", "GameTimeService", "TimeManager");
        var current = GetTimeModeValue(svc);
        _timeModeStack.Push(current);

        if (svc == null)
        {
            WarnTime();
            return;
        }

        SetTimeModeByName(svc, modeName);
    }

    public static void PopTimeMode()
    {
        if (_timeModeStack.Count == 0)
            return;

        var previous = _timeModeStack.Pop();
        if (previous == null)
            return;

        var svc = FindService("TimeService", "ITimeService", "GameTimeService", "TimeManager");
        if (svc == null)
        {
            WarnTime();
            return;
        }

        SetTimeModeObject(svc, previous);
    }

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

                // 1) پراپرتی‌های استاتیک ServiceLocator (مثل .TimeService / .Flags / ...) — بر اساس تایپ، نه نام
                foreach (var prop in locatorType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!serviceType.IsAssignableFrom(prop.PropertyType)) continue;
                    try { var v = prop.GetValue(null); if (v != null) return v; } catch { }
                }

                // 2) فیلدهای استاتیک ServiceLocator
                foreach (var field in locatorType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!serviceType.IsAssignableFrom(field.FieldType)) continue;
                    try { var v = field.GetValue(null); if (v != null) return v; } catch { }
                }

                // 3) متدジェنریک Get<T> (اگر روزی اضافه شد)
                foreach (var method in locatorType.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.Name == "Get"))
                {
                    try
                    {
                        if (method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 1)
                        {
                            var generic = method.MakeGenericMethod(serviceType);
                            var result = generic.Invoke(null, null);
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

        // 4) Instance/Current استاتیک روی خود تایپ سرویس
        foreach (var typeName in typeNames)
        {
            var serviceType = FindType(typeName);
            if (serviceType == null) continue;
            var staticInstance = GetStaticInstance(serviceType);
            if (staticInstance != null) return staticInstance;
        }

        return null;
    }

    private static object GetTimeModeValue(object svc)
    {
        if (svc == null)
            return null;

        var propertyNames = new[] { "CurrentTimeMode", "CurrentMode", "Mode", "State", "TimeMode" };
        foreach (var propertyName in propertyNames)
        {
            var prop = svc.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.CanRead)
            {
                try
                {
                    return prop.GetValue(svc);
                }
                catch
                {
                    // ignore
                }
            }
        }

        var methodNames = new[] { "GetTimeMode", "GetCurrentTimeMode", "GetMode", "GetCurrentMode" };
        foreach (var methodName in methodNames)
        {
            var method = svc.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null && method.GetParameters().Length == 0)
            {
                try
                {
                    return method.Invoke(svc, null);
                }
                catch
                {
                    // ignore
                }
            }
        }

        return null;
    }

    private static void SetTimeModeByName(object svc, string modeName)
    {
        if (TrySetTimeProperty(svc, modeName))
            return;

        var enumType = FindType("TimeMode");
        object enumValue = null;

        if (enumType != null && enumType.IsEnum)
        {
            try
            {
                enumValue = Enum.Parse(enumType, modeName, true);
            }
            catch
            {
                enumValue = null;
            }
        }

        var methods = GetCandidateMethods(svc, new[]
        {
            "SetTimeMode", "SetMode", "ChangeMode", "ChangeTimeMode", "ApplyTimeMode",
            "SetCurrentTimeMode", "SetState", "EnterTimeMode", "PushTimeMode"
        });

        foreach (var method in methods)
        {
            var p = method.GetParameters()[0].ParameterType;
            try
            {
                if (enumValue != null && (p.IsInstanceOfType(enumValue) || p == typeof(object)))
                {
                    method.Invoke(svc, new[] { enumValue });
                    return;
                }

                if (p.IsEnum)
                {
                    try
                    {
                        var parsed = Enum.Parse(p, modeName, true);
                        method.Invoke(svc, new[] { parsed });
                        return;
                    }
                    catch
                    {
                        // ignore
                    }
                }

                if (p == typeof(string) || p == typeof(object))
                {
                    method.Invoke(svc, new object[] { modeName });
                    return;
                }
            }
            catch
            {
                // ignore
            }
        }

        WarnTime();
    }

    private static void SetTimeModeObject(object svc, object modeValue)
    {
        if (modeValue == null)
            return;

        if (TrySetTimeProperty(svc, modeValue))
            return;

        var methods = GetCandidateMethods(svc, new[]
        {
            "SetTimeMode", "SetMode", "ChangeMode", "ChangeTimeMode", "ApplyTimeMode",
            "SetCurrentTimeMode", "SetState", "EnterTimeMode", "PushTimeMode"
        });

        foreach (var method in methods)
        {
            var p = method.GetParameters()[0].ParameterType;
            try
            {
                if (p.IsInstanceOfType(modeValue) || p == typeof(object))
                {
                    method.Invoke(svc, new[] { modeValue });
                    return;
                }

                if (p == typeof(string))
                {
                    method.Invoke(svc, new object[] { modeValue.ToString() });
                    return;
                }
            }
            catch
            {
                // ignore
            }
        }

        WarnTime();
    }

    private static bool TrySetTimeProperty(object svc, object value)
    {
        if (svc == null || value == null)
            return false;

        var propertyNames = new[] { "CurrentTimeMode", "CurrentMode", "Mode", "State", "TimeMode" };
        foreach (var propertyName in propertyNames)
        {
            var prop = svc.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.CanWrite)
            {
                try
                {
                    if (prop.PropertyType.IsInstanceOfType(value) || prop.PropertyType == typeof(object))
                    {
                        prop.SetValue(svc, value);
                        return true;
                    }

                    if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(svc, value.ToString());
                        return true;
                    }

                    if (prop.PropertyType.IsEnum && value is string s)
                    {
                        var parsed = Enum.Parse(prop.PropertyType, s, true);
                        prop.SetValue(svc, parsed);
                        return true;
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        return false;
    }

    private static IEnumerable<MethodInfo> GetCandidateMethods(object svc, string[] names)
    {
        if (svc == null)
            return Enumerable.Empty<MethodInfo>();

        return svc.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => names.Contains(m.Name) && m.GetParameters().Length == 1 && !m.IsGenericMethod);
    }

    private static bool TryInvoke(object target, string[] methodNames, params object[] args)
    {
        if (target == null)
            return false;

        foreach (var method in GetMethods(target, methodNames, args.Length))
        {
            var converted = ConvertArgs(method, args);
            if (converted == null)
                continue;

            try
            {
                method.Invoke(target, converted);
                return true;
            }
            catch
            {
                // ignore
            }
        }

        return false;
    }

    private static object Invoke(object target, string[] methodNames, params object[] args)
    {
        if (target == null)
            return null;

        foreach (var method in GetMethods(target, methodNames, args.Length))
        {
            var converted = ConvertArgs(method, args);
            if (converted == null)
                continue;

            try
            {
                return method.Invoke(target, converted);
            }
            catch
            {
                // ignore
            }
        }

        return null;
    }

    private static IEnumerable<MethodInfo> GetMethods(object target, string[] names, int argCount)
    {
        return target.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => names.Contains(m.Name) && m.GetParameters().Length == argCount && !m.IsGenericMethod);
    }

    private static object[] ConvertArgs(MethodInfo method, object[] args)
    {
        var parameters = method.GetParameters();
        if (parameters.Length != args.Length)
            return null;

        var converted = new object[args.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var targetType = parameters[i].ParameterType;
            var arg = args[i];

            if (arg == null)
            {
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                    return null;

                converted[i] = null;
                continue;
            }

            if (targetType.IsInstanceOfType(arg))
            {
                converted[i] = arg;
                continue;
            }

            if (arg is string s)
            {
                if (targetType.IsEnum)
                {
                    try
                    {
                        converted[i] = Enum.Parse(targetType, s, true);
                        continue;
                    }
                    catch
                    {
                        return null;
                    }
                }

                if (targetType == typeof(bool))
                {
                    if (bool.TryParse(s, out var b))
                    {
                        converted[i] = b;
                        continue;
                    }

                    return null;
                }
            }

            if (targetType == typeof(string))
            {
                converted[i] = arg.ToString();
                continue;
            }

            if (targetType == typeof(object))
            {
                converted[i] = arg;
                continue;
            }

            try
            {
                converted[i] = Convert.ChangeType(arg, targetType);
            }
            catch
            {
                return null;
            }
        }

        return converted;
    }

    private static Type FindType(string name)
    {
        if (_typeCache.TryGetValue(name, out var cached))
            return cached;

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var type = asm.GetTypes().FirstOrDefault(t => t != null && t.Name == name);
                if (type != null)
                {
                    _typeCache[name] = type;
                    return type;
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                var type = e.Types.FirstOrDefault(t => t != null && t.Name == name);
                if (type != null)
                {
                    _typeCache[name] = type;
                    return type;
                }
            }
            catch
            {
                // ignore assembly
            }
        }

        _typeCache[name] = null;
        return null;
    }

    private static object GetStaticInstance(Type type)
    {
        var propertyNames = new[] { "Instance", "Current", "Main", "Service" };
        foreach (var propertyName in propertyNames)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                try
                {
                    var value = prop.GetValue(null);
                    if (value != null)
                        return value;
                }
                catch
                {
                    // ignore
                }
            }
        }

        var fieldNames = new[] { "instance", "_instance", "current", "_current" };
        foreach (var fieldName in fieldNames)
        {
            var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                try
                {
                    var value = field.GetValue(null);
                    if (value != null)
                        return value;
                }
                catch
                {
                    // ignore
                }
            }
        }

        return null;
    }

    private static void TryStaticVoid(Type type, string methodName)
    {
        try
        {
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null && method.GetParameters().Length == 0)
                method.Invoke(null, null);
        }
        catch
        {
            // ignore
        }
    }

    private static void WarnFlags()
    {
        if (_warnedFlags)
            return;

        Debug.LogWarning("[TehranCity] ServiceBridge: FlagsService API not found. jobAccepted flag may not be set.");
        _warnedFlags = true;
    }

    private static void WarnTime()
    {
        if (_warnedTime)
            return;

        Debug.LogWarning("[TehranCity] ServiceBridge: TimeService API not found. Dialogue TimeMode may not change.");
        _warnedTime = true;
    }
}