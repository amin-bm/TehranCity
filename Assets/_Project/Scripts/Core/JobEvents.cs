/// <summary>
/// رویدادهای استخدام/شیفت (قدم ۱۰-۱۱).
/// تنها نقطه‌ی تعریف این رویدادها؛ JobService آینده از همین‌ها استفاده می‌کند.
/// </summary>
public struct JobAcceptedEvent { }

public struct ShiftCompletedEvent { }

public struct SalaryReceivedEvent
{
    public long Amount;
}