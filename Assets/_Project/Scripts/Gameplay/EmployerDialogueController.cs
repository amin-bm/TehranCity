using System.Linq;
using UnityEngine;

public class EmployerDialogueController : MonoBehaviour, IInteractable
{
    public InterviewDialogueSO dialogue;
    public DialogueCameraController dialogueCamera;
    public ShiftController shiftController;

    public string prompt = "صحبت با کارفرما";
    public string shortLineAfterJob = "خوبه، برو سر کارت. مشتری‌ها رو زیاد منتظر نذار.";

    public string Prompt => prompt;
    public string InteractionPrompt => prompt;
    public string PromptText => prompt;
    public bool CanInteract => shiftController == null || !shiftController.IsRunning;

    private void Awake()
    {
        if (dialogue == null)
            dialogue = Resources.FindObjectsOfTypeAll<InterviewDialogueSO>().FirstOrDefault();
    }

    public string GetPrompt() => prompt;
    public string GetInteractionPrompt() => prompt;
    public string GetPromptText() => prompt;

    public void Interact() => StartInterview();
    public void Interact(GameObject interactor) => StartInterview();
    public void Interact(Component interactor) => StartInterview();
    public void Interact(PlayerController interactor) => StartInterview();
    public void OnInteract() => StartInterview();
    public void OnInteract(GameObject interactor) => StartInterview();

    public void StartInterview()
    {
        if (DialogueUI.Instance == null || DialogueUI.Instance.IsOpen)
            return;

        if (dialogueCamera != null)
            dialogueCamera.Show();

        if (ServiceBridge.GetFlag(GameFlags.JobAccepted))
        {
            if (shiftController != null && !shiftController.IsRunning &&
                !ServiceBridge.GetFlag(GameFlags.FirstShiftCompleted))
            {
                ShowShiftIntro(true);
                return;
            }

            var speaker = dialogue != null ? dialogue.employerName : "کارفرما";
            DialogueUI.Instance.ShowLine(speaker, shortLineAfterJob, HideCamera);
            return;
        }

        if (dialogue == null)
        {
            HideCamera();
            return;
        }

        DialogueUI.Instance.ShowInterview(dialogue, OnAcceptedClosed, OnRejectedClosed);
    }

    private void OnAcceptedClosed()
    {
        ServiceBridge.SetFlag(GameFlags.JobAccepted, true);
        ServiceLocator.EventBus.Publish(new JobAcceptedEvent());
        HideCamera();

        // شروع خودکار شیفت، بلافاصله بعد از قبول کار
        ShowShiftIntro(false);
    }

    private void OnRejectedClosed()
    {
        HideCamera();
    }

    private void ShowShiftIntro(bool hideCameraOnClose)
    {
        if (shiftController == null)
        {
            Debug.LogWarning("[Shift] ShiftController وصل نیست! منوی «TehranCity/Setup/13) Shift Minigame + Food Shop» را اجرا کن.");
            return;
        }

        if (shiftController.IsRunning || ServiceBridge.GetFlag(GameFlags.FirstShiftCompleted))
            return;

        var speaker = dialogue != null ? dialogue.employerName : "کارفرما";
        DialogueUI.Instance.ShowLine(speaker,
            "خب، وقت شیفته. حواست به صندوق، قفسه و مشتری‌ها باشه.",
            () =>
            {
                if (hideCameraOnClose) HideCamera();
                shiftController.Begin();
            });
    }

    private void HideCamera()
    {
        if (dialogueCamera != null)
            dialogueCamera.Hide();
    }
}