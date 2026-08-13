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
- هنوز پیاده‌سازی نشده: `ISaveService`، `JobService`.
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