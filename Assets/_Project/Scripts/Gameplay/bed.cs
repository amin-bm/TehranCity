using UnityEngine;

/// <summary>
/// تخت اتاق: خواب = skip به صبح فردا ۰۷:۳۰ (Sleep=skip سند) + ریست نیازها
/// + پیام روزانه‌ی صاحب‌خانه + فشار استرس اگر پیام ندیده مانده باشد.
/// اعداد ریست: data-driven — ثبت در Changelog سند Economy.
/// </summary>
public class Bed : MonoBehaviour, IInteractable
{
    [Header("Wake balance (اعداد جدید — ثبت در سند Economy)")]
    public float energyOnWake = 90f;
    public float stressDeltaOnWake = -30f;
    public float hungerDeltaOnWake = -20f;
    public float socialDeltaOnWake = -5f;
    public float unseenLandlordStress = 4f;

    public string prompt = "خوابیدن";
    public string Prompt => prompt;
    public bool CanInteract => true;

    public void OnInteract(GameObject interactor)
    {
        DialogueUI.Instance.ShowChoice("آرش",
            "بخوابم؟ فردا ساعت ۰۷:۳۰ بیدار می‌شم.",
            "می‌خوابم", "نه فعلاً",
            "", "",
            Sleep, null);
    }

    private void Sleep()
    {
        var time = ServiceLocator.TimeService;

        // فشار اجاره: پیام دیروز ندیده مانده؟
        var phone = FindFirstObjectByType<PhoneController>(FindObjectsInactive.Include);
        Debug.Log($"[Bed] phone instance={(phone != null ? phone.GetInstanceID().ToString() : "null")}");

        if (phone != null && PhoneController.ConsumeUnseenAny())
        {
            ServiceBridge.AddNeed("Stress", unseenLandlordStress);
            Debug.Log("[Landlord] پیام صاحب‌خانه ندیده ماند -> +استرس.");
        }

        // Sleep = skip به صبح فردا
        time.SetDateTime(time.Day + 1, 7, 30);

        // ریست نیازها
        ServiceBridge.AddNeed("Energy", energyOnWake - ServiceBridge.GetNeed("Energy"));
        ServiceBridge.AddNeed("Stress", stressDeltaOnWake);
        ServiceBridge.AddNeed("Hunger", hungerDeltaOnWake);
        ServiceBridge.AddNeed("Social", socialDeltaOnWake);

        // روایت فشار اجاره: پیام صبحگاهی صاحب‌خانه
        PhoneController.BroadcastRuntime("صاحب‌خانه", LandlordLine(time.Day));

        Debug.Log($"[Sleep] بیدار شدی @ روز {time.Day} | ۰۷:۳۰");
        // Save + خلاصه‌ی پایان روز: خودکار توسط SaveDirector (تشخیص تغییر روز)
    }

    private static string LandlordLine(int day)
    {
        if (day <= 2) return "اجاره‌ی این ماه یادت نره. آخر هفته باید برسه دستم.";
        if (day == 3) return "هنوز اجاره نیومده. دیر بشه، مجبوریم فکرای دیگه بکنیم.";
        return "آخرین فرصته. وگرنه کلید رو پس می‌گیرم.";
    }
}