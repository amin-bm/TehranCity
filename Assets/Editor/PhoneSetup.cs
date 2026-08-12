using System.IO;
using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PhoneSetup
{
    private const string RoomPath = "Assets/_Project/Scenes/Interior_Room01.unity";
    private const string FontPath = "Assets/_Project/Art/UI/Fonts/TMP_Tahoma_Persian.asset";
    private const string SoDir = "Assets/_Project/Data/ScriptableObjects";

    [MenuItem("TehranCity/Setup/10) Setup Phone (Tab) + First Messages")]
    public static void Setup()
    {
        EditorSceneManager.SaveOpenScenes();

        var landlord = CreateMessage("Msg_Landlord", "msg_landlord", "صاحبخانه",
            "سلام، اجاره این ماه هنوز واریز نشده. تا آخر هفته مهلت داری.", "landlordMessageSeen");
        var friend = CreateMessage("Msg_Friend", "msg_friend_saman", "سامان",
            "شنیدم دنبال کاری. پاساژ موبایل‌سنتر یه فروشنده میخواد. بگو سامان فرستادت.", "");

        var room = EditorSceneManager.OpenScene(RoomPath);
        var old = GameObject.Find("UI_Phone");
        if (old != null) Object.DestroyImmediate(old);

        // Canvas همیشه ACTIVE می‌ماند تا PhoneController زنده باشد
        var canvasGo = new GameObject("UI_Phone");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        canvasGo.AddComponent<GraphicRaycaster>();
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // فقط این Root بصری مخفی/نمایش می‌شود
        var phoneRoot = new GameObject("PhoneRoot");
        phoneRoot.transform.SetParent(canvasGo.transform, false);
        var rootRt = phoneRoot.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero; rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero; rootRt.offsetMax = Vector2.zero;

        var dim = new GameObject("Dim");
        dim.transform.SetParent(phoneRoot.transform, false);
        var dimRt = dim.AddComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero; dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero; dimRt.offsetMax = Vector2.zero;
        dim.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        var panel = new GameObject("Panel");
        panel.transform.SetParent(phoneRoot.transform, false);
        var panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = panelRt.anchorMax = panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(700, 860);
        panel.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.14f, 0.97f);

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(panel.transform, false);
        var titleRt = titleGo.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 1f); titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = Vector2.zero;
        titleRt.sizeDelta = new Vector2(0f, 70f);
        var title = titleGo.AddComponent<RTLTextMeshPro>();
        title.font = font; title.fontSize = 40;
        title.alignment = TextAlignmentOptions.Center;
        title.text = "گوشی";

        var content = new GameObject("Messages");
        content.transform.SetParent(panel.transform, false);
        var contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = Vector2.zero; contentRt.anchorMax = Vector2.one;
        contentRt.offsetMin = new Vector2(30f, 30f);
        contentRt.offsetMax = new Vector2(-30f, -80f);
        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 24f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var pc = canvasGo.AddComponent<PhoneController>();
        pc.phonePanel = phoneRoot;
        pc.messagesContainer = content.transform;
        pc.faFont = font;
        pc.messages = new System.Collections.Generic.List<PhoneMessageSO> { landlord, friend };
        Debug.Log($"[TehranCity] Phone wired | msgs={pc.messages.Count} | landlord={landlord != null} | friend={friend != null}");

        phoneRoot.SetActive(false); // Canvas فعال می‌ماند
        EditorSceneManager.SaveScene(room);
        Debug.Log("[TehranCity] Phone (Tab) + first messages ready.");
    }

    private static PhoneMessageSO CreateMessage(string assetName, string id, string sender, string body, string flagKey)
    {
        Directory.CreateDirectory(SoDir);
        string path = $"{SoDir}/{assetName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<PhoneMessageSO>(path);
        if (existing != null) return existing;

        var so = ScriptableObject.CreateInstance<PhoneMessageSO>();
        so.messageId = id;
        so.senderName = sender;
        so.body = body;
        so.flagKey = flagKey;
        AssetDatabase.CreateAsset(so, path);
        return so;
    }
}