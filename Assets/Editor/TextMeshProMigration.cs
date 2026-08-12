using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// تمام کامپوننت‌های TextMeshProUGUI در صحنه فعلی را به RTLTextMeshPro تبدیل می‌کند.
/// </summary>
public static class TextMeshProMigration
{
    [MenuItem("TehranCity/Migrate/Convert TextMeshProUGUI to RTLTextMeshPro")]
    public static void Migrate()
    {
        int converted = 0;
        var allTMP = Object.FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var tmp in allTMP)
        {
            // اگر از قبل RTLTextMeshPro است، رد کن
            if (tmp is RTLTMPro.RTLTextMeshPro) continue;

            var go = tmp.gameObject;
            var font = tmp.font;
            var fontSize = tmp.fontSize;
            var alignment = tmp.alignment;
            var color = tmp.color;
            var text = tmp.text;
            var rectTransform = go.GetComponent<RectTransform>();
            var outline = go.GetComponent<Outline>();

            // حذف کامپوننت قدیمی
            Object.DestroyImmediate(tmp);

            // اضافه کردن RTLTextMeshPro
            var rtl = go.AddComponent<RTLTMPro.RTLTextMeshPro>();
            rtl.font = font;
            rtl.fontSize = fontSize;
            rtl.alignment = alignment;
            rtl.color = color;
            rtl.text = text;

            converted++;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[TehranCity] Converted {converted} TextMeshProUGUI components to RTLTextMeshPro.");
    }
}