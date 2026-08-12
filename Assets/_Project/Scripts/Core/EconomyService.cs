using System.Collections.Generic;

public struct Transaction
{
    public long Id;
    public long Amount;
    public string Reason;
    public int Day;
    public int Hour;
}

public interface IEconomyService
{
    long Balance { get; }
    bool TrySpend(long amount, string reason);
    void AddMoney(long amount, string reason);
    IReadOnlyList<Transaction> History { get; }
}

/// <summary>تنها نقطه تغییر پول (ADR-007). بالانس long است.</summary>
public class EconomyService : IEconomyService
{
    private readonly IEventBus _bus;
    private readonly ITimeService _time;
    private readonly List<Transaction> _history = new();
    private long _nextId = 1;

    public long Balance { get; private set; }
    public IReadOnlyList<Transaction> History => _history;

    public EconomyService(IEventBus bus, ITimeService time, long initialBalance)
    {
        _bus = bus;
        _time = time;
        Balance = initialBalance;
    }

    public bool TrySpend(long amount, string reason)
    {
        if (amount <= 0 || amount > Balance) return false;
        Balance -= amount;
        Record(-amount, reason);
        return true;
    }

    public void AddMoney(long amount, string reason)
    {
        if (amount <= 0) return;
        Balance += amount;
        Record(amount, reason);
    }

    private void Record(long delta, string reason)
    {
        _history.Add(new Transaction
        {
            Id = _nextId++,
            Amount = delta,
            Reason = reason,
            Day = _time.Day,
            Hour = _time.Hour
        });
        _bus.Publish(new BalanceChangedEvent { Balance = Balance, Delta = delta, Reason = reason });
    }
}