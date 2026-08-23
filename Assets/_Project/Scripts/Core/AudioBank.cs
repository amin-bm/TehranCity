using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// بانک رفرنس کلیپ‌های صدا. تنها asset داخل Resources؛
/// خود فایل‌های صوتی در Audio/SFX و Audio/Ambience می‌مانند (ساختار مصوب سند).
/// </summary>
public class AudioBank : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string key;
        public AudioClip clip;
    }

    public List<Entry> entries = new();

    public AudioClip Get(string key)
    {
        for (int i = 0; i < entries.Count; i++)
            if (entries[i].key == key) return entries[i].clip;
        return null;
    }
}