using UnityEngine;

public interface INeedsService
{
    float Energy { get; }
    float Stress { get; }
    float Hunger { get; }
    float Social { get; }
    float MoveSpeedMultiplier { get; } // Energy<25 => 0.8 طبق سند
    void Tick(float gameHoursDelta);
}

public class NeedsService : INeedsService
{
    private readonly IEventBus _bus;
    private int _lastE, _lastS, _lastH, _lastSo;

    public float Energy { get; private set; }
    public float Stress { get; private set; }
    public float Hunger { get; private set; }
    public float Social { get; private set; }

    public float MoveSpeedMultiplier => Energy < 25f ? 0.8f : 1f;

    public NeedsService(IEventBus bus, float energy, float stress, float hunger, float social)
    {
        _bus = bus;
        Energy = energy; Stress = stress; Hunger = hunger; Social = social;
        CacheAndPublish(true);
    }

    public void Tick(float gameHours)
    {
        if (gameHours <= 0f) return;
        // بیدار بودن عادی (از جدول سند)
        Energy = Clamp(Energy - 2f * gameHours);
        Hunger = Clamp(Hunger - 2f * gameHours);
        Stress = Clamp(Stress + 1f * gameHours);
        Social = Clamp(Social - 1f * gameHours);
        CacheAndPublish(false);
    }

    private static float Clamp(float v) => Mathf.Clamp(v, 0f, 100f);

    private void CacheAndPublish(bool force)
    {
        int e = Mathf.RoundToInt(Energy), s = Mathf.RoundToInt(Stress),
            h = Mathf.RoundToInt(Hunger), so = Mathf.RoundToInt(Social);
        if (force || e != _lastE || s != _lastS || h != _lastH || so != _lastSo)
        {
            _lastE = e; _lastS = s; _lastH = h; _lastSo = so;
            _bus.Publish(new NeedsChangedEvent { Energy = e, Stress = s, Hunger = h, Social = so });
        }
    }
}