using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// حالت Clip (قدم ۱۳): با F9 همه‌ی UI پنهان/آشکار می‌شود.
/// فیکس: رفرنس UI_Canvas کش می‌شود؛ GameObject.Find آبجکت غیرفعال را نمی‌یابد.
/// </summary>
public class ClipModeDirector : MonoBehaviour
{
    private bool _clip;
    private GameObject _uiCanvas;
    private bool _shiftWasActive;
    private bool _summaryWasActive;

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.f9Key.wasPressedThisFrame)
            Toggle();
    }

    private void Toggle()
    {
        _clip = !_clip;

        // UI_Canvas (HUD + پرامپت‌ها): با رفرنس کش‌شده، حتی وقتی غیرفعال است
        if (_uiCanvas == null)
            _uiCanvas = FindAnywhere("UI_Canvas");
        if (_uiCanvas != null)
            _uiCanvas.SetActive(!_clip);

        var shift = FindFirstObjectByType<ShiftHUD>(FindObjectsInactive.Include);
        if (shift != null)
        {
            if (_clip)
            {
                _shiftWasActive = shift.gameObject.activeSelf;
                shift.gameObject.SetActive(false);
            }
            else if (_shiftWasActive)
            {
                shift.gameObject.SetActive(true);
                _shiftWasActive = false;
            }
        }

        var summary = FindFirstObjectByType<DaySummaryUI>(FindObjectsInactive.Include);
        if (summary != null)
        {
            if (_clip)
            {
                _summaryWasActive = summary.gameObject.activeSelf;
                summary.gameObject.SetActive(false);
            }
            else if (_summaryWasActive)
            {
                summary.gameObject.SetActive(true);
                _summaryWasActive = false;
            }
        }

        Debug.Log($"[Clip] clipMode={_clip}");
    }

    /// <summary>پیدا کردن آبجکت حتی غیرفعال، در همه‌ی صحنه‌ها (از جمله DontDestroyOnLoad).</summary>
    private static GameObject FindAnywhere(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) return go;

        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (!s.isLoaded) continue;
            foreach (var root in s.GetRootGameObjects())
            {
                if (root.name == name) return root;
                var child = root.transform.Find(name);
                if (child != null) return child.gameObject;
            }
        }
        return null;
    }
}