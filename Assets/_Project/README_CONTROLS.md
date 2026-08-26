# Tehran City — کنترل‌ها و خلاصه فاز A

## وضعیت فاز A (Art Pass + Audio + Lighting + Post Processing)
**تاریخ:** 2026-08-26
**موتور:** Unity 6.3 LTS + URP
**استایل:** Stylized Low-Poly (Precinct Style)

### قدم‌های انجام‌شده
| قدم | عنوان | Setup | وضعیت |
|---|---|---|---|
| A0 | URP فعال‌سازی | — | ✅ |
| A1 | Art Kit (Palette) | 18 | ✅ |
| A2 | Room Art Pass | 19 | ✅ |
| A3 | Alley Art Pass | 20 | ✅ |
| A4 | Shop + Facade Art Pass | 21 | ✅ |
| A5 | Character + Animator | 22 | ✅ |
| A6 | Audio (SFX + Ambience) | 23 | ✅ |
| A7 | Music | 24 | ✅ |
| A8 | Lighting (Bake + Probes) | 25-28 | ✅ |
| A9 | Post Processing | 29 | ✅ |
| A10 | README | 30 | ✅ |

---

## کنترل‌ها (Keyboard + Mouse)

| کلید | عملکرد | توضیح |
|---|---|---|
| **WASD** | حرکت | کنترل کاراکتر در صحنه |
| **Mouse Move** | چرخش دوربین | حرکت ماوس = چرخش ایزومتریک |
| **Mouse Scroll** | زوم | نزدیک/دور کردن دوربین |
| **E** | تعامل | باز کردن در، تخت، گوشی، دیالوگ |
| **Tab** | گوشی موبایل | باز/بسته کردن گوشی (زمان Pause) |
| **Esc** | Pause | توقف زمان + اورلی |
| **F5** | Save دستی | ذخیره وضعیت بازی |
| **F6** | خلاصه روز | نمایش آمار روز |
| **F9** | Clip Mode | پنهان/آشکار کردن UI (برای کلیپ‌گیری) |
| **1 / 2** | انتخاب دیالوگ | قبول / رد گزینه‌ها در VN |

---

## جریان بازی (First Hour Demo)

1. **Bootstrap** → لود صحنه اتاق
2. **اتاق** (Interior_Room01):
   - تخت → خواب (Skip تا فردا ۰۷:۳۰)
   - گوشی (Tab) → پیام صاحب‌خانه / دوست
   - در خروج (E) → کوچه
3. **کوچه** (Prototype_Block01):
   - راه رفتن به سمت تابلوها
   - سوپرمارکت (E) → دمو
   - پاساژ موبایل‌سنتر (E) → ورود به مغازه
   - مغازه خیلی کوچک (E) → هوک هدف ۱۵ میلیون
4. **پاساژ** (Interior_MobileShop01):
   - کارفرما (E) → مصاحبه VN (قبول/رد)
   - پس از قبول → شیفت مینی‌گیم (۴ دقیقه واقعی = ۶ ساعت بازی)
   - حقوق ۶۰۰,۰۰۰ تومان → خرید غذا (Hunger +25)
5. **بازگشت به اتاق** → خواب → روز بعد

---

## ساختار Setupها (Editor Menus)

### فاز دمو (قدم‌های ۱-۱۷)
- **1-11:** Folders, Scenes, Settings, Greybox, Player, Camera, Font, Interaction, HUD, Phone, Alley
- **12:** Mobile Shop + Interview (VN)
- **13:** Shift Minigame + Food Shop
- **14:** Save System + Tiny Shop Hook
- **15:** Bed + Sleep (Day Cycle)
- **16:** Cleanup Duplicate Phones
- **17:** Write shifts.csv

### فاز Art Pass (قدم‌های ۱۸-۳۰)
- **18:** Art Kit (Palette)
- **19:** Room Art Pass
- **20:** Alley Art Pass
- **21:** Shop + Facade Art Pass
- **22:** Character Mannequin + Animator
- **23:** Audio Placeholder (Procedural WAVs)
- **24:** Music Placeholder (Procedural Loop)
- **25:** Lighting + Probes
- **26:** Bake Each Scene (Single-Scene, Sync + Save)
- **27:** Cancel Bake
- **28:** Lighting Polish (Shadows ON + Sun Up)
- **29:** Post Processing (Bloom + Color + Vignette)
- **30:** Generate README

---

## فایل‌های مهم

### صحنه‌ها
- `Assets/_Project/Scenes/Bootstrap.unity` — نقطه شروع (همیشه از اینجا Play کن)
- `Assets/_Project/Scenes/Interior_Room01.unity` — اتاق بازیکن
- `Assets/_Project/Scenes/Prototype_Block01.unity` — کوچه نارمک
- `Assets/_Project/Scenes/Interior_MobileShop01.unity` — پاساژ موبایل‌سنتر

### تنظیمات
- `Assets/_Project/Settings/URP_TehranCity.asset` — Render Pipeline
- `Assets/_Project/Settings/PostProcessProfile.asset` — Bloom + Vignette + Color Grading
- `Assets/_Project/Resources/AudioBank.asset` — رفرنس کلیپ‌های صدا

### صدا
- `Assets/_Project/Audio/SFX/` — ۶ افکت صوتی (placeholder رویه‌ای)
- `Assets/_Project/Audio/Ambience/` — ۳ صدای محیطی (اتاق/کوچه/پاساژ)
- `Assets/_Project/Audio/Music/` — موسیقی پس‌زمینه

### کاراکتر
- `Assets/_Project/Art/Characters/AC_Arash.controller` — Animator Controller
- `Assets/_Project/Art/Characters/AC_Arash_Idle.anim` — کلیپ Idle
- `Assets/_Project/Art/Characters/AC_Arash_Walk.anim` — کلیپ Walk

---

## نکات فنی

### اجرا
1. همیشه از **Bootstrap.unity** Play کن (نه مستقیم از صحنه‌های دیگر).
2. اگر Lightmapها در Play دیده نمی‌شوند: `Setup 26` را دوباره بزن.
3. برای صدای بهتر: فایل‌های `.wav` در `Audio/SFX` و `Audio/Ambience` را با فایل واقعی جایگزین کن و دوباره `Setup 23` بزن.

### Build
- **Build Settings** → همه ۴ صحنه را اضافه کن (Bootstrap اول).
- **Player Settings** → PC / Windows.
- **Build** → خروجی `Builds/TehranCity.exe`.

### مشکلات رایج
- **صفحه صورتی:** URP asset را دوباره ست کن (`Edit > Project Settings > Graphics > Default Render Pipeline`).
- **سایه‌های خشن:** `Setup 28` را بزن (سایه‌های Soft + خورشید بلندتر).
- **Bloom کم:** در `PostProcessProfile` مقدار `Intensity` را به ۱٫۳ برسان.
- **صدا نمی‌آید:** `Setup 23` را دوباره بزن (AudioBank را پر می‌کند).

---

## قدم‌های بعدی (فاز B)

پس از تأیید فاز A، می‌توانیم روی این موارد کار کنیم:
1. **آینه واقعی** در اتاق (Reflection Probe را کامل کنیم).
2. **NPCهای عابر** در کوچه (حس زنده بودن شهر).
3. **انیمیشن‌های بیشتر** برای کاراکتر (Run, Interact, Sit).
4. **مغازه قابل خرید** (سیستم Asset + دکوراسیون).
5. **حمل و نقل** (مترو/تاکسی برای اتصال محله‌ها).
6. **Online-ready** (Leaderboard محلی + معماری سرور).

---

**ساخته‌شده با عشق برای تهران. 🇮🇷**
