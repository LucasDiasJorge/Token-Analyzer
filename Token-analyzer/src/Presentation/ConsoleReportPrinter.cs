using System.Text;
using TokenAnalyzer.Services;

namespace TokenAnalyzer.Presentation;

public static class ConsoleReportPrinter
{
    private const int DateWidth = 12;
    private const int CreditsWidth = 16;
    private const int CostWidth = 12;

    public static string PrintSummary(ScanResult result, string rootPath, DateTime startDate, DateTime endDate)
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("=== Relatorio de Gasto Diario (credits) ===");
        report.AppendLine($"Raiz analisada: {rootPath}");
        report.AppendLine($"Periodo: {startDate:dd/MM/yyyy} ate {endDate:dd/MM/yyyy}");
        report.AppendLine($"Pastas chatSessions encontradas: {result.ChatSessionDirectoriesFound}");
        report.AppendLine($"Pastas consideradas no periodo: {result.ChatSessionDirectoriesProcessed}");
        report.AppendLine($"Arquivos analisados: {result.FilesAnalyzed}");
        report.Append($"Entradas de credits encontradas: {result.CreditEntriesFound}");

        string output = report.ToString();
        Console.WriteLine(output);
        Console.WriteLine();
        return output;
    }

    public static string PrintDailyReport(ScanResult result)
    {
        if (result.DailyCredits.Count == 0)
        {
            const string emptyReport = "Nenhum gasto encontrado no periodo informado.";
            Console.WriteLine(emptyReport);
            return emptyReport;
        }

        StringBuilder report = new StringBuilder();
        PrintTableHeader(report);

        foreach (KeyValuePair<DateTime, decimal> row in result.DailyCredits)
        {
            report.AppendLine($"{row.Key:dd/MM/yyyy}".PadRight(DateWidth) + $"{row.Value,16:F2}" + $"{row.Value / 100,12:C2}");
        }

        report.AppendLine(new string('-', DateWidth + CreditsWidth + CostWidth));
        report.Append($"{"TOTAL",-DateWidth}{result.TotalCredits,16:F2}{result.TotalCredits / 100,12:C2}");

        string output = report.ToString();
        Console.WriteLine(output);
        return output;
    }

    private static void PrintTableHeader(StringBuilder report)
    {
        report.AppendLine($"{"Data",-DateWidth}{"Credits",CreditsWidth}{"Cost",CostWidth}");
        report.AppendLine(new string('-', DateWidth + CreditsWidth + CostWidth));
    }
}

