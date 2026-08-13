using UnityEngine;

public enum ShiftStationKind { Register, Shelf, Customer }

/// <summary>ایستگاه وظیفه در مینی‌گیم شیفت؛ فقط وقتی «وظیفه فعلی» است قابل تعامل است.</summary>
public class ShiftStation : MonoBehaviour, IInteractable
{
    public ShiftStationKind kind;
    public string promptText = "ایستگاه";
    public bool active; // توسط ShiftController کنترل می‌شود

    public string Prompt => promptText;
    public bool CanInteract => active;

    public void OnInteract(GameObject interactor)
    {
        var ctrl = FindFirstObjectByType<ShiftController>();
        if (ctrl != null) ctrl.OnStationCompleted(this);
    }
}