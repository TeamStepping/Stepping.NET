namespace Stepping.TmProviders.Dtm.Grpc.Extensions;

public static class SteppingStringExtensions
{
    public static string RemovePreFix(this string str, params string[] preFixes)
    {
        return str.RemovePreFix(StringComparison.Ordinal, preFixes);
    }

    public static string RemovePreFix(this string str, StringComparison comparisonType, params string[] preFixes)
    {
        foreach (var preFix in preFixes)
        {
            if (str.StartsWith(preFix, comparisonType))
            {
                return str.Substring(preFix.Length, str.Length - preFix.Length);
            }
        }

        return str;
    }

    public static string RemovePostFix(this string str, params string[] postFixes)
    {
        return str.RemovePostFix(StringComparison.Ordinal, postFixes);
    }

    public static string RemovePostFix(this string str, StringComparison comparisonType, params string[] postFixes)
    {
        foreach (var postFix in postFixes)
        {
            if (str.EndsWith(postFix, comparisonType))
            {
                return str[..^postFix.Length];
            }
        }

        return str;
    }

    public static string EnsureStartsWith(this string str, char c,
        StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (str.StartsWith(c.ToString(), comparisonType))
        {
            return str;
        }

        return c + str;
    }

    public static string EnsureEndsWith(this string str, char c,
        StringComparison comparisonType = StringComparison.Ordinal)
    {

        if (str.EndsWith(c.ToString(), comparisonType))
        {
            return str;
        }

        return str + c;
    }
}