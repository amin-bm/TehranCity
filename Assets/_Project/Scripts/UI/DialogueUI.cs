using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using RTLTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// پنل دیالوگ Visual-Novel — کانواس Overlay مستقل بالای همه‌چیز.
/// حالت‌ها: Line / Choice / Result (مصاحبه، خط ساده، انتخاب خرید و...).
/// نسخه نهایی یکپارچه — بدون گارد _data در ShowChoices.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    private enum State { Hidden, Line, Choice, Result }

    private static DialogueUI _instance;
    public static DialogueUI Instance { get { EnsureInstance(); return _instance; } }
    public bool IsOpen => _state != State.Hidden && _panel != null && _panel.activeSelf;

    private Canvas _canvas;
    private GameObject _panel;
    private CanvasGroup _panelCanvasGroup;

    private RTLTextMeshPro _nameText;
    private RTLTextMeshPro _bodyText;
    private RTLTextMeshPro _hintText;

    private GameObject _choicesRoot;
    private Button _acceptButton;
    private Button _rejectButton;
    private RTLTextMeshPro _acceptLabel;
    private RTLTextMeshPro _rejectLabel;

    private TMP_FontAsset _font;

    private State _state = State.Hidden;
    private InterviewDialogueSO _data;
    private bool _choiceMode;
    private string _rtAcceptedLine;
    private string _rtRejectedLine;

    private Action _acceptedCallback;
    private Action _rejectedCallback;
    private Action _lineClosedCallback;

    private bool _wasAccepted;
    private bool _wasRejected;

    private float _inputUnlockTime;
    private readonly List<Behaviour> _disabledBehaviours = new List<Behaviour>();

    private static void EnsureInstance()
    {
        if (_instance != null) return;
        var existing = FindObjectsByType<DialogueUI>(FindObjectsSortMode.None);
        if (existing.Length > 0) { _instance = existing[0]; return; }
        var go = new GameObject("DialogueUI_Root");
        _instance = go.AddComponent<DialogueUI>();
    }

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) { Destroy(gameObject); return; }
        if (transform.parent == null) DontDestroyOnLoad(gameObject);
        EnsureBuilt();
    }

    private void OnDestroy() { if (_instance == this) _instance = null; }

    private void Update()
    {
        if (_state == State.Hidden || _panel == null || !_panel.activeSelf) return;
        var kb = Keyboard.current;
        if (kb == null) return;
        if (Time.unscaledTime < _inputUnlockTime) return;

        bool advance = kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;

        if (_state == State.Line) { if (advance) AdvanceLine(); }
        else if (_state == State.Choice)
        {
            bool accept = kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame || kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame;
            bool reject = kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame;
            if (accept) ChooseAccept();
            else if (reject) ChooseReject();
        }
        else if (_state == State.Result) { if (advance) Close(); }
    }

    /* ================= API ================= */

    public void ShowInterview(InterviewDialogueSO data, Action onAcceptedClosed, Action onRejectedClosed)
    {
        if (data == null) return;
        EnsureBuilt();
        if (_state != State.Hidden) return;

        _data = data;
        _choiceMode = true;
        _rtAcceptedLine = null; _rtRejectedLine = null;
        _acceptedCallback = onAcceptedClosed;
        _rejectedCallback = onRejectedClosed;
        _lineClosedCallback = null;
        _wasAccepted = false; _wasRejected = false;

        _acceptLabel.text = data.acceptLabel;
        _rejectLabel.text = data.rejectLabel;

        Open();
        _nameText.text = data.employerName;
        _bodyText.text = data.employerLine;
        _hintText.text = "ادامه  [E]";
        _choicesRoot.SetActive(false);
        _state = State.Line;
    }

    public void ShowLine(string speaker, string text, Action onClose = null)
    {
        EnsureBuilt();
        if (_state != State.Hidden) return;

        _data = null;
        _choiceMode = false;
        _rtAcceptedLine = null; _rtRejectedLine = null;
        _acceptedCallback = null; _rejectedCallback = null; _lineClosedCallback = onClose;
        _wasAccepted = false; _wasRejected = false;

        Open();
        _nameText.text = speaker ?? string.Empty;
        _bodyText.text = text ?? string.Empty;
        _hintText.text = "ادامه  [E]";
        _choicesRoot.SetActive(false);
        _state = State.Line;
    }

    public void ShowChoice(string speaker, string text, string acceptLabel, string rejectLabel,
        string acceptedLine, string rejectedLine, Action onAccepted, Action onRejected)
    {
        EnsureBuilt();
        if (_state != State.Hidden) return;

        _data = null;
        _choiceMode = true;
        _rtAcceptedLine = acceptedLine;
        _rtRejectedLine = rejectedLine;
        _acceptedCallback = onAccepted;
        _rejectedCallback = onRejected;
        _lineClosedCallback = null;
        _wasAccepted = false; _wasRejected = false;

        _acceptLabel.text = acceptLabel;
        _rejectLabel.text = rejectLabel;

        Open();
        _nameText.text = speaker ?? string.Empty;
        _bodyText.text = text ?? string.Empty;
        _hintText.text = "ادامه  [E]";
        _choicesRoot.SetActive(false);
        _state = State.Line;
    }

    /* ================= داخلی ================= */

    private void Open()
    {
        EnsureEventSystem();
        _panel.SetActive(true);
        _panel.transform.SetAsLastSibling();
        if (_panelCanvasGroup != null)
        {
            _panelCanvasGroup.alpha = 1f;
            _panelCanvasGroup.interactable = true;
            _panelCanvasGroup.blocksRaycasts = true;
        }

        var interactionUI = UnityEngine.Object.FindFirstObjectByType<InteractionUI>();
        if (interactionUI != null) interactionUI.HidePrompt();

        _inputUnlockTime = Time.unscaledTime + 0.25f;
        ServiceBridge.PushTimeMode("Dialogue");
        SetGameplayInputEnabled(false);
    }

    private void Close()
    {
        if (_state == State.Hidden) return;

        var acceptedCb = _acceptedCallback;
        var rejectedCb = _rejectedCallback;
        var lineCb = _lineClosedCallback;
        bool wasAccepted = _wasAccepted;
        bool wasRejected = _wasRejected;

        _state = State.Hidden;
        _panel.SetActive(false);

        ServiceBridge.PopTimeMode();
        SetGameplayInputEnabled(true);

        _data = null;
        _acceptedCallback = null; _rejectedCallback = null; _lineClosedCallback = null;
        _wasAccepted = false; _wasRejected = false;

        if (wasAccepted) acceptedCb?.Invoke();
        else if (wasRejected) rejectedCb?.Invoke();
        else lineCb?.Invoke();
    }

    private void AdvanceLine()
    {
        if (_state != State.Line) return;
        if (_choiceMode) ShowChoices();
        else Close();
    }

    private void ShowChoices()
    {
        if (_state != State.Line) return; // بدون گارد _data — برای ShowChoice لازم است
        _state = State.Choice;
        _choicesRoot.SetActive(true);
        _hintText.text = "کلید یک: قبول — کلید دو: بعداً";
        _inputUnlockTime = Time.unscaledTime + 0.2f;
    }

    private void ChooseAccept()
    {
        if (_state != State.Choice) return;
        _wasAccepted = true; _wasRejected = false;
        var line = _data != null ? _data.acceptedLine : _rtAcceptedLine;
        if (string.IsNullOrEmpty(line)) { Close(); return; }
        _state = State.Result;
        _choicesRoot.SetActive(false);
        _bodyText.text = line;
        _hintText.text = "ادامه  [E]";
        _inputUnlockTime = Time.unscaledTime + 0.25f;
    }

    private void ChooseReject()
    {
        if (_state != State.Choice) return;
        _wasAccepted = false; _wasRejected = true;
        var line = _data != null ? _data.rejectedLine : _rtRejectedLine;
        if (string.IsNullOrEmpty(line)) { Close(); return; }
        _state = State.Result;
        _choicesRoot.SetActive(false);
        _bodyText.text = line;
        _hintText.text = "ادامه  [E]";
        _inputUnlockTime = Time.unscaledTime + 0.25f;
    }

    /* ================= ساخت UI ================= */

    private void EnsureBuilt()
    {
        if (_panel != null) return;

        var canvasGO = new GameObject("DialogueCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 950;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        EnsureEventSystem();
        _font = ResolveFont();
        BuildPanel();
        _panel.SetActive(false);
    }

    private void BuildPanel()
    {
        _panel = new GameObject("DialoguePanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(CanvasGroup));
        _panel.transform.SetParent(_canvas.transform, false);

        var rt = _panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 50f);
        rt.sizeDelta = new Vector2(1200f, 380f);

        var image = _panel.GetComponent<Image>();
        image.color = new Color(0.03f, 0.03f, 0.06f, 0.88f);
        image.raycastTarget = true;

        var vlg = _panel.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(28, 28, 18, 18);
        vlg.spacing = 12f;
        vlg.childAlignment = TextAnchor.UpperRight;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        _panelCanvasGroup = _panel.GetComponent<CanvasGroup>();

        _nameText = CreateRTLText(_panel.transform, "NameText", string.Empty, 30, TextAlignmentOptions.TopRight, new Color(1f, 0.85f, 0.25f));
        _nameText.gameObject.AddComponent<LayoutElement>().preferredHeight = 40f;

        _bodyText = CreateRTLText(_panel.transform, "BodyText", string.Empty, 36, TextAlignmentOptions.TopRight, Color.white);
        var bodyLE = _bodyText.gameObject.AddComponent<LayoutElement>();
        bodyLE.minHeight = 150f;
        bodyLE.preferredHeight = 150f;

        _hintText = CreateRTLText(_panel.transform, "HintText", string.Empty, 22, TextAlignmentOptions.TopRight, new Color(0.75f, 0.75f, 0.75f));
        _hintText.gameObject.AddComponent<LayoutElement>().preferredHeight = 32f;

        _choicesRoot = new GameObject("Choices", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        _choicesRoot.transform.SetParent(_panel.transform, false);
        _choicesRoot.GetComponent<LayoutElement>().preferredHeight = 92f;

        var hlg = _choicesRoot.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 24f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        _acceptButton = CreateButton(_choicesRoot.transform, "AcceptButton", ChooseAccept);
        _rejectButton = CreateButton(_choicesRoot.transform, "RejectButton", ChooseReject);

        _acceptLabel = _acceptButton.GetComponentInChildren<RTLTextMeshPro>();
        _rejectLabel = _rejectButton.GetComponentInChildren<RTLTextMeshPro>();

        _choicesRoot.SetActive(false);
    }

    private RTLTextMeshPro CreateRTLText(Transform parent, string name, string initialText, int fontSize, TextAlignmentOptions alignment, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<RTLTextMeshPro>();
        if (_font != null) text.font = _font;
        text.text = initialText;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    private Button CreateButton(Transform parent, string name, Action onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var img = go.GetComponent<Image>();
        img.color = new Color(0.13f, 0.13f, 0.18f, 0.96f);
        img.raycastTarget = true;

        var button = go.GetComponent<Button>();
        button.targetGraphic = img;

        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 320f;
        le.preferredHeight = 80f;

        var labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(go.transform, false);
        var label = labelGO.AddComponent<RTLTextMeshPro>();
        if (_font != null) label.font = _font;
        label.text = string.Empty;
        label.fontSize = 28;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;

        var lrt = labelGO.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(10f, 4f);
        lrt.offsetMax = new Vector2(-10f, -4f);

        button.onClick.AddListener(() => onClick?.Invoke());
        return button;
    }

    private void SetGameplayInputEnabled(bool enabled)
    {
        if (!enabled)
        {
            _disabledBehaviours.Clear();
            foreach (var pc in FindObjectsByType<PlayerController>(FindObjectsSortMode.None)) DisableBehaviour(pc);
            foreach (var inter in FindObjectsByType<Interactor>(FindObjectsSortMode.None)) DisableBehaviour(inter);
        }
        else
        {
            foreach (var b in _disabledBehaviours) if (b != null) b.enabled = true;
            _disabledBehaviours.Clear();
        }
    }

    private void DisableBehaviour(Behaviour b)
    {
        if (b == null) return;
        if (b.enabled) { b.enabled = false; _disabledBehaviours.Add(b); }
    }

    private static void EnsureEventSystem()
    {
        var systems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        var es = systems.Length > 0 ? systems[0] : null;

        if (es == null)
        {
            var go = new GameObject("EventSystem");
            es = go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
            DontDestroyOnLoad(go); // <= ماندگار بین همه‌ی صحنه‌ها
            return;
        }

        if (es.GetComponent<BaseInputModule>() == null)
            es.gameObject.AddComponent<InputSystemUIInputModule>();

        // اگر EventSystem صحنه‌ایِ روت است، آن را هم ماندگار کن تا با unload نابود نشود
        if (es.transform.parent == null && es.gameObject.scene.name != "DontDestroyOnLoad")
            DontDestroyOnLoad(es.gameObject);
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