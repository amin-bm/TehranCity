using Unity.Cinemachine;
using UnityEngine;

public class DialogueCameraController : MonoBehaviour
{
    public CinemachineCamera dialogueCamera;
    public int showPriority = 1000;
    public int hidePriority = -1000;

    private void Awake()
    {
        if (dialogueCamera == null)
            dialogueCamera = GetComponentInChildren<CinemachineCamera>(true);

        Hide();
    }

    public void Show()
    {
        if (dialogueCamera == null)
            return;

        dialogueCamera.gameObject.SetActive(true);
        dialogueCamera.enabled = true;
        dialogueCamera.Priority = showPriority;
    }

    public void Hide()
    {
        if (dialogueCamera == null)
            return;

        dialogueCamera.Priority = hidePriority;
        dialogueCamera.enabled = false;
        dialogueCamera.gameObject.SetActive(false);
    }
}