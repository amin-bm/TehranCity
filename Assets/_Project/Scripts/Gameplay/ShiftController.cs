using UnityEngine;

/// <summary>
/// مینی‌گیم شیفت آزمایشی (قدم ۱۱): دنباله وظایف صندوق/قفسه/مشتری در ۴ دقیقه واقعی.
/// پایان شیفت: حقوق 600,000 فقط از EconomyService + رویداد OnSalaryReceived.
/// طبق سند: Phone/Menus = Pause — حین باز بودن گوشی، تایمر و ساعت اسکریپت‌شده شیفت متوقف می‌شوند.
/// </summary>
public class ShiftController : MonoBehaviour
{
    [Header("Refs")]
    public ShiftStation stationRegister;
    public ShiftStation stationShelf;
    public Transform customerSpot;
    public SceneDoor door;

    [Header("Balance (فقط از سند Economy)")]
    public long salary = 600000L;          // دستمزد هر شیفت ساده
    public float shiftDuration = 240f;     // ۴ دقیقه واقعی = ۶ ساعت بازی

    public bool IsRunning { get; private set; }

    private static readonly ShiftStationKind[] Sequence =
    {
        ShiftStationKind.Register, ShiftStationKind.Customer, ShiftStationKind.Shelf,
        ShiftStationKind.Register, ShiftStationKind.Customer, ShiftStationKind.Shelf
    };

    private int _taskIndex;
    private float _remaining;
    private float _scriptedAcc;
    private float _hudAcc = 1f;
    private GameObject _customer;
    private ShiftStation _customerStation;
    private ShiftHUD _hud;

    public void Begin()
    {
        if (IsRunning) return;
        IsRunning = true;
        _taskIndex = 0;
        _remaining = shiftDuration;
        _scriptedAcc = 0f;

        ServiceBridge.PushTimeMode("Scripted");
        if (door != null) door.locked = true;
        _hud = ShiftHUD.Show();
        ActivateCurrent();
        Debug.Log("[Shift] شیفت آزمایشی شروع شد.");
    }

    private void Update()
    {
        if (!IsRunning) return;

        // Phone/Menus = Pause: حین باز بودن گوشی، همه‌چیزِ شیفت نگه داشته می‌شود
        if (ServiceLocator.TimeService.Mode == TimeMode.Phone) return;

        _hudAcc += Time.deltaTime;
        if (_hudAcc >= 0.2f)
        {
            _hudAcc = 0f;
            RefreshHud();
        }

        // پیشروی اسکریپت‌شده ساعت بازی: ۴ دقیقه واقعی = ۶ ساعت بازی => 1.5 GameMinute بر ثانیه
        _scriptedAcc += Time.deltaTime * 1.5f;
        int whole = Mathf.FloorToInt(_scriptedAcc);
        if (whole > 0)
        {
            _scriptedAcc -= whole;
            ServiceLocator.TimeService.AdvanceMinutes(whole);
        }

        _remaining -= Time.deltaTime;
        if (_remaining <= 0f)
            End(false);
    }

    private void RefreshHud()
    {
        if (_hud == null) return;
        float remain = Mathf.Max(0f, _remaining);
        int mm = Mathf.FloorToInt(remain / 60f);
        int ss = Mathf.FloorToInt(remain % 60f);
        int task = Mathf.Min(_taskIndex + 1, Sequence.Length);
        _hud.SetText($"شیفت آزمایشی | وظیفه {Fa(task.ToString())} از {Fa(Sequence.Length.ToString())} | زمان {Fa(mm.ToString("00"))}:{Fa(ss.ToString("00"))}");
    }

    private static string Fa(string s)
    {
        var c = s.ToCharArray();
        for (int i = 0; i < c.Length; i++)
            if (c[i] >= '0' && c[i] <= '9')
                c[i] = (char)('۰' + (c[i] - '0'));
        return new string(c);
    }

    private void ActivateCurrent()
    {
        var kind = Sequence[_taskIndex];
        if (stationRegister != null) stationRegister.active = kind == ShiftStationKind.Register;
        if (stationShelf != null) stationShelf.active = kind == ShiftStationKind.Shelf;
        if (kind == ShiftStationKind.Customer) SpawnCustomer();
        else DespawnCustomer();
        RefreshHud();
    }

    private void SpawnCustomer()
    {
        if (_customer == null)
        {
            _customer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _customer.name = "Customer";
            _customer.transform.position =
                (customerSpot != null ? customerSpot.position : transform.position) + Vector3.up * 1f;
            _customerStation = _customer.AddComponent<ShiftStation>();
            _customerStation.kind = ShiftStationKind.Customer;
            _customerStation.promptText = "مشتری: جواب دادن";
        }
        _customerStation.active = true;
    }

    private void DespawnCustomer()
    {
        if (_customerStation != null) _customerStation.active = false;
    }

    public void OnStationCompleted(ShiftStation st)
    {
        if (!IsRunning || st.kind != Sequence[_taskIndex]) return;
        _taskIndex++;
        if (_taskIndex >= Sequence.Length)
        {
            End(true);
            return;
        }
        ActivateCurrent();
    }

    private void End(bool completed)
    {
        if (!IsRunning) return;
        IsRunning = false;

        if (stationRegister != null) stationRegister.active = false;
        if (stationShelf != null) stationShelf.active = false;
        DespawnCustomer();
        if (door != null) door.locked = false;
        ServiceBridge.PopTimeMode();
        if (_hud != null) _hud.Hide();

        // پول فقط از EconomyService + رویداد فقط از EventBus
        ServiceBridge.SetFlag(GameFlags.FirstShiftCompleted, true);
        Debug.Log($"[Flags] firstShiftCompleted => {ServiceBridge.GetFlag(GameFlags.FirstShiftCompleted)}");

        bool paid = ServiceBridge.AddMoney(salary);
        Debug.Log($"[Shift] AddMoney({salary}) => {paid}");
        ServiceLocator.EventBus.Publish(new SalaryReceivedEvent { Amount = salary });
        Debug.Log($"[Shift] پایان شیفت (completed={completed}). حقوق {salary} واریز شد.");

        DialogueUI.Instance.ShowLine(
            "کارفرما",
            completed
                ? "خوب بود. اینم حقوق امروزت: ششصد هزار تومان. فردا هم بیا."
                : "شیفت تمیز شد. اینم حقوقت: ششصد هزار تومان. خسته نباشی.",
            null);
    }

    private void OnDestroy()
    {
        // اگر صحنه وسط شیفت به هر دلیلی unload شد، حالت زمان قفل نماند
        if (IsRunning)
        {
            IsRunning = false;
            ServiceBridge.PopTimeMode();
        }
    }
}