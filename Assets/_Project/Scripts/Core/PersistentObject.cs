using UnityEngine;

/// <summary>
/// آبجکت ماندگار بین صحنه‌ها.
/// اگر نسخه ماندگار همین نام از قبل وجود داشته باشد (reload صحنه)، کپی جدید نابود می‌شود.
/// </summary>
public class PersistentObject : MonoBehaviour
{
    private void Awake()
    {
        var all = FindObjectsByType<PersistentObject>(FindObjectsSortMode.None);
        foreach (var o in all)
        {
            if (o != this &&
                o.gameObject.name == gameObject.name &&
                o.gameObject.scene.name == "DontDestroyOnLoad")
            {
                // این آبجکت، کپی صحنه است؛ نسخه ماندگار از قبل هست
                Destroy(gameObject);
                return;
            }
        }
        DontDestroyOnLoad(gameObject);
    }
}