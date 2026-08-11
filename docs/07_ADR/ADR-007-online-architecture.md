# ADR-007: Online-Ready Architecture
status: Approved
date: 2026-08-08
deciders: Project Lead

## مسئله
بازی در آینده باید آنلاین شود اما نسخه اولیه نباید به سرور وابسته باشد.

## تصمیم
معماری **Offline-first + Online-ready**

## قوانین
1. گیم‌پلی اصلی آفلاین اجرا می‌شود.
2. سرور در نمونه اولیه وجود ندارد.
3. تمام تغییرات پول از `EconomyService` عبور می‌کند.
4. تمام تغییرات دارایی از `AssetService` عبور می‌کند.
5. Saveها `SaveVersion` دارند.
6. Simulation از Presentation جدا است.
7. هر آیتم/مغازه بازیکن Id منحصر به فرد دارد.

## سرویس‌های اولیه
- `ITimeService`
- `IEconomyService`
- `INeedsService`
- `ISaveService`
- `IEventBus`
- `JobService`

## پیامدها
### مثبت
- دمو بدون اینترنت کار می‌کند.
- بعداً می‌توان آن را آنلاین کرد.
- معماری تمیز می‌ماند.

### منفی
- باید از روز اول نظم معماری رعایت شود و تغییر مستقیم فیلدهای پول ممنوع باشد.

## معیار قبولی
- هیچ MonoBehaviour پول را مستقیم تغییر ندهد.
- Save/Load بعد از تغییر اقتصاد سالم باشد.
