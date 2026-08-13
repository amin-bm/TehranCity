using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// هماهنگ‌کننده Save بدون دخالت مستقیم Gameplay:
/// ذخیره خودکار هنگام تغییر روز و پس از اولین شیفت + ذخیره دستی F5 + پیش‌نمایش خلاصه F6.
/// </summary>
public class SaveDirector : MonoBehaviour
{
    public long shopTarget = 15_000_000L; // از سند Economy — hook مغازه خیلی کوچک

    private int _lastDay;
    private bool _lastShift;

    private void Update()
    {
        if (!ServiceLocator.IsReady) return;

        int day = ServiceLocator.TimeService.Day;
        if (_lastDay == 0) _lastDay = day;
        else if (day != _lastDay)
        {
            ServiceLocator.Save.Save();
            DaySummaryUI.Show(
                ServiceBridge.GetBalance(),
                SaveServiceGetNeed("Energy"), SaveServiceGetNeed("Stress"),
                SaveServiceGetNeed("Hunger"), SaveServiceGetNeed("Social"),
                _lastDay, shopTarget);
            _lastDay = day;
        }

        bool shift = ServiceBridge.GetFlag("firstShiftCompleted");
        if (shift && !_lastShift)
            ServiceLocator.Save.Save();
        _lastShift = shift;

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.f5Key.wasPressedThisFrame)
            {
                ServiceLocator.Save.Save();
                Debug.Log("[Save] F5 manual save.");
            }
            if (kb.f6Key.wasPressedThisFrame)
            {
                DaySummaryUI.Show(
                    ServiceBridge.GetBalance(),
                    SaveServiceGetNeed("Energy"), SaveServiceGetNeed("Stress"),
                    SaveServiceGetNeed("Hunger"), SaveServiceGetNeed("Social"),
                    ServiceLocator.TimeService.Day, shopTarget);
            }
        }
    }

    private static float SaveServiceGetNeed(string name)
    {
        // خواندن نیازها از همان مسیر SaveService (رفلکسیون ایمن)
        var mi = typeof(SaveService).GetMethod("GetNeed",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        if (mi == null) return 0f;
        var r = mi.Invoke(null, new object[] { name });
        return r is float f ? f : 0f;
    }

    private void OnApplicationQuit()
    {
        if (ServiceLocator.IsReady)
            ServiceLocator.Save.Save();
    }
}