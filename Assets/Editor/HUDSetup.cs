using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class HUDSetup
{
    private const string BootstrapPath = "Assets/_Project/Scenes/Bootstrap.unity";
    private const string RoomPath = "Assets/_Project/Scenes/Interior_Room01.unity";
    private const string FontPath = "Assets/_Project/Art/UI/Fonts/TMP_Tahoma_Persian.asset";

    [MenuItem("TehranCity/Setup/9) Setup HUD + Bootstrap (Services)")]
    public static void Setup()
    {
        EditorSceneManager.SaveOpenScenes();
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null) { Debug.LogError("[TehranCity] Font not found. Run Setup 7 first."); return; }

        // ---- Bootstrap: GameManager ----
        var bootstrap = EditorSceneManager.OpenScene(BootstrapPath);
        if (GameObject.Find("GameManager") == null)
        {
            var gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
            var so = new SerializedObject(gm.GetComponent<GameManager>());
            so.FindProperty("nextScene").stringValue = "Interior_Room01";
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        EditorSceneManager.SaveScene(bootstrap);

        // ---- اتاق: HUD ----
        var room = EditorSceneManager.OpenScene(RoomPath);

        var old = GameObject.Find("UI_HUD");
        if (old != null) Object.DestroyImmediate(old);

        var canvasGo = new GameObject("UI_HUD");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGo.AddComponent<GraphicRaycaster>();
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var money = CreateText(canvasGo.transform, "MoneyText", font, 34, new Vector2(-60, -50));
        var time = CreateText(canvasGo.transform, "TimeText", font, 30, new Vector2(-60, -105));
        var needs = CreateText(canvasGo.transform, "NeedsText", font, 28, new Vector2(-60, -155));

        var hud = canvasGo.AddComponent<HUDController>();
        var soHud = new SerializedObject(hud);
        soHud.FindProperty("moneyText").objectReferenceValue = money;
        soHud.FindProperty("timeText").objectReferenceValue = time;
        soHud.FindProperty("needsText").objectReferenceValue = needs;
        soHud.FindProperty("faFont").objectReferenceValue = font;
        soHud.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(room);
        Debug.Log("[TehranCity] HUD + Bootstrap ready. Play from Bootstrap!");
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, TMP_FontAsset font, float size, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f); // بالا-راست
        rt.sizeDelta = new Vector2(900, 60);
        rt.anchoredPosition = pos;

        var tmp = go.AddComponent<TextMeshProUGUI>();   // به‌جای RTLTextMeshPro
        tmp.font = font;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.color = Color.white;

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);
        return tmp;
    }
}