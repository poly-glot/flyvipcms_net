using System.Text.RegularExpressions;

namespace FlyVip.Web.Infrastructure;

public static partial class PasswordStrength
{
    private const int MinimumLength = 8;
    private const double MaxSimilarity = 50;

    private static readonly HashSet<string> TrivialWords = ["a", "an", "and", "as", "at", "but", "for", "if", "in", "not", "of", "or", "so", "the", "then"];

    public static string? Weakness(string password, string username, string email)
    {
        var trimmed = password.Trim();

        if (trimmed == "")
        {
            return "A Password is required.";
        }

        if (trimmed.EnumerateRunes().Count() < MinimumLength)
        {
            return $"Passwords must be at least {MinimumLength} characters long.";
        }

        var lowered = trimmed.ToLowerInvariant();

        if (IsPersonal(lowered, username.ToLowerInvariant(), email.ToLowerInvariant()))
        {
            return "Passwords cannot contain re-hashed personal information.";
        }

        return Similarity(lowered, username.ToLowerInvariant()) >= MaxSimilarity ? "Password is too similar to the username." : null;
    }

    private static bool IsPersonal(string password, string username, string email)
    {
        if (password == username || password == email || password == new string(username.Reverse().ToArray()))
        {
            return true;
        }

        var at = email.IndexOf('@', StringComparison.Ordinal);
        var localPart = at < 0 ? email : email[..at];
        var domain = at < 0 ? "" : email[(at + 1)..];

        var needles = Words(username).Concat(Words(localPart)).Append(domain).Where(IsMeaningful).ToList();

        return Words(password)
            .Where(IsMeaningful)
            .Any(haystack => needles.Any(needle => haystack.Contains(needle, StringComparison.Ordinal) || needle.Contains(haystack, StringComparison.Ordinal)));
    }

    private static bool IsMeaningful(string word) =>
        word != "" && !TrivialWords.Contains(word) && word.EnumerateRunes().Count() >= 3;

    private static IEnumerable<string> Words(string text)
    {
        var parts = NonWordPattern().Replace(text, " ").Trim().Split(' ');

        return parts.Contains(text) ? parts : parts.Prepend(text);
    }

    private static double Similarity(string first, string second)
    {
        var total = first.Length + second.Length;

        return total == 0 ? 0 : CommonCharacters(first, second) * 2.0 * 100 / total;
    }

    private static int CommonCharacters(string first, string second)
    {
        var (max, firstAt, secondAt) = (0, 0, 0);

        for (var i = 0; i < first.Length; i++)
        {
            for (var j = 0; j < second.Length; j++)
            {
                var length = 0;

                while (i + length < first.Length && j + length < second.Length && first[i + length] == second[j + length])
                {
                    length++;
                }

                if (length > max)
                {
                    (max, firstAt, secondAt) = (length, i, j);
                }
            }
        }

        if (max == 0)
        {
            return 0;
        }

        var left = firstAt > 0 && secondAt > 0 ? CommonCharacters(first[..firstAt], second[..secondAt]) : 0;
        var right = firstAt + max < first.Length && secondAt + max < second.Length
            ? CommonCharacters(first[(firstAt + max)..], second[(secondAt + max)..])
            : 0;

        return max + left + right;
    }

    [GeneratedRegex(@"[\W_]+")]
    private static partial Regex NonWordPattern();
}
