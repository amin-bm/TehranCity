using UnityEngine;

/// <summary>ابزار اعداد فارسی: ارقام، جداکننده ٬، و نوشتار حروفی (۰ تا ۱۰۰) طبق قانون سه‌سطحی متن.</summary>
public static class PersianFormat
{
    public static string FaDigits(string s)
    {
        var c = s.ToCharArray();
        for (int i = 0; i < c.Length; i++)
            if (c[i] >= '0' && c[i] <= '9')
                c[i] = (char)('۰' + (c[i] - '0'));
        return new string(c);
    }

    public static string Money(long toman)
    {
        string raw = toman.ToString();
        var sb = new System.Text.StringBuilder();
        int count = 0;
        for (int i = raw.Length - 1; i >= 0; i--)
        {
            sb.Insert(0, raw[i]);
            count++;
            if (count % 3 == 0 && i > 0) sb.Insert(0, '٬');
        }
        return FaDigits(sb.ToString()) + " تومان";
    }

    private static readonly string[] Ones =
    {
        "صفر","یک","دو","سه","چهار","پنج","شش","هفت","هشت","نه","ده",
        "یازده","دوازده","سیزده","چهارده","پانزده","شانزده","هفده","هجده","نوزده"
    };
    private static readonly string[] Tens = { "", "", "بیست", "سی", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود" };

    public static string IntWords(int n)
    {
        n = Mathf.Clamp(n, 0, 100);
        if (n == 100) return "صد";
        if (n < 20) return Ones[n];
        int t = n / 10, o = n % 10;
        return o == 0 ? Tens[t] : $"{Tens[t]} و {Ones[o]}";
    }
}