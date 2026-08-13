/// <summary>رجیستری سرویس‌ها؛ Offline-first / Online-ready (بعداً سرویس‌های سرور جایگزین می‌شوند).</summary>
public static class ServiceLocator
{
    public static IEventBus EventBus { get; private set; }
    public static ITimeService TimeService { get; private set; }
    public static IEconomyService Economy { get; private set; }
    public static INeedsService Needs { get; private set; }
    public static bool IsReady { get; private set; }
    public static IFlagsService Flags { get; private set; }
    public static ISaveService Save { get; private set; }

    public static void Ensure()
    {
        if (IsReady) return;
        var bus = new EventBus();
        var time = new TimeService(bus);
        var economy = new EconomyService(bus, time, 350_000L);   // طبق نمونه Save سند
        var needs = new NeedsService(bus, 70f, 40f, 55f, 45f);   // Energy/Stress/Hunger/Social
        EventBus = bus;
        TimeService = time;
        Economy = economy;
        Needs = needs;
        Flags = new FlagsService();
        Save = new SaveService();
        IsReady = true;
    }

    public static void Reset()
    {
        EventBus = null; TimeService = null; Economy = null; Needs = null;
        IsReady = false;
        Flags = null;
        Save = null;
    }
}