namespace TokenAnalyzer;

public static class Printer
{
    private const int DateWidth = 12;
    private const int CreditsWidth = 16;
    private const int CostWidth = 12;
    
    public static void PrintSummary(ScanResult result, string rootPath, DateTime startDate, DateTime endDate)
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

    public static void PrintDailyReport(ScanResult result)
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
        Console.WriteLine($"{"TOTAL",-DateWidth}{result.TotalCredits,16:F2}{result.TotalCredits / 100,12:C2}");
    }

    private static void PrintTableHeader()
    {
        Console.WriteLine($"{"Data",-DateWidth}{"Credits",CreditsWidth}{"Cost",CostWidth}");
        Console.WriteLine(new string('-', DateWidth + CreditsWidth + CostWidth));
    }
}

