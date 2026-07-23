using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TokenAnalyzer;

public sealed class ChatSessionAnalyzer
{
    private static readonly Regex DetailsCreditsRegex = new(
        "\"details\"\\s*:\\s*\"[^\"]*?(?<credits>[0-9]+(?:\\.[0-9]+)?)\\s+credits\"",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex TimestampRegex = new(
        "\"timestamp\"\\s*:\\s*(?<ts>[0-9]{10,13})",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public ScanResult Scan(string rootPath, DateTime startDate, DateTime endDate)
    {
        DateTime start = startDate.Date;
        DateTime end = endDate.Date.AddDays(1).AddTicks(-1);

        SortedDictionary<DateTime, decimal> dailyCredits = new SortedDictionary<DateTime, decimal>();
        int directoriesFound = 0;
        int directoriesProcessed = 0;
        int filesAnalyzed = 0;
        int creditEntriesFound = 0;

        foreach (string chatSessionsDir in FindDirectoriesByName(rootPath, "chatSessions"))
        {
            directoriesFound++;

            List<string> candidateFiles = EnumerateFilesSafe(chatSessionsDir)
                .Where(file => IsInRange(File.GetLastWriteTime(file), start, end))
                .ToList();

            bool directoryInRange = IsInRange(Directory.GetLastWriteTime(chatSessionsDir), start, end);
            if (!directoryInRange && candidateFiles.Count == 0)
            {
                continue;
            }

            directoriesProcessed++;

            foreach (string file in candidateFiles)
            {
                filesAnalyzed++;
                DateTime fallbackDate = File.GetLastWriteTime(file);

                foreach (CreditEntry entry in ParseCreditsFromFile(file, fallbackDate))
                {
                    if (!IsInRange(entry.OccurredAt, start, end))
                    {
                        continue;
                    }

                    creditEntriesFound++;
                    DateTime date = entry.OccurredAt.Date;

                    if (!dailyCredits.TryAdd(date, entry.Credits))
                    {
                        dailyCredits[date] += entry.Credits;
                    }
                }
            }
        }

        decimal total = dailyCredits.Values.Sum();

        return new ScanResult(
            dailyCredits,
            total,
            directoriesFound,
            directoriesProcessed,
            filesAnalyzed,
            creditEntriesFound);
    }

    private static IEnumerable<CreditEntry> ParseCreditsFromFile(string filePath, DateTime fallbackTimestamp)
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

    private static bool IsInRange(DateTime value, DateTime start, DateTime end)
        => value >= start && value <= end;

    private static IEnumerable<string> FindDirectoriesByName(string rootPath, string targetDirectoryName)
    {
        Stack<string> stack = new Stack<string>();
        stack.Push(rootPath);

        while (stack.Count > 0)
        {
            string current = stack.Pop();

            IEnumerable<string> subDirectories;
            try
            {
                subDirectories = Directory.EnumerateDirectories(current);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }

            foreach (string subDir in subDirectories)
            {
                if (string.Equals(Path.GetFileName(subDir), targetDirectoryName, StringComparison.OrdinalIgnoreCase))
                {
                    yield return subDir;
                }

                stack.Push(subDir);
            }
        }
    }

    private static IEnumerable<string> EnumerateFilesSafe(string rootDir)
    {
        Stack<string> stack = new Stack<string>();
        stack.Push(rootDir);

        while (stack.Count > 0)
        {
            string current = stack.Pop();

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(current);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }

            foreach (string file in files)
            {
                yield return file;
            }

            IEnumerable<string> subDirectories;
            try
            {
                subDirectories = Directory.EnumerateDirectories(current);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }

            foreach (string subDir in subDirectories)
            {
                stack.Push(subDir);
            }
        }
    }

    private readonly record struct CreditEntry(DateTime OccurredAt, decimal Credits);
}

public sealed record ScanResult(
    SortedDictionary<DateTime, decimal> DailyCredits,
    decimal TotalCredits,
    int ChatSessionDirectoriesFound,
    int ChatSessionDirectoriesProcessed,
    int FilesAnalyzed,
    int CreditEntriesFound);
