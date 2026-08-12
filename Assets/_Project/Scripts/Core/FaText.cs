using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Shaping + RTL برای خط‌های مخلوط (عدد/نماد) مثل HUD.
/// خروجی برای TMP معمولی (موتور LTR) با Alignment راست است.
/// </summary>
public static class FaText
{
    private static readonly Dictionary<char, char[]> Forms = new();
    private static readonly HashSet<char> NoLeftConnect = new()
        { 'ا', 'آ', 'أ', 'إ', 'د', 'ذ', 'ر', 'ز', 'و', 'ء', 'ة', 'ى' };

    static FaText()
    {
        char[] order = { 'ء','آ','أ','ؤ','إ','ئ','ا','ب','ة','ت','ث','ج','ح','خ',
                         'د','ذ','ر','ز','س','ش','ص','ض','ط','ظ','ع','غ','ف','ق',
                         'ك','ل','م','ن','ه','و','ى','ي' };
        ushort[] first = { 0xFE80,0xFE81,0xFE83,0xFE85,0xFE87,0xFE89,0xFE8D,0xFE8F,
                           0xFE93,0xFE95,0xFE99,0xFE9D,0xFEA1,0xFEA5,0xFEA9,0xFEAB,
                           0xFEAD,0xFEAF,0xFEB1,0xFEB5,0xFEB9,0xFEBD,0xFEC1,0xFEC5,
                           0xFEC9,0xFECD,0xFED1,0xFED5,0xFED9,0xFEDD,0xFEE1,0xFEE5,
                           0xFEE9,0xFEED,0xFEEF,0xFEF1 };
        int[] counts =   { 1,2,2,2,2,4,2,4,2,4,4,4,4,4,2,2,2,2,4,4,4,4,4,4,4,4,
                           4,4,4,4,4,4,4,2,2,4 };
        for (int i = 0; i < order.Length; i++)
        {
            var arr = new char[counts[i]];
            for (int k = 0; k < counts[i]; k++) arr[k] = (char)(first[i] + k);
            Forms[order[i]] = arr; // [isolated, final, initial, medial]
        }

        Forms['پ'] = new[] { '\uFB56', '\uFB57', '\uFB58', '\uFB59' };
        Forms['چ'] = new[] { '\uFB7A', '\uFB7B', '\uFB7C', '\uFB7D' };
        Forms['ژ'] = new[] { '\uFB8A', '\uFB8B' };
        Forms['گ'] = new[] { '\uFB92', '\uFB93', '\uFB94', '\uFB95' };
        Forms['ک'] = new[] { '\uFB8E', '\uFB8F', '\uFB90', '\uFB91' };
        Forms['ی'] = new[] { '\uFBFC', '\uFBFD', '\uFBFE', '\uFBFF' };

        if (Forms['و'].Length != 2 || Forms['ه'].Length != 4 || Forms['ی'].Length != 4)
            Debug.LogError("[FaText] Mapping table is broken!");
    }

    private static bool IsLetter(char c) => Forms.ContainsKey(c) || c == 'ـ';
    private static bool ConnectsLeft(char c) => IsLetter(c) && !NoLeftConnect.Contains(c);

    private static bool HasArabic(string t)
    {
        foreach (var c in t)
            if (IsLetter(c)) return true;
        return false;
    }

    private static string ShapeAndReverse(string t)
    {
        var shaped = new char[t.Length];
        for (int i = 0; i < t.Length; i++)
        {
            char c = t[i];
            if (Forms.TryGetValue(c, out var f))
            {
                bool prevC = i > 0 && ConnectsLeft(t[i - 1]);
                bool nextC = i < t.Length - 1 && IsLetter(t[i + 1]);
                int idx = f.Length == 4
                    ? (prevC && nextC ? 3 : prevC ? 1 : nextC ? 2 : 0)
                    : (prevC ? 1 : 0);
                shaped[i] = f[idx];
            }
            else shaped[i] = c;
        }
        System.Array.Reverse(shaped);
        return new string(shaped);
    }

    /// <summary>
    /// ترتیب توکن‌ها برعکس؛ توکن فارسی shape+reverse؛ جزیره‌های عدد/نماد (۳۵۰٬۰۰۰ / ۰۷:۳۰) دست‌نخورده.
    /// </summary>
    public static string Fix(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        s = s.Replace("‌", ""); // نیم‌فاصله

        var tokens = new List<string>();
        var sb = new StringBuilder();
        foreach (char c in s)
        {
            if (char.IsWhiteSpace(c))
            {
                if (sb.Length > 0) { tokens.Add(sb.ToString()); sb.Clear(); }
                tokens.Add(" ");
            }
            else sb.Append(c);
        }
        if (sb.Length > 0) tokens.Add(sb.ToString());

        var result = new StringBuilder();
        for (int i = tokens.Count - 1; i >= 0; i--)
        {
            string t = tokens[i];
            result.Append(t == " " ? " " : (HasArabic(t) ? ShapeAndReverse(t) : t));
        }
        return result.ToString();
    }
}