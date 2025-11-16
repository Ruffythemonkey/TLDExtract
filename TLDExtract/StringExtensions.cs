using System.Text.RegularExpressions;

namespace TLDExtract;

internal static class StringExtensions
{
    /// <summary>
    /// Gibt alle Teilstrings zwischen zwei Markern zurück.
    /// Beachtet Zeilenumbrüche und ist standardmäßig case-insensitive.
    /// </summary>
    public static List<string> Between(this string text, string from, string to)
    {
        if (string.IsNullOrEmpty(text) || from == null || to == null)
            return new List<string>();

        string pattern = $"(?si)(?<=({Regex.Escape(from)}))(.*?)(?=({Regex.Escape(to)}))";
        return Regex.Matches(text, pattern)
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .ToList();
    }

    /// <summary>
    /// Ersetzt alle Inhalte zwischen zwei Markern (inklusive der Marker selbst)
    /// durch den angegebenen Ersatztext.
    /// </summary>
    public static string BetweenReplace(this string text, string from, string to, string replaceWith = "")
    {
        if (string.IsNullOrEmpty(text) || from == null || to == null)
            return text;

        string pattern = $"(?si){Regex.Escape(from)}.*?{Regex.Escape(to)}";
        return Regex.Replace(text, pattern, replaceWith);
    }

    /// <summary>
    /// Entfernt Steuerzeichen und bereinigt mehrfaches Leerzeichen.
    /// </summary>
    public static string RemoveControlCharacters(this string input)
    {
        if (input == null) return string.Empty;

        string cleaned = Regex.Replace(input, @"[\p{C}&&[^\r\n\t]]", " ");
        cleaned = Regex.Replace(cleaned, @"\s+", " ");
        return cleaned.Trim();
    }


}
