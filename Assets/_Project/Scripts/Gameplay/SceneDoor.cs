using UnityEngine;

/// <summary>در تعاملی بین صحنه‌ها (IInteractable).</summary>
public class SceneDoor : MonoBehaviour, IInteractable
{
    public string prompt = "رفتن به...";
    public string targetScene = "";
    public string spawnPointName = "";

    public string Prompt => prompt;
    
    public bool locked;
    public bool CanInteract => !locked;
    public void OnInteract(GameObject interactor)
    {
        Debug.Log($"[Door] Loading '{targetScene}' spawn='{spawnPointName}'");
        SceneTransition.Load(targetScene, spawnPointName);
    }
}