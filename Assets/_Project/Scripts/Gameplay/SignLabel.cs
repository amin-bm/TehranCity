using RTLTMPro;
using UnityEngine;

/// <summary>تابلوهای 3D: متن خام فارسی -> RTLTextMeshPro3D (خودش shaping و RTL می‌کند).</summary>
public class SignLabel : MonoBehaviour
{
    public string text = "";

    private void Awake()
    {
        var tmp = GetComponent<RTLTextMeshPro3D>();
        if (tmp != null)
        {
            tmp.text = text; // خام؛ بدون FaText
        }
        else
        {
            Debug.LogError($"[Sign] '{name}' کامپوننت RTLTextMeshPro3D ندارد!");
        }
    }
}