using UnityEngine;

/// <summary>
/// اینتراکتبل ساده Greybox. جریان‌های واقعی بعداً از طریق Service/EventBus می‌روند.
/// </summary>
public class SimpleInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "تعامل";
    [SerializeField] private string toastMessage = "";

    public string Prompt => prompt;
    public bool CanInteract => true;

    public void OnInteract(GameObject interactor)
    {
        if (!string.IsNullOrEmpty(toastMessage))
        {
            var ui = Object.FindFirstObjectByType<InteractionUI>();
            if (ui != null) ui.ShowToast(toastMessage);
        }
        Debug.Log($"[Interact] {gameObject.name} by {interactor.name}");
    }
}