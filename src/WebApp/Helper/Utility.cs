namespace WebApp.Helper;

public static class Utility
{
    public static string Truncate(string s, int maxLength)
    {
        if (!string.IsNullOrEmpty(s) && s.Length > maxLength)
        {
            return s.Substring(0, maxLength);
        }
        return s;
    }
}