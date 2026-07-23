using System.Globalization;
using TokenAnalyzer;

internal static class Program
{
    private const int DateWidth = 12;
    private const int CreditsWidth = 16;
    private const int CostWidth = 12;

    private static int Main(string[] args)
    {
        (string rootPath, DateTime startDate, DateTime endDate) = ParseArguments(args);
        if (!ValidateInputs(rootPath, startDate, endDate))
        {
            return 1;
        }

        ScanResult result = RunAnalysis(rootPath, startDate, endDate);
        PrintSummary(result, rootPath, startDate, endDate);
        PrintDailyReport(result);

        return 0;
    }

    private static (string RootPath, DateTime StartDate, DateTime EndDate) ParseArguments(string[] args)
    {
        string rootPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        DateTime startDate = args.Length > 1
            ? ParseDate(args[1], "data inicial")
            : new DateTime(DateTime.Today.Year, 6, 1);
        DateTime endDate = args.Length > 2
            ? ParseDate(args[2], "data final")
            : DateTime.Today;

        return (rootPath, startDate, endDate);
    }

    private static bool ValidateInputs(string rootPath, DateTime startDate, DateTime endDate)
    {
        if (!Directory.Exists(rootPath))
        {
            Console.Error.WriteLine($"Diretorio nao encontrado: {rootPath}");
            return false;
        }

        if (startDate > endDate)
        {
            Console.Error.WriteLine("A data inicial nao pode ser maior que a data final.");
            return false;
        }

        return true;
    }

    private static ScanResult RunAnalysis(string rootPath, DateTime startDate, DateTime endDate)
    {
        ChatSessionAnalyzer analyzer = new ChatSessionAnalyzer();
        return analyzer.Scan(rootPath, startDate, endDate);
    }

    private static void PrintSummary(ScanResult result, string rootPath, DateTime startDate, DateTime endDate)
    {
        Console.WriteLine("=== Relatorio de Gasto Diario (credits) ===");
        Console.WriteLine($"Raiz analisada: {rootPath}");
        Console.WriteLine($"Periodo: {startDate:dd/MM/yyyy} ate {endDate:dd/MM/yyyy}");
        Console.WriteLine($"Pastas chatSessions encontradas: {result.ChatSessionDirectoriesFound}");
        Console.WriteLine($"Pastas consideradas no periodo: {result.ChatSessionDirectoriesProcessed}");
        Console.WriteLine($"Arquivos analisados: {result.FilesAnalyzed}");
        Console.WriteLine($"Entradas de credits encontradas: {result.CreditEntriesFound}");
        Console.WriteLine();
    }

    private static void PrintDailyReport(ScanResult result)
    {
        if (result.DailyCredits.Count == 0)
        {
            Console.WriteLine("Nenhum gasto encontrado no periodo informado.");
            return;
        }

        PrintTableHeader();

        foreach (KeyValuePair<DateTime, decimal> row in result.DailyCredits)
        {
            Console.WriteLine($"{row.Key:dd/MM/yyyy}".PadRight(DateWidth) + $"{row.Value,16:F2}" + $"{row.Value / 100,12:C2}");
        }

        Console.WriteLine(new string('-', DateWidth + CreditsWidth + CostWidth));
        Console.WriteLine($"{"TOTAL".PadRight(DateWidth)}{result.TotalCredits,16:F2}{result.TotalCredits / 100,12:C2}");
    }

    private static void PrintTableHeader()
    {
        Console.WriteLine($"{"Data".PadRight(DateWidth)}{"Credits".PadLeft(CreditsWidth)}{"Cost".PadLeft(CostWidth)}");
        Console.WriteLine(new string('-', DateWidth + CreditsWidth + CostWidth));
    }

    private static DateTime ParseDate(string value, string argName)
    {
        string[] formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy" };

        if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
        {
            return parsed.Date;
        }

        Console.Error.WriteLine($"Formato invalido para {argName}: {value}");
        Console.Error.WriteLine("Use: yyyy-MM-dd, dd/MM/yyyy ou dd-MM-yyyy");
        Environment.Exit(1);
        return DateTime.MinValue;
    }
}
