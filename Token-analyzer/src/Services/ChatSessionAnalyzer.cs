namespace TokenAnalyzer.Services;

public sealed class ChatSessionAnalyzer
{
    private readonly ChatSessionFileDiscovery fileDiscovery;
    private readonly ChatCreditParser creditParser;

    public ChatSessionAnalyzer()
        : this(new ChatSessionFileDiscovery(), new ChatCreditParser())
    {
    }

    internal ChatSessionAnalyzer(ChatSessionFileDiscovery fileDiscovery, ChatCreditParser creditParser)
    {
        this.fileDiscovery = fileDiscovery;
        this.creditParser = creditParser;
    }

    public ScanResult Scan(string rootPath, DateTime startDate, DateTime endDate)
        => Scan(new[] { rootPath }, startDate, endDate);

    public ScanResult Scan(IEnumerable<string> rootPaths, DateTime startDate, DateTime endDate)
    {
        SortedDictionary<DateTime, decimal> dailyCredits = new SortedDictionary<DateTime, decimal>();
        int directoriesFound = 0;
        int directoriesProcessed = 0;
        int filesAnalyzed = 0;
        int creditEntriesFound = 0;

        foreach (string rootPath in rootPaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            foreach (string chatSessionsDir in fileDiscovery.FindSessionDirectories(rootPath))
            {
                directoriesFound++;

                IReadOnlyList<ChatSessionFile> candidateFiles = fileDiscovery
                    .FindFilesInRange(chatSessionsDir, startDate, endDate);

                bool directoryInRange = IsInRange(
                    fileDiscovery.GetLastWriteTime(chatSessionsDir),
                    startDate,
                    endDate);
                if (!directoryInRange && candidateFiles.Count == 0)
                {
                    continue;
                }

                directoriesProcessed++;

                foreach (ChatSessionFile file in candidateFiles)
                {
                    filesAnalyzed++;

                    foreach (CreditEntry entry in creditParser.ParseFile(file.Path, file.LastWriteTime))
                    {
                        if (!IsInRange(entry.OccurredAt, startDate, endDate))
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

    private static bool IsInRange(DateTime value, DateTime start, DateTime end)
        => value >= start && value <= end;
}
