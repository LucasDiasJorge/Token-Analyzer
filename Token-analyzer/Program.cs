namespace TokenAnalyzer;

public static class Program
{
    private static int Main(string[] args)
    {
        (string rootPath, DateTime startDate, DateTime endDate) = Helper.ParseArguments(args);
        if (!Validator.ValidateInputs(rootPath, startDate, endDate))
            return 1;

        ScanResult result = RunAnalysis(rootPath, startDate, endDate);
        Printer.PrintSummary(result, rootPath, startDate, endDate);
        Printer.PrintDailyReport(result);

        return 0;
    }

    private static ScanResult RunAnalysis(string rootPath, DateTime startDate, DateTime endDate)
    {
        ChatSessionAnalyzer analyzer = new ChatSessionAnalyzer();
        return analyzer.Scan(rootPath, startDate, endDate);
    }
}