using UnityEngine;

/// <summary>
/// تعامل با صاحب‌خانه برای پرداخت اجاره ماهانه (۷ میلیون تومان).
/// مسیر: Assets/_Project/Scripts/Gameplay/LandlordInteractable.cs
/// </summary>
public class LandlordInteractable : MonoBehaviour, IInteractable
{
    public long rentAmount = 7_000_000L;
    public string prompt = "پرداخت اجاره";

    public string Prompt => prompt;
    public bool CanInteract => true;

    public void OnInteract(GameObject interactor)
    {
        if (ServiceBridge.GetFlag(GameFlags.RentPaidDay7))
        {
            DialogueUI.Instance.ShowLine("صاحب‌خانه",
                "اجاره‌ی این ماه رو که دادی. دمِت گرم. فعلاً خونه مال توئه.", null);
            return;
        }

        long cash = ServiceBridge.GetBalance();
        if (cash >= rentAmount)
        {
            DialogueUI.Instance.ShowChoice("صاحب‌خانه",
                $"اجاره این ماه {PersianFormat.Money(rentAmount)} تومانه. پرداخت می‌کنی؟",
                "پرداخت می‌کنم", "الان نه",
                "", "",
                PayRent, null);
        }
        else
        {
            DialogueUI.Instance.ShowLine("صاحب‌خانه",
                $"پولت کافی نیست. اجاره {PersianFormat.Money(rentAmount)} تومانه. تو جیبت {PersianFormat.Money(cash)} تومنه. زودتر جورش کن وگرنه باید جمع کنی بری.", null);
        }
    }

    private void PayRent()
    {
        if (ServiceBridge.TrySpend(rentAmount, "RentPayment"))
        {
            ServiceBridge.SetFlag(GameFlags.RentPaidDay7, true);
            ServiceBridge.SetFlag(GameFlags.EvictionWarningActive, false);
            DialogueUI.Instance.ShowLine("صاحب‌خانه",
                "رسید پرداخت. فعلاً که جات امنه. ولی ماه بعد یادت نره.", null);
            Debug.Log("[Landlord] Rent paid successfully.");
        }
        else
        {
            DialogueUI.Instance.ShowLine("صاحب‌خانه",
                "تراکنش ناموفق بود. پولت کم اومد یا مشکلی پیش اومد.", null);
        }
    }
}