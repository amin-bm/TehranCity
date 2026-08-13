using UnityEngine;

/// <summary>hook هدف ۱۵ میلیونی (ADR-004): نمایش پیشرفت پس‌انداز با اعداد حروفی (سطح ۱ قانون متن).</summary>
public class TinyShopHook : MonoBehaviour, IInteractable
{
    public long targetPrice = 15_000_000L; // سند Economy

    public string Prompt => "مغازه خیلی کوچک";
    public bool CanInteract => true;

    public void OnInteract(GameObject interactor)
    {
        long cash = ServiceBridge.GetBalance();

        if (cash >= targetPrice)
        {
            DialogueUI.Instance.ShowLine("صاحب مغازه",
                "پولت کامله؟ دمت گرم. اجاره این مغازه مال توئه... به‌زودی در نسخه کامل!", null);
        }
        else
        {
            int percent = (int)Mathf.Clamp(cash * 100L / targetPrice, 0L, 100L);
            DialogueUI.Instance.ShowLine("صاحب مغازه",
                $"برای اجاره این مغازه به پانزده میلیون تومان نیاز داری. فعلاً {PersianFormat.IntWords(percent)} درصد مسیر رو اومدی. ادامه بده!", null);
        }
    }
}