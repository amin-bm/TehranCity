using System.Collections.Generic;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// گوشی ساده: باز/بسته با Tab، توقف زمان، نمایش پیام‌ها (SO + runtime)،
/// ثبت flag پیام‌های دیده‌شده + ردیابی پیام ندیده‌ی صاحب‌خانه (فشار استرس).
/// نسخه نهایی: پیام‌های runtime روزانه + اسکرول + لاگ شناسه (تشخیص آبجکت تکراری).
/// </summary>
public class PhoneController : MonoBehaviour
{
    public GameObject phonePanel;
    public Transform messagesContainer;
    public List<PhoneMessageSO> messages = new();
    public TMP_FontAsset faFont;

    private TehranCityInput _input;
    private PlayerController _player;
    private TimeMode _previousMode;
    private bool _open;
    private bool _built;

    private readonly List<string[]> _runtime = new List<string[]>();
    private bool _unseenLandlord;

    public bool IsOpen => _open;

    private void OnDestroy() => _input.Gameplay.Disable();

    private void Awake()
    {
        _input = new TehranCityInput();
        _input.Gameplay.Enable();
        _player = FindFirstObjectByType<PlayerController>();
        if (phonePanel == null) Debug.LogError("[Phone] phonePanel assign نشده!");
        for (int i = 0; i < messages.Count; i++)
            if (messages[i] == null) Debug.LogError($"[Phone] messages[{i}] NULL است!");
        phonePanel.SetActive(false);
        FixLayoutPadding();
    }

    private void Update()
    {
        if (_input.Gameplay.Phone.triggered)
            Toggle();
    }

    public void Toggle()
    {
        if (_player == null) _player = FindFirstObjectByType<PlayerController>();
        _open = !_open;
        phonePanel.SetActive(_open);

        if (_open)
        {
            ServiceLocator.Ensure();
            _previousMode = ServiceLocator.TimeService.Mode;
            ServiceLocator.TimeService.Mode = TimeMode.Phone; // زمان متوقف
            if (_player != null) _player.inputLocked = true;
            BuildMessages();
            MarkSeen();
            _unseenLandlord = false; // دیدن گوشی = دیده شدن پیام صاحب‌خانه
        }
        else
        {
            ServiceLocator.TimeService.Mode = _previousMode;
            if (_player != null) _player.inputLocked = false;
        }
    }

    /* ---------- پیام‌های runtime (صاحب‌خانه) ---------- */

    public void AddRuntimeMessage(string sender, string body)
    {
        _runtime.Add(new[] { sender, body });
        _unseenLandlord = true;
        if (_built) AddEntry(sender, body);
    }

    public bool ConsumeUnseenLandlord()
    {
        var v = _unseenLandlord;
        _unseenLandlord = false;
        return v;
    }

    /// <summary>پیام runtime را به همه‌ی نمونه‌های فعال می‌فرستد (ایمن در برابر آبجکت تکراری).</summary>
    public static void BroadcastRuntime(string sender, string body)
    {
        var all = FindObjectsByType<PhoneController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int n = 0;
        foreach (var pc in all)
            if (pc != null && pc.gameObject.activeInHierarchy) { pc.AddRuntimeMessage(sender, body); n++; }
    }

    /// <summary>پرچم ندیده از هر نمونه‌ی فعالی که باشد مصرف می‌شود.</summary>
    public static bool ConsumeUnseenAny()
    {
        bool v = false;
        foreach (var pc in FindObjectsByType<PhoneController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (pc != null && pc.gameObject.activeInHierarchy)
                v |= pc.ConsumeUnseenLandlord();
        return v;
    }

    /* ---------- ساخت UI پیام‌ها ---------- */

    private void MarkSeen()
    {
        foreach (var m in messages)
            if (m != null && !string.IsNullOrEmpty(m.flagKey))
                ServiceLocator.Flags.Set(m.flagKey, true);
    }

    private void BuildMessages()
    {
        if (_built) return;
        _built = true;
        foreach (var m in messages)
        {
            if (m == null) continue;
            AddEntry(m.senderName, m.body);
        }
        foreach (var r in _runtime)
            AddEntry(r[0], r[1]);
    }

    private void AddEntry(string sender, string body)
    {
        var entry = new GameObject("Msg_" + sender);
        entry.transform.SetParent(messagesContainer, false);
        var v = entry.AddComponent<VerticalLayoutGroup>();
        v.spacing = 6f;
        MakeRtl("Sender", sender, 30, new Color(1f, 0.85f, 0.3f), entry.transform);
        MakeRtl("Body", body, 28, Color.white, entry.transform);
    }

    private void MakeRtl(string name, string text, float size, Color color, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<RTLTextMeshPro>();
        tmp.font = faFont;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.text = text;
    }

    /// <summary>فقط فاصله از لبه‌ی بالا؛ بدون دست‌کاری anchor/والد.</summary>
    private void FixLayoutPadding()
    {
        if (messagesContainer == null) return;
        var vlg = messagesContainer.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = messagesContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(24, 24, 40, 20);
        vlg.spacing = 18f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
    }
}