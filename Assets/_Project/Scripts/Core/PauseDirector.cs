using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>مکث با Esc (جدول ورودی سند): زمان متوقف + اورلی «مکث» + قفل حرکت.</summary>
public class PauseDirector : MonoBehaviour
{
    private bool _paused;
    private TimeMode _prev;
    private GameObject _panel;
    private PlayerController _player;

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
            Toggle();
    }

    private void Toggle()
    {
        _paused = !_paused;

        if (_paused)
        {
            _prev = ServiceLocator.TimeService.Mode;
            ServiceLocator.TimeService.Mode = TimeMode.Phone; // توقف زمان
            _player = FindFirstObjectByType<PlayerController>();
            if (_player != null) _player.inputLocked = true;
            BuildPanel();
            _panel.SetActive(true);
        }
        else
        {
            ServiceLocator.TimeService.Mode = _prev;
            if (_player != null) _player.inputLocked = false;
            if (_panel != null) _panel.SetActive(false);
        }

        Debug.Log($"[Pause] paused={_paused}");
    }

    private void BuildPanel()
    {
        if (_panel != null) return;

        var canvasGO = new GameObject("PauseCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 970;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        _panel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
        _panel.transform.SetParent(canvasGO.transform, false);
        var rt = _panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(700f, 220f);
        _panel.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f, 0.88f);

        var tgo = new GameObject("Text", typeof(RectTransform));
        tgo.transform.SetParent(_panel.transform, false);
        var tmp = tgo.AddComponent<TextMeshProUGUI>();
        tmp.font = ResolveFont();
        tmp.fontSize = 40;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = FaText.Fix("مکث — برای ادامه Esc بزن");
        var trt = tgo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        _panel.SetActive(false);
    }

    private static TMP_FontAsset ResolveFont()
    {
        var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        var font = fonts.FirstOrDefault(f => f != null && f.name.Contains("TMP_Tahoma_Persian"));
        if (font != null) return font;
        font = fonts.FirstOrDefault(f => f != null && f.name.Contains("Tahoma"));
        if (font != null) return font;
        return TMP_Settings.defaultFontAsset;
    }
}