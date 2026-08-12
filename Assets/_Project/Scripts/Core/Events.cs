public struct GameStartedEvent { }
public struct HourChangedEvent { public int Day; public int Hour; public int Minute; }
public struct DayChangedEvent { public int Day; }
public struct BalanceChangedEvent { public long Balance; public long Delta; public string Reason; }
public struct NeedsChangedEvent { public float Energy; public float Stress; public float Hunger; public float Social; }