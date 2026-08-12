using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class InteractionSetup
{
    private const string ScenePath = "Assets/_Project/Scenes/Interior_Room01.unity";
    private const string FontPath = "Assets/_Project/Art/UI/Fonts/TMP_Tahoma_Persian.asset";

    [MenuItem("TehranCity/Setup/8) Setup Interaction + Prompt UI")]
    public static void Setup()
    {
        EditorSceneManager.SaveOpenScenes();
        var scene = EditorSceneManager.OpenScene(ScenePath);

        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("[TehranCity] Player not found."); return; }
        if (player.GetComponent<Interactor>() == null)
            player.AddComponent<Interactor>();

        MakeInteractable("Interact_Phone", "برداشتن گوشی", "گوشی برداشته شد (دمو)");
        MakeInteractable("Mirror", "نگاه به آینه", "قیافت خسته‌ست... (دمو)");

        // Canvas
        var canvasGo = GameObject.Find("UI_Canvas");
        if (canvasGo == null)
        {
            canvasGo = new GameObject("UI_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<GraphicRaycaster>();
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null) { Debug.LogError("[TehranCity] Font not found. Run Setup 7 first."); return; }

        var prompt = CreateText(canvasGo.transform, "PromptText", font, 40, new Vector2(0.5f, 0f), new Vector2(1000, 90), new Vector2(0f, 100f));
        var toast = CreateText(canvasGo.transform, "ToastText", font, 34, new Vector2(0.5f, 1f), new Vector2(1000, 90), new Vector2(0f, -100f));

        var ui = canvasGo.GetComponent<InteractionUI>() ?? canvasGo.AddComponent<InteractionUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("promptText").objectReferenceValue = prompt;
        so.FindProperty("toastText").objectReferenceValue = toast;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[TehranCity] Interaction + Prompt UI ready.");
    }

    private static void MakeInteractable(string objName, string prompt, string toast)
    {
        var go = GameObject.Find(objName);
        if (go == null) { Debug.LogWarning($"[TehranCity] {objName} not found."); return; }
        var si = go.GetComponent<SimpleInteractable>() ?? go.AddComponent<SimpleInteractable>();
        var so = new SerializedObject(si);
        so.FindProperty("prompt").stringValue = prompt;
        so.FindProperty("toastMessage").stringValue = toast;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, TMP_FontAsset font, float size,
    Vector2 anchor, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var old = parent.Find(name);
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;

        var tmp = go.AddComponent<RTLTMPro.RTLTextMeshPro>(); // ← تنها تغییر
        tmp.font = font;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.color = Color.white;
        tmp.text = "";

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);

        go.SetActive(false);
        return tmp;
    }
}