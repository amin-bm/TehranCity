using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// خلاصه پایان روز — بدون نیاز به کلید: بعد از ~۵ ثانیه خودکار بسته می‌شود
/// تا با تعامل تخت/E تداخل نکند؛ بستن دستی با E/Enter هم ممکن است.
/// حین نمایش، Interactor و حرکت قفل‌اند تا همان E تخت را فعال نکند.
/// </summary>
public class DaySummaryUI : MonoBehaviour
{
    private static DaySummaryUI _instance;

    private GameObject _panel;
    private Transform _linesRoot;
    private TMP_FontAsset _font;
    private bool _open;
    private TimeMode _prevMode;
    private float _openAt;
    private float _autoCloseAt;
    private PlayerController _player;
    private readonly List<Behaviour> _disabled = new List<Behaviour>();

    public static void Show(long cash, float energy, float stress, float hunger, float social, int endedDay, long shopTarget)
    {
        if (_instance == null)
        {
            var go = new GameObject("DaySummaryUI", typeof(RectTransform));
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<DaySummaryUI>();
        }
        _instance.Open(cash, energy, stress, hunger, social, endedDay, shopTarget);
    }

    private void Awake()
    {
        var canvasGO = new GameObject("SummaryCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 960;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGO.AddComponent<GraphicRaycaster>();

        _panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        _panel.transform.SetParent(canvasGO.transform, false);
        var rt = _panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(900f, 480f);
        _panel.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f, 0.92f);
        var vlg = _panel.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(30, 30, 24, 24);
        vlg.spacing = 14f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        _linesRoot = new GameObject("Lines", typeof(RectTransform)).transform;
        _linesRoot.SetParent(_panel.transform, false);
        var le = _linesRoot.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = 400f;
        var vlg2 = _linesRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg2.spacing = 12f;
        vlg2.childControlWidth = true;
        vlg2.childControlHeight = false;

        _font = ResolveFont();
        _panel.SetActive(false);
    }

    private void Open(long cash, float energy, float stress, float hunger, float social, int endedDay, long shopTarget)
    {
        if (_open) return;
        _open = true;

        foreach (Transform child in _linesRoot)
            Destroy(child.gameObject);

        AddLine($"خلاصه پایان روز {PersianFormat.IntWords(endedDay)}", 36, new Color(1f, 0.85f, 0.3f));
        AddLine("پول نقد: " + PersianFormat.Money(cash), 30, Color.white);
        AddLine(FaText.Fix($"انرژی {PersianFormat.FaDigits(((int)energy).ToString())} | گرسنگی {PersianFormat.FaDigits(((int)hunger).ToString())} | استرس {PersianFormat.FaDigits(((int)stress).ToString())} | اجتماعی {PersianFormat.FaDigits(((int)social).ToString())}"), 30, Color.white);
        AddLine($"هدف: پس‌انداز {PersianFormat.IntWords((int)(shopTarget / 1_000_000))} میلیون تومان برای مغازه خیلی کوچک", 28, new Color(0.7f, 0.9f, 1f));
        AddLine("این پیام خودکار بسته می‌شود؛ E/Enter هم می‌بندد", 22, new Color(0.75f, 0.75f, 0.75f));

        _panel.SetActive(true);
        _prevMode = ServiceLocator.TimeService.Mode;
        ServiceLocator.TimeService.Mode = TimeMode.Phone;
        LockGameplay(true);

        _openAt = Time.unscaledTime;
        _autoCloseAt = _openAt + 5f;
    }

    private void Close()
    {
        if (!_open) return;
        _open = false;
        _panel.SetActive(false);
        ServiceLocator.TimeService.Mode = _prevMode;
        LockGameplay(false);
    }

    private void Update()
    {
        if (!_open) return;
        float t = Time.unscaledTime;
        if (t >= _autoCloseAt) { Close(); return; }
        if (t - _openAt > 0.5f)
        {
            var kb = Keyboard.current;
            if (kb != null && (kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
                Close();
        }
    }

    private void LockGameplay(bool lockOn)
    {
        if (lockOn)
        {
            _disabled.Clear();
            foreach (var b in FindObjectsByType<Interactor>(FindObjectsSortMode.None))
                if (b != null && b.enabled) { b.enabled = false; _disabled.Add(b); }
            _player = FindFirstObjectByType<PlayerController>();
            if (_player != null) _player.inputLocked = true;
        }
        else
        {
            foreach (var b in _disabled) if (b != null) b.enabled = true;
            _disabled.Clear();
            if (_player != null) _player.inputLocked = false;
        }
    }

    private void AddLine(string text, int size, Color color)
    {
        var go = new GameObject("Line", typeof(RectTransform));
        go.transform.SetParent(_linesRoot, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = _font;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.text = FaText.Fix(text);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = size + 18f;
    }

    private static TMP_FontAsset ResolveFont()
    {
        var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (var f in fonts) if (f != null && f.name.Contains("TMP_Tahoma_Persian")) return f;
        foreach (var f in fonts) if (f != null && f.name.Contains("Tahoma")) return f;
        return TMP_Settings.defaultFontAsset;
    }
}