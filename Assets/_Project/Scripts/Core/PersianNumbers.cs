using System.Text;

public static class PersianNumbers
{
    private const string FaDigitsStr = "۰۱۲۳۴۵۶۷۸۹";

    public static string Digits(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (var c in s)
        {
            if (c >= '0' && c <= '9') sb.Append(FaDigitsStr[c - '0']);
            else if (c == ',') sb.Append('٬');
            else sb.Append(c);
        }
        return sb.ToString();
    }

    public static string FormatLong(long v) => Digits(v.ToString("#,0"));
}