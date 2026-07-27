namespace TokenAnalyzer.Services;

public sealed record ScanResult(
    SortedDictionary<DateTime, decimal> DailyCredits,
    decimal TotalCredits,
    int ChatSessionDirectoriesFound,
    int ChatSessionDirectoriesProcessed,
    int FilesAnalyzed,
    int CreditEntriesFound);