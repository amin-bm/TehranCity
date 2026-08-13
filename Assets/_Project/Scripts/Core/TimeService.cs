using UnityEngine;

public enum TimeMode { FreeRoam, Indoor, Dialogue, Phone, Scripted }

public interface ITimeService
{
    int Day { get; }
    int Hour { get; }
    int Minute { get; }
    TimeMode Mode { get; set; }
    /// <summary>پیشروی شبیه‌سازی؛ مقدار GameMinute پیشرفته را برمی‌گرداند.</summary>
    float Tick(float realDeltaSeconds);

    /// <summary>پیشروی دستی زمان (برای شیفت Scripted).</summary>
    void AdvanceMinutes(int count);
}

public class TimeService : ITimeService
{
    private readonly IEventBus _bus;
    private float _acc;

    public int Day { get; private set; } = 1;
    public int Hour { get; private set; } = 7;
    public int Minute { get; private set; } = 30;
    public TimeMode Mode { get; set; } = TimeMode.Indoor; // داخل اتاق

    public TimeService(IEventBus bus) => _bus = bus;

    public float Tick(float dt)
    {
        float rate = Mode switch
        {
            TimeMode.FreeRoam => 1f,    // 1s = 1 GameMinute
            TimeMode.Indoor => 0.5f,    // 1s = 0.5 GameMinute
            TimeMode.Dialogue => 0.1f,
            _ => 0f,                    // Phone / Scripted
        };
        if (rate <= 0f) return 0f;

        float advanced = dt * rate;
        _acc += advanced;
        while (_acc >= 1f)
        {
            _acc -= 1f;
            AdvanceMinute();
        }
        return advanced;
    }

    private void AdvanceMinute()
    {
        Minute++;
        if (Minute >= 60)
        {
            Minute = 0;
            Hour++;
            if (Hour >= 24)
            {
                Hour = 0;
                Day++;
                _bus.Publish(new DayChangedEvent { Day = Day });
            }
            _bus.Publish(new HourChangedEvent { Day = Day, Hour = Hour, Minute = Minute });
        }
    }

    public void AdvanceMinutes(int count)
    {
        for (int i = 0; i < count; i++)
            AdvanceMinute();
    }
}