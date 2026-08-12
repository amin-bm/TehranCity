using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text needsText;
    [SerializeField] private TMP_FontAsset faFont;

    private bool _driveSimFallback;
    private float _uiAcc;

    private void Awake()
    {
        ServiceLocator.Ensure();

        // HUD = خط‌های عدددار => FaText + TMP معمولی (نه RTLTMPro)
        foreach (var t in new[] { moneyText, timeText, needsText })
            if (t != null && faFont != null && t.font != faFont) t.font = faFont;

        _driveSimFallback = FindFirstObjectByType<GameManager>() == null;

        var bus = ServiceLocator.EventBus;
        bus.Subscribe<BalanceChangedEvent>(OnBalance);
        bus.Subscribe<NeedsChangedEvent>(OnNeeds);
        bus.Subscribe<DayChangedEvent>(OnDay);

        RefreshMoney(); RefreshTime(); RefreshNeeds();
        Debug.Log($"[HUD] Awake | GameManager={(FindFirstObjectByType<GameManager>() != null)} | font={moneyText.font.name}");
    }

    private void OnDestroy()
    {
        if (!ServiceLocator.IsReady) return;
        var bus = ServiceLocator.EventBus;
        bus.Unsubscribe<BalanceChangedEvent>(OnBalance);
        bus.Unsubscribe<NeedsChangedEvent>(OnNeeds);
        bus.Unsubscribe<DayChangedEvent>(OnDay);
    }

    private void Update()
    {
        if (_driveSimFallback)
        {
            float gm = ServiceLocator.TimeService.Tick(Time.deltaTime);
            ServiceLocator.Needs.Tick(gm / 60f);
        }

        _uiAcc += Time.deltaTime;
        if (_uiAcc >= 0.5f) { _uiAcc = 0f; RefreshTime(); }
    }

    private void OnBalance(BalanceChangedEvent e) => RefreshMoney();
    private void OnNeeds(NeedsChangedEvent e) => RefreshNeeds();
    private void OnDay(DayChangedEvent e) => RefreshTime();

    private void RefreshMoney()
    {
        moneyText.text = FaText.Fix($"پول: {PersianNumbers.FormatLong(ServiceLocator.Economy.Balance)} تومان");
    }

    private void RefreshTime()
    {
        var t = ServiceLocator.TimeService;
        timeText.text = FaText.Fix(
            $"روز {PersianNumbers.Digits(t.Day.ToString())} | " +
            $"{PersianNumbers.Digits(t.Hour.ToString("00"))}:{PersianNumbers.Digits(t.Minute.ToString("00"))}");
    }

    private void RefreshNeeds()
    {
        var n = ServiceLocator.Needs;
        needsText.text = FaText.Fix(
            $"انرژی {PersianNumbers.Digits(Mathf.RoundToInt(n.Energy).ToString())} | " +
            $"گرسنگی {PersianNumbers.Digits(Mathf.RoundToInt(n.Hunger).ToString())} | " +
            $"استرس {PersianNumbers.Digits(Mathf.RoundToInt(n.Stress).ToString())} | " +
            $"اجتماعی {PersianNumbers.Digits(Mathf.RoundToInt(n.Social).ToString())}");
    }
}