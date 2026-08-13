using UnityEngine;

/// <summary>خرید غذای ارزان از رفاه‌مارکت (قدم ۱۱) — قیمت از سند Economy.</summary>
public class FoodShopInteractable : MonoBehaviour, IInteractable
{
    public string prompt = "رفاه‌مارکت: خرید غذا";
    public long price = 120000L; // غذای ارزان — سند Economy
    [Header("Effect (عدد جدید — ثبت در Changelog سند Economy)")]
    public float hungerRestore = 25f; // غذای ارزان: +۲۵ گرسنگی

    public string Prompt => prompt;
    public bool CanInteract => true;

    public void OnInteract(GameObject interactor)
    {
        DialogueUI.Instance.ShowChoice(
            "فروشنده رفاه‌مارکت",
            "غذای ارزان می‌خوای؟ صد و بیست هزار تومان. می‌خری؟",
            "می‌خرم",
            "نه، فعلاً",
            "", // بدون خط نتیجه؛ نتیجه در OnBuy تعیین می‌شود
            "باشه، هر وقت گرسنت شد بیا.",
            OnBuy,
            null);
    }

    private void OnBuy()
    {
        if (ServiceBridge.TrySpend(price, "FoodCheap"))
        {
            Debug.Log($"[Food] خرید غذا {price} تومان.");
            DialogueUI.Instance.ShowLine("فروشنده رفاه‌مارکت", "بفرما، نوش جان. حالا برو سراغ زندگی.", null);
        }
        else
        {
            DialogueUI.Instance.ShowLine("فروشنده رفاه‌مارکت", "پولت کافی نیست دوست من. اول یه شیفت بزن.", null);
        }
        ServiceBridge.AddNeed("Hunger", hungerRestore);
    }
}