using System.Collections.Generic;

public interface IFlagsService
{
    bool Get(string key, bool defaultValue = false);
    void Set(string key, bool value);
    IReadOnlyDictionary<string, bool> All { get; }
}

/// <summary>پرچم‌های داستانی (متناظر با بخش flags در Save Data سند).</summary>
public class FlagsService : IFlagsService
{
    private readonly Dictionary<string, bool> _flags = new();

    public bool Get(string key, bool defaultValue = false) =>
        _flags.TryGetValue(key, out var v) ? v : defaultValue;

    public void Set(string key, bool value) => _flags[key] = value;

    public IReadOnlyDictionary<string, bool> All => _flags;
}