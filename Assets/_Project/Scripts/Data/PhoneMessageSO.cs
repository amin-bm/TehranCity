using UnityEngine;

[CreateAssetMenu(menuName = "TehranCity/Phone Message", fileName = "PhoneMessage")]
public class PhoneMessageSO : ScriptableObject
{
    public string messageId;
    public string senderName;
    [TextArea(3, 8)] public string body;
    [Tooltip("کلید flag برای Save (اختیاری)")] public string flagKey = "";
}