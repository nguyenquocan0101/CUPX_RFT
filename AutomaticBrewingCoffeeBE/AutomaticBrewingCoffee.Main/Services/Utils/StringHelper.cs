using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Services.Utils;

public class StringHelper
{
    public static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return new string(chars.ToArray()).Normalize(NormalizationForm.FormC);
    }

    public static string RemoveSpacesAndDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var chars = normalized.Where(c =>
            CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark
            && !char.IsWhiteSpace(c)
        );
        return new string(chars.ToArray()).Normalize(NormalizationForm.FormC);
    }
    
    public static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "NA";
        var s = input.Trim().ToUpperInvariant();
        s = Regex.Replace(s, @"[^A-Z0-9\-:_]", "");
        return s.Length > 20 ? s[..20] : s;
    }
}