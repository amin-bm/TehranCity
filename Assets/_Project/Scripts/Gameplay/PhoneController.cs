using System.Collections.Generic;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>گوشی ساده (Phase 1): باز/بسته با Tab، توقف زمان، نمایش پیام‌ها، ثبت flag.</summary>
public class PhoneController : MonoBehaviour
{
    // public تا Setup مستقیم و بدون SerializedObject مقداردهی کند
    public GameObject phonePanel;
    public Transform messagesContainer;
    public List<PhoneMessageSO> messages = new();
    public TMP_FontAsset faFont;

    private TehranCityInput _input;
    private PlayerController _player;
    private TimeMode _previousMode;
    private bool _open;
    private bool _built;

    public bool IsOpen => _open;

    private void Awake()
    {
        _input = new TehranCityInput();
        _input.Gameplay.Enable();
        _player = FindFirstObjectByType<PlayerController>();
        if (phonePanel == null) Debug.LogError("[Phone] phonePanel assign نشده!");
        for (int i = 0; i < messages.Count; i++)
            if (messages[i] == null) Debug.LogError($"[Phone] messages[{i}] NULL است!");
        phonePanel.SetActive(false);
        Debug.Log($"[Phone] Awake | panel={phonePanel != null} | player={_player != null} | messages={messages.Count}");
    }

    private void Update()
    {
        if (_input.Gameplay.Phone.triggered)
        {
            Debug.Log("[Phone] Tab pressed -> Toggle");
            Toggle();
        }
    }

    public void Toggle()
    {
        _open = !_open;
        phonePanel.SetActive(_open);
        Debug.Log($"[Phone] Open={_open}");

        if (_open)
        {
            ServiceLocator.Ensure();
            _previousMode = ServiceLocator.TimeService.Mode;
            ServiceLocator.TimeService.Mode = TimeMode.Phone; // زمان متوقف
            if (_player != null) _player.inputLocked = true;
            BuildMessages();
            MarkSeen();
        }
        else
        {
            ServiceLocator.TimeService.Mode = _previousMode;
            if (_player != null) _player.inputLocked = false;
        }
    }

    private void MarkSeen()
    {
        foreach (var m in messages)
            if (m != null && !string.IsNullOrEmpty(m.flagKey))
            {
                ServiceLocator.Flags.Set(m.flagKey, true);
                Debug.Log($"[Phone] flag '{m.flagKey}' = true");
            }
    }

    private void BuildMessages()
    {
        if (_built) return;
        _built = true;
        foreach (var m in messages)
        {
            if (m == null) { Debug.LogWarning("[Phone] پیام null رد شد."); continue; }

            var entry = new GameObject("Msg_" + m.messageId);
            entry.transform.SetParent(messagesContainer, false);
            var v = entry.AddComponent<VerticalLayoutGroup>();
            v.spacing = 6f;

            MakeRtl("Sender", m.senderName, 30, new Color(1f, 0.85f, 0.3f), entry.transform);
            MakeRtl("Body", m.body, 28, Color.white, entry.transform);
        }
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
}