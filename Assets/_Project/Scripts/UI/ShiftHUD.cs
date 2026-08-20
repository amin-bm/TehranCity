using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// نوار بالای صفحه حین شیفت — متن روی UI_Canvas ماندگار (همان کانواس اثبات‌شده‌ی HUD).
/// سطح ۲ قانون متن: TextMeshProUGUI + FaText.Fix + ارقام فارسی.
/// </summary>
public class ShiftHUD : MonoBehaviour
{
    private TextMeshProUGUI _text;

    public static ShiftHUD Show()
    {
        var existing = FindFirstObjectByType<ShiftHUD>(FindObjectsInactive.Include);
        if (existing != null) { existing.gameObject.SetActive(true); return existing; }

        var go = new GameObject("ShiftHUD", typeof(RectTransform));
        return go.AddComponent<ShiftHUD>();
    }

    private void Awake()
    {
        var host = GameObject.Find("UI_Canvas");

        if (host != null)
        {
            transform.SetParent(host.transform, false);

            // خودِ ShiftHUD تمام صفحه شود تا anchor فرزند درست کار کند
            var selfRt = GetComponent<RectTransform>();
            selfRt.anchorMin = Vector2.zero;
            selfRt.anchorMax = Vector2.one;
            selfRt.offsetMin = Vector2.zero;
            selfRt.offsetMax = Vector2.zero;
            selfRt.localScale = Vector3.one;
        }
        else
        {
            // fallback نادر: کانواس مستقل
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 940;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        var tgo = new GameObject("ShiftText", typeof(RectTransform));
        tgo.transform.SetParent(transform, false);

        _text = tgo.AddComponent<TextMeshProUGUI>();
        _text.font = ResolveFont();
        _text.fontSize = 34;
        _text.color = Color.white;
        _text.alignment = TextAlignmentOptions.Top;
        _text.raycastTarget = false;

        var rt = tgo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -80f); // ۸۰ پیکسل پایین‌تر از لبه‌ی بالا
        rt.sizeDelta = new Vector2(1600f, 60f);

    }

    public void SetText(string s)
    {
        if (_text == null)
        {
            Debug.LogWarning("[ShiftHUD] _text NULL!");
            return;
        }

        var output = FaText.Fix(s);
        if (string.IsNullOrEmpty(output)) output = s;
        _text.text = output;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private static TMP_FontAsset ResolveFont()
    {
        var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        var font = fonts.FirstOrDefault(f => f != null && f.name.Contains("TMP_Tahoma_Persian"));
        if (font != null) return font;
        font = fonts.FirstOrDefault(f => f != null && f.name.Contains("Tahoma"));
        if (font != null) return font;
        font = fonts.FirstOrDefault(f => f != null && f.name.Contains("Persian"));
        if (font != null) return font;
        return TMP_Settings.defaultFontAsset;
    }
}