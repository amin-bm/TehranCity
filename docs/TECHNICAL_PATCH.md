# Tehran City — اصلاحیه‌های فنی سند (Technical Patch Notes)
version: 1.1-patch
status: Approved (مکمل اسناد v1.0 — هر جا با متن اصلی تضاد داشت، این سند حاکم است)

## ۱. پکیج‌ها (اصلاح بخش ۳ Technical Design)
- افزودنی: **RTLTMPro v4.0.0** (سازگار با Unity 6+). نسخه‌های v3 و قدیمی‌تر با Unity 6.3 ناسازگارند.
  نصب: `v4.0.0.unitypackage` از صفحه Releases ریپازیتوری pnarimani/RTLTMPro یا OpenUPM با `com.nosuchstudio.rtltmpro`.
- دلیل: TMP به‌تنهایی Shaping و RTL فارسی/عربی انجام نمی‌دهد.

## ۲. خط لوله متن فارسی (قانون سه‌سطحی — جایگزین هر بخش فونت/RTL قبلی)
۱) متن فارسی خالص (پرامپت تعامل، توست، دیالوگ، گوشی): `RTLTextMeshPro` / `RTLTextMeshPro3D` با متن خام.
۲) خط‌های عدددار/مخلوط (HUD پول/زمان/نیازها): `TextMeshProUGUI` معمولی + `FaText.Fix` + `PersianNumbers` (ارقام فارسی، جداکننده ٬).
۳) تابلوهای 3D: `RTLTextMeshPro3D` با متن خام + چرخش صفر + والد بدون Scale + `fontSize = 4.2` (ثابت پروژه).
نکات الزام‌آور:
- RTLTMPro جهت پاراگراف را از اولین کاراکتر قوی می‌گیرد؛ پرامپت‌ها همیشه به صورت «متن فارسی + "  [E]"» ساخته شوند.
- اعداد فارسی/انگلیسی داخل رشته‌های RTLTMPro به‌هم می‌ریزند؛ در توست‌ها اعداد با حروف نوشته شوند یا از مسیر FaText استفاده شود.
- فونت فعلی: `TMP_Tahoma_Persian` (SDF داینامیک از Tahoma ویندوز) — placeholder. فونت نهایی طبق Open Questions بعداً (پیشنهاد: Vazirmatn).

## ۳. دوربین (اصلاحیه ADR-001)
- Cinemachine 3.x: به‌جای VirtualCamera/Transposer از `CinemachineCamera + CinemachineFollow` استفاده شد (Transposer منسوخ است).
- الگوی **CameraPivot**: آبجکت خالی که فقط موقعیت بازیکن را کپی می‌کند (بدون چرخش) تا دوربین ایزومتریک با چرخش کاراکتر دور او نگردد.
- دوربین بین صحنه‌ها ماندگار است (CameraRig + PersistentObject).

## ۴. تعامل (اصلاح بخش Interaction)
- تشخیص نزدیک‌ترین `IInteractable` با `Physics.OverlapSphere` (پولینگ ۰.۱s) به‌جای Trigger؛ چون Trigger بین دو Collider بدون Rigidbody رویداد نمی‌سازد.

## ۵. ماندگاری و انتقال صحنه (الگوی جدید)
- `PersistentObject`: DontDestroyOnLoad + حذف خودکار کپی تکراری هنگام reload صحنه (رفع باگ دو PhoneController و توقف زمان).
- ماندگارها: `CameraRig`، `UI_Canvas`، `UI_HUD`، `UI_Phone`. هر صحنه فقط Player و محتوای خودش را دارد.
- انتقال: `SceneTransition.Load(scene, spawnPoint)` + `PendingSpawn`؛ `SceneDoor` پیاده‌سازی `IInteractable` برای درها.
- `GameManager` صحنه بعدی را در `Start` لود می‌کند (نه Awake).

## ۶. سرویس‌ها (اصلاح بخش ۶)
- `FlagsService` اضافه شد (متناظر بخش flags در Save Data).
- `ServiceLocator.Ensure()` = ساخت تنبل سرویس‌ها (برای Play مستقیم از هر صحنه در Editor).
- `ISaveService` پیاده‌سازی شد: `SaveService` (Save/Load JSON) + `SaveDirector` (ذخیره خودکار/F5/خلاصه روز) — جزئیات در الحاقیه 1.2-patch، بخش ۱۶.
- هنوز پیاده‌سازی نشده: `JobService`.
- Assembly Definitions (TC.Core/…) هنوز ایجاد نشده‌اند؛ پس از تثبیت سرویس‌ها اضافه شوند.

## ۷. یادداشت‌های API در Unity 6.3
- `PlayerSettings.activeInputHandler` حذف شده؛ Input System جدید پیش‌فرض است.
- `Object.FindObjectsOfType` منسوخ → `FindObjectsByType`.

## ۸. ابزارهای Editor (بازتولیدپذیر و idempotent)
منوی `TehranCity/Setup/` شماره‌های 1 تا 11:
1 Folders، 2 Scenes، 3 Base Settings، 4 Greybox Room، 5 Player، 6 Isometric Camera،
7 Persian Font، 8 Interaction+Prompt UI، 9 HUD+Bootstrap، 10 Phone، 11 Alley+Persistence.
به‌علاوه `TehranCity/Debug/Validate Interaction Setup` و `TehranCity/Migrate/Convert TMP to RTL`.

## ۹. Changelog
[1.1.0] — RTLTMPro v4؛ قانون سه‌سطحی متن؛ CameraPivot؛ PersistentObject با dedup؛ SceneTransition/SceneDoor؛ FlagsService؛ HUD با FaText؛ گوشی (Tab)؛ کوچه نارمک با تابلوهای فارسی (رفاه‌مارکت / پاساژ موبایل‌سنتر / مغازه خیلی کوچک-اجاره).

## الحاقیه 1.2-patch (پایان فاز دمو — قدم‌های ۱۰ تا ۱۷)
status: Approved — حاکم بر هر تضاد با متن قبلی

### ۱. سرویس‌ها (جایگزین بخش ۶ پچ ۱.۱)
- `ISaveService` پیاده‌سازی شد: `SaveService` (Newtonsoft.Json، saveVersion=1، DTO دقیقاً طبق §11 سند Technical).
  مسیر: `Application.persistentDataPath/tehran_city_save.json`. Load در `GameManager.Start` قبل از لود صحنه.
  ذخیره خودکار: تغییر روز، پایان شیفت، F5، OnApplicationQuit. `ServiceLocator.Save` اضافه شد.
- `ITimeService` توسعه یافت: `void AdvanceMinutes(int)` و `void SetDateTime(int day,int hour,int minute)`.
  enum نهایی: `TimeMode { FreeRoam, Indoor, Dialogue, Phone, Scripted }`.
  انحراف از سند: «ShiftMinigame» همان `Scripted` است؛ «Sleep» حالت زمان نیست — skip فوری با `SetDateTime(day+1, 7, 30)`.
- امضای واقعی `IEconomyService`: `AddMoney(long, string reason)` و `TrySpend(long, string reason)`؛
  `GetBalance` روی اینترفیس نیست (خواندن از طریق `ServiceBridge.GetBalance` با رفلکسیون).
- `FlagsService`: setter تایپ‌دار `Set(string,bool)`؛ getter با اسکن سیگنیچر-محور در ServiceBridge (نام متد لوکال نامشخص).
- `ShiftService` جدید (Core): خواندن `StreamingAssets/shifts.csv` با فرمت
  `id,salary,duration_real_sec,sequence,scripted_minutes` (sequence با `;` و مقادیر register/customer/shelf).
  `ShiftController` مصرف‌کننده است؛ فیلدهای serialized فقط fallback.
- `JobService` همچنان پیاده‌سازی نشده؛ منطق شیفت در ShiftController+ShiftService.
- Assembly Definitions هنوز ایجاد نشده‌اند.

### ۲. متن (الحاق به بخش ۲)
- پنل `DialogueUI` از سطح ۱ به **سطح ۲** منتقل شد (TextMeshProUGUI + FaText.Fix)؛
  دلیل: RTLTMPro ترتیب گروه‌های رقم ساعت (۰۷:۳۰) را برعکس نمایش می‌دهد.
  پرامپت‌ها/پیام‌های گوشی سطح ۱ و تابلوها سطح ۳ باقی ماندند.
- هشدار فنی: هرگز Fix دوگانه اجرا نشود (FaText/RTLTMPro روی خروجیِ قبلاً شکل‌گرفته) →
  گلیف‌هایPresentation Forms می‌شکنند (هشدار \uFC00-\uFC02). فایل `RtlOptions.cs` ساخته و سپس منسوخ شد (حذف شود).
- گزینه‌ی PreserveNumbers در این بیلد RTLTMPro ترتیب ساعت را اصلاح نمی‌کند؛ به آن تکیه نشود.

### ۳. ابزارهای Editor (جایگزین بخش ۸)
منوها اکنون ۱ تا ۱۷: 12 Mobile Shop+Interview، 13 Shift Minigame+Food Shop،
14 Save System+Tiny Shop Hook، 15 Bed+Sleep (Day Cycle)، 16 Cleanup Duplicate Phones، 17 Write shifts.csv.
همه idempotent.

### ۴. سیستم‌های گیم‌پلی جدید (الحاقی)
- خواب/چرخه روز: تخت در اتاق → دیالوگ تأیید → SetDateTime + ریست نیازها (Data-Driven روی Bed):
  energyOnWake=90، stressDelta=-30، hungerDelta=-20، socialDelta=-5.
  `DaySummaryUI` پس از ~۵ ثانیه خودکار بسته می‌شود و حین نمایش، ورودی گیم‌پلی قفل است.
- حلقه صاحب‌خانه: پیام runtime روزانه در گوشی (متن پلکانی)؛ پیام ندیده تا خواب بعدی → Stress +4
  (unseenLandlordStress). پیام‌های گوشی اسکرول‌پذیر + RectMask2D.
- غذا: `FoodShopInteractable.hungerRestore=25`.
- شیفت پس از اولین تکمیل **تکرارپذیر** است (گیت FirstShiftCompleted فقط برای روایت/ثبت)؛
  کارفرما وقتی شیفت در جریان نیست شیفت بعدی را پیشنهاد می‌دهد (لازم برای لوپ پس‌انداز ۱۵م).
- Clip Mode = F9: پنهان‌کردن UI_Canvas+ShiftHUD+Summary با رفرنس کش‌شده
  (GameObject.Find آبجکت غیرفعال را نمی‌یابد). Pause = Esc: TimeMode.Phone + اورلی + قفل ورودی.

### ۵. اعداد جدید (طبق قانون طلایی — ثبت در CSV/سند)
غذای ارزان Hunger+25 | خواب: Energy=90، Stress-30، Hunger-20، Social-5 | پیام ندیده Stress+4 |
اعداد شیفت فقط در shifts.csv. همه در Changelog [1.2.0] ثبت شوند.

### ۶. یادداشت‌های API Unity 6.3 (الحاق به بخش ۷)
- `FindObjectsByType<T>(FindObjectsInactive)` به‌تنهایی کامپایل نمی‌شود؛ آرگومان SortMode الزامی است.
- `SceneManager.GetAllScenes()` منسوخ → `sceneCount`/`GetSceneAt`.
- EventSystem باید DDOL باشد وگرنه با unload صحنه نابود می‌شود (مرگ کلیک موس).

### ۷. گراف آبجکت‌های ماندگار (DDOL)
GameManager، SaveDirector، ClipModeDirector، PauseDirector، CameraRig، UI_Canvas، UI_HUD، UI_Phone،
DialogueUI_Root، EventSystem، DaySummaryUI. ShiftHUD runtime و به‌ازای هر شیفت.

### ۸. انحراف از ADR-001
Clip Mode فعلی فقط UI را پنهان می‌کند؛ فریم عمودی 9:16 هنوز پیاده‌سازی نشده (فاز پولیش بعدی).

### ۹. Changelog
[1.2.0] — قدم‌های ۱۰-۱۷: پاساژ+مصاحبه VN؛ شیفت+حقوق+خرید غذا؛ Save/Load+خلاصه روز+هوک ۱۵م؛
Clip/Pause؛ خواب/روز+حلقه صاحب‌خانه؛ ShiftService/CSV؛ پولیش کنسول و بیلد Release تأییدشده (0 خطا/0 اخطار، 60FPS).

## الحاقیه‌ی 1.2-patch — قدم‌های ۱۰ تا ۱۷ (دمو First Hour کامل، تگ v1.0-first-hour)

### ۱۰. امضای واقعی سرویس‌ها (حاکم بر بخش ۶ الحاقیه‌ی قبل و بخش سرویس‌های سند اصلی)
- `IEconomyService`: `AddMoney(long amount, string reason)` و `TrySpend(long amount, string reason)` — پارامتر `reason` **اجباری** است. `GetBalance` روی اینترفیس **وجود ندارد**؛ خواندن موجودی فقط از `ServiceBridge.GetBalance()` (کشف به ترتیب: متد GetBalance/GetCash → پراپرتی Balance/Cash → فیلد _balance/cash).
- `ITimeService`: متدهای `AdvanceMinutes(int)` و `SetDateTime(int day, int hour, int minute)` اضافه شدند.
- `TimeMode`: اعضا دقیقاً `{ FreeRoam, Indoor, Dialogue, Phone, Scripted }`. برای شیفت `PushTimeMode("Scripted")` استفاده می‌شود («ShiftMinigame» عضو enum نیست).
- `ISaveService`: پیاده‌سازی شد (`SaveService`). همچنان پیاده‌سازی‌نشده: `JobService`.

### ۱۱. ServiceBridge — مسیر رسمی Gameplay/UI → سرویس‌ها
- پرچم‌ها: دسترسی سیگنیچر-محور + آینه‌ی داخلی `_flagMirror` برای تضمین round-trip Set/Get و Save/Load. همه‌ی خواندن/نوشتن فلگ فقط از `ServiceBridge.GetFlag/SetFlag`.
- نیازها: `AddNeed(name, delta)` / `GetNeed(name)` با Clamp روی ۰..۱۰۰.
- پول: `AddMoney / TrySpend / GetBalance` فقط از ServiceBridge (کال تایپ‌دار به `ServiceLocator.Economy`).

### ۱۲. اصلاح قانون سه‌سطحی متن (حاکم بر بخش متن الحاقیه‌ی قبل)
۱) پیام‌های گوشی: `RTLTextMeshPro` با متن خام. **هشدار:** هرگز Fix دوم روی خروجی RTLTMPro اجرا نشود؛ toggling گزینه‌ی PreserveNumbers در runtime + Refix باعث double-Fix و گلیف‌های مفقود `\uFC00–\uFC02` شد.
۲) هر متن runtime شامل عدد/ساعت (پنل دیالوگ VN، HUD، نوار شیفت، خلاصه روز): `TextMeshProUGUI + FaText.Fix + ارقام فارسی`. پنل `DialogueUI` عملاً به این سطح منتقل شد؛ چون RTLTMPro ترتیب گروه‌های «۰۷:۳۰» را برعکس نمایش می‌دهد.
۳) تابلوهای 3D: بدون تغییر (`RTLTextMeshPro3D` + `fontSize = 4.2`).
- نکته‌ی RectTransform: با pivot/anchor بالا، `anchoredPosition.y` مثبت یعنی **بالای** لبه‌ی صفحه (بیرون دید)؛ برای پایین آوردن، مقدار منفی (نوار شیفت: `y = -80`).

### ۱۳. جدول ورودی (افزودنی‌ها)
| کلید | عملکرد |
|---|---|
| E | تعامل / ادامه دیالوگ |
| 1 / 2 | قبول / رد در انتخاب‌ها |
| Tab | گوشی (زمان متوقف) |
| Esc | Pause (اورلی + توقف زمان + قفل حرکت) |
| F5 | Save دستی |
| F6 | خلاصه‌ی روز |
| F9 | Clip Mode (پنهان/آشکار کل UI) |

### ۱۴. منوهای Setup (تکمیل بخش ۸)
12) Passage + Interview — 13) Shift Minigame + Food Shop — 14) Save System + Tiny Shop Hook — 15) Bed + Sleep — 16) Cleanup Duplicate Phones — 17) Write shifts.csv

### ۱۵. شیفت Data-Driven
- `StreamingAssets/shifts.csv` با قالب: `id,salary,duration_real_sec,sequence,scripted_minutes` (sequence با `;` از `register/customer/shelf`؛ خطوط `#` کامنت).
- `ShiftService` می‌خواند؛ `ShiftController` فقط مصرف‌کننده (پیش‌فرض‌ها fallback). نرخ ساعت اسکریپت‌شده = `scripted_minutes / duration` (پیش‌فرض ۳۶۰/۲۴۰ = ۱٫۵ GameMinute بر ثانیه واقعی).
- شیفت‌ها **تکرارپذیرند**؛ `firstShiftCompleted` فقط ثبت است و جلوی شروع نمی‌گیرد. حین شیفت، تعامل کارفرما غیرفعال (`CanInteract`).
- حین باز بودن گوشی، تایمر شیفت و ساعت اسکریپت‌شده متوقف‌اند (Phone/Menus = Pause).

### ۱۶. Save/Load
- مسیر: `Application.persistentDataPath/tehran_city_save.json`؛ ساختار دقیق بخش ۱۱ سند اصلی + بررسی `saveVersion`.
- Load در `GameManager.Start` **قبل** از لود اولین صحنه؛ پول با **دلتا** از AddMoney/TrySpend اعمال می‌شود.
- Save توسط `SaveDirector`: تغییر روز، لبه‌ی `firstShiftCompleted`، F5، و `OnApplicationQuit`.
- خلاصه‌ی روز (`DaySummaryUI`) با تغییر روز؛ بستن **خودکار ~۵ ثانیه** + قفل Interactor/حرکت (تداخل با E تخت حذف شد).

### ۱۷. چرخه‌ی خواب/روز (Sleep = Skip)
- خواب → `SetDateTime(day+1, 7, 30)`.
- اعداد جدید (ثبت در `03_ECONOMY_BALANCE.md`): Energy=۹۰ (set)، Stress −۳۰، Hunger −۲۰، Social −۵؛ پیام ندیده‌ی صاحب‌خانه تا خواب بعدی: Stress +۴؛ غذای ارزان ۱۲۰٬۰۰۰ = Hunger +۲۵.
- پیام روزانه‌ی صاحب‌خانه به‌صورت runtime message به گوشی (متن پلکانی بر اساس روز).

### ۱۸. یادداشت‌های API Unity 6.3 (افزودنی به بخش ۷)
- `SceneManager.GetAllScenes()` منسوخ → حلقه با `sceneCount` / `GetSceneAt(i)`.
- `FindObjectsByType<T>` همراه `FindObjectsInactive.Include` همچنان `FindObjectsSortMode` هم می‌خواهد (اورلود تک‌آرگومانی ندارد).
- `GameObject.Find` آبجکت غیرفعال را نمی‌یابد (Clip Mode برای UI_Canvas از رفرنس کش‌شده استفاده می‌کند).
- `EventSystem` باید DDOL باشد و در هر باز شدن دیالوگ تضمین شود؛ وگرنه کلیک موس بعد از تعویض صحنه می‌میرد.

### ۱۹. Changelog
[1.2.0] — دمو First Hour کامل: مصاحبه VN + شیفت مینی‌گیم + حقوق/خرید + Save/Load + خلاصه روز + خواب/چرخه‌ی روز + حلقه‌ی صاحب‌خانه + Clip/Pause + شیفت CSV + بیلد Release تمیز (۰ خطا/۰ اخطار، ۶۰FPS).