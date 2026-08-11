# Tehran City
Technical Design & Day-One Checklist
version: 1.0
date: 2026-08-08
status: Approved
owner: Project Lead
engine: Unity 6.3 LTS
render pipeline: URP

## ۱. هدف سند
این سند معماری فنی اولیه و چک‌لیست شروع کدنویسی را مشخص می‌کند.
- شروع سریع
- معماری تمیز
- آماده‌سازی برای آنلاین در آینده
- جلوگیری از وابستگی‌های خطرناک
- ساخت دمو بدون پیچیدگی غیر ضروری

## ۲. اصول فنی
- **اصل Offline-first:** بازی در نمونه اولیه بدون سرور اجرا می‌شود.
- **اصل Online-ready:** کد باید طوری باشد که بعداً سرویس‌های محلی به سرویس‌های سرور تبدیل شوند.
- **اصل Data-Driven:** آیتم‌ها، شغل‌ها، دیالوگ‌ها، قیمت‌ها و بالانس از ScriptableObject یا JSON/CSV خوانده شوند.
- **اصل Simulation جدا از Presentation:** منطق اقتصادی نباید مستقیم به Transform، MonoBehaviour یا UI وابسته باشد.
- **اصل تغییر پول فقط از EconomyService:** هیچ کدی مستقیم پول را کم و زیاد نمی‌کند.

## ۳. پکیج‌های مورد نیاز برای شروع
در Package Manager نصب کن:
- Input System
- Cinemachine
- TextMesh Pro
- Newtonsoft.Json
- Timeline
- ProBuilder

فعلاً اینها را نصب نکن مگر لازم شد:
- Addressables
- Netcode / Multiplayer packages

## ۴. ساختار پوشه پروژه
```
Assets/
  _Project/
    Art/
      Characters/
      Environment/
        Narmak/
      Props/
      UI/
    Audio/
      Music/
      SFX/
    Data/
      ScriptableObjects/
      Balance/
    Prefabs/
      Characters/
      NPCs/
      Interactables/
      UI/
    Scenes/
      Bootstrap.unity
      Main Menu.unity
      Prototype_Block01.unity
      Interior_Room01.unity
      Interior_MobileShop01.unity
    Scripts/
      Core/
      Gameplay/
      UI/
      Data/
      Settings/
```

## ۵. Assembly Definitions پیشنهادی
- `TC.Core`
- `TC.Gameplay`
- `TC.UI`
- `TC.Data`

**قوانین مهم:**
- UI نباید مستقیم اقتصاد را تغییر دهد.
- Gameplay نباید مستقیم Save بنویسد.
- Core نباید به صحنه وابسته باشد.

## ۶. سرویس‌های اولیه
- `IEventBus`: OnGameStarted, OnHourChanged, OnDayChanged, OnBalanceChanged, OnNeedsChanged, OnJobAccepted, OnShiftCompleted, OnSalaryReceived
- `ITimeService`: نگهداری Day/Hour/Minute، انتشار OnHourChanged، پشتیبانی از Pause
- `IEconomyService`: GetBalance, TrySpend, AddMoney, ثبت تراکنش (long balance)
- `INeedsService`: Energy, Stress, Hunger, Social, اعمال Decay
- `ISaveService`: JSON Save/Load, SaveVersion

## ۷. الگوی ساده معماری
```
UI Layer -> Gameplay Layer -> Service Layer -> Data Layer
```

## ۸. صحنه‌های اولیه
- `Bootstrap.unity`: ساخت GameManager و ServiceLocator، لود Main Menu / Prototype
- `Prototype_Block01.unity`: Greybox کوچه، پاساژ، Player، Camera
- `Interior_Room01.unity`: تخت، گوشی، آینه، میز
- `Interior_MobileShop01.unity`: قفسه، صندوق، کارفرما

## ۹. دوربین
- Cinemachine Virtual Camera (زاویه ۴۵ درجه، ارتفاع قابل زوم، Follow روی بازیکن، چرخش محدود)

## ۱۰. Input Actions
| اکشن | ورودی |
|---|---|
| Move | WASD |
| Interact | E |
| Phone | Tab |
| Pause | Esc |
| Zoom | Mouse Scroll |
| Rotate Camera | Right Mouse Drag |
| Clip Mode | F9 |

## ۱۱. Save Data Structure
```json
{
  "saveVersion": 1,
  "time": {
    "day": 1,
    "hour": 7,
    "minute": 30
  },
  "player": {
    "cash": 350000,
    "energy": 70,
    "stress": 40,
    "hunger": 55,
    "social": 45
  },
  "flags": {
    "landlordMessageSeen": false,
    "jobAccepted": false,
    "firstShiftCompleted": false
  }
}
```

## ۱۲. چک‌لیست روز اول
1. آماده‌سازی پروژه Unity 6.3 LTS + URP + Packages
2. ساخت ساختار پوشه‌ها و Git Repo + LFS
3. ساخت صحنه‌های Bootstrap و Prototype
4. پیاده‌سازی Player Movement و Cinemachine Camera
5. پیاده‌سازی سیستم Interaction ساده با `IInteractable`
6. ساخت HUD اولیه (نمایش پول، زمان، نیازها)
7. ساخت GameManager اولیه

## ۱۳. Definition of Done برای روز اول
- پروژه در Unity 6.3 بدون خطا باز شود.
- بازیکن در Greybox حرکت کند و دوربین ایزومتریک او را دنبال کند.
- تعامل با دکمه E روی اشیاء کار کند.
- HUD پول، روز و ساعت را نشان دهد.
- همه چیز در Git commit شده باشد.

## ۱۴. قوانین کدنویسی
1. هیچ عدد اقتصادی داخل UI hardcode نشود.
2. پول فقط از EconomyService تغییر کند.
3. هر سیستم مهم یک Interface داشته باشد.
4. Save Dataها SaveVersion داشته باشند.
5. از static زیاد پرهیز شود.
