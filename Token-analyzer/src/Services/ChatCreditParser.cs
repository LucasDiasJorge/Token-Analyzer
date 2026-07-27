using System.Globalization;
using System.Text.RegularExpressions;

namespace TokenAnalyzer.Services;

internal sealed class ChatCreditParser
{
    private static readonly Regex DetailsCreditsRegex = new(
        "\"details\"\\s*:\\s*\"[^\"]*?(?<credits>[0-9]+(?:\\.[0-9]+)?)\\s+credits\"",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex TimestampRegex = new(
        "\"timestamp\"\\s*:\\s*(?<ts>[0-9]{10,13})",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public IEnumerable<CreditEntry> ParseFile(string filePath, DateTime fallbackTimestamp)
    {
        foreach (string line in File.ReadLines(filePath))
        {
            if (!line.Contains("\"details\"", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Match detailsMatch = DetailsCreditsRegex.Match(line);
            if (!detailsMatch.Success)
            {
                continue;
            }

            string rawCredits = detailsMatch.Groups["credits"].Value;
            if (!decimal.TryParse(rawCredits, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal credits))
            {
                continue;
            }

            DateTime occurredAt = fallbackTimestamp;
            Match timestampMatch = TimestampRegex.Match(line);
            if (timestampMatch.Success && long.TryParse(timestampMatch.Groups["ts"].Value, out long rawTimestamp))
            {
                occurredAt = ToDateTime(rawTimestamp);
            }

            yield return new CreditEntry(occurredAt, credits);
        }
    }

    private static DateTime ToDateTime(long unix)
    {
        try
        {
            return unix > 9_999_999_999
                ? DateTimeOffset.FromUnixTimeMilliseconds(unix).LocalDateTime
                : DateTimeOffset.FromUnixTimeSeconds(unix).LocalDateTime;
        }
        catch (ArgumentOutOfRangeException)
        {
            return DateTime.MinValue;
        }
    }
}

internal readonly record struct CreditEntry(DateTime OccurredAt, decimal Credits);