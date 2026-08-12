using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TMP_Text toastText;

    private Coroutine _toastRoutine;

    private void Awake()
    {
        if (promptText != null) promptText.alignment = TextAlignmentOptions.Right;
        if (toastText != null) toastText.alignment = TextAlignmentOptions.Right;

        // لاگ تشخیص مسیر: هر دو باید RTLTextMeshPro باشند
        Debug.Log($"[InteractionUI] promptText = {promptText.GetType().Name} | toastText = {toastText.GetType().Name}");
    }

    public void ShowPrompt(string prompt)
    {
        // فارسی اول تا جهت پاراگراف RTL تشخیص داده شود؛ [E] انتهای خط می‌نشیند.
        promptText.text = prompt + "  [E]";
        promptText.gameObject.SetActive(true);
    }

    public void HidePrompt() => promptText.gameObject.SetActive(false);

    public void ShowToast(string message, float duration = 2.5f)
    {
        if (_toastRoutine != null) StopCoroutine(_toastRoutine);
        _toastRoutine = StartCoroutine(Toast(message, duration));
    }

    private System.Collections.IEnumerator Toast(string message, float duration)
    {
        toastText.text = message;
        toastText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(duration);
        toastText.gameObject.SetActive(false);
        _toastRoutine = null;
    }
}