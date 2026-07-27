using System.Text;
using TokenAnalyzer.Services;

namespace TokenAnalyzer.Presentation;

public static class ConsoleReportPrinter
{
    private const int DateWidth = 12;
    private const int CreditsWidth = 16;
    private const int CostWidth = 12;

    private static readonly string[] LowUsageMessages =
    {
        "🟢 Tudo leve por aqui! O uso ficou baixinho e o bolso agradece.",
        "✨ Check-in do bot: consumo tranquilo, sem sustos no radar.",
        "🤖 Relatorio suave: poucos tokens usados e tudo sob controle."
    };

    private static readonly string[] MediumUsageMessages =
    {
        "🟡 Opa, movimento moderado por aqui. Nada critico, mas vale ficar de olho.",
        "📊 Check-in do bot: uso medio detectado, ritmo produtivo na area.",
        "🤖 Os tokens trabalharam hoje! Gasto na faixa media e acompanhamento recomendado."
    };

    private static readonly string[] HighUsageMessages =
    {
        "🔴 Alerta amigavel do bot: uso alto detectado. Vale revisar o consumo com carinho.",
        "🚀 Os tokens aceleraram forte hoje! Gasto elevado e merece atencao.",
        "🤖 Check-in importante: consumo acima do normal. Hora de dar uma olhada no painel."
    };

    public static string PrintSummary(ScanResult result, IEnumerable<string> rootPaths, DateTime startDate, DateTime endDate)
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("=== Relatorio de Gasto Diario (tokens) ===");
        report.AppendLine("Raizes analisadas:");
        foreach (string rootPath in rootPaths)
        {
            report.AppendLine($"- {rootPath}");
        }
        report.AppendLine($"Periodo: {startDate:dd/MM/yyyy} ate {endDate:dd/MM/yyyy}");
        report.AppendLine($"Pastas chatSessions encontradas: {result.ChatSessionDirectoriesFound}");
        report.AppendLine($"Pastas consideradas no periodo: {result.ChatSessionDirectoriesProcessed}");
        report.AppendLine($"Arquivos analisados: {result.FilesAnalyzed}");
        report.Append($"Entradas de tokens encontradas: {result.CreditEntriesFound}");

        return report.ToString();
    }

    public static string PrintDailyReport(ScanResult result)
    {
        if (result.DailyCredits.Count == 0)
        {
            const string emptyReport = "Nenhum gasto encontrado no periodo informado.";
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

        return report.ToString();
    }

    public static string BuildSlackMessage(ScanResult result, DateTime startDate, DateTime endDate, string summary, string dailyReport)
    {
        decimal totalCostUsd = result.TotalCredits / 100m;
        string usageMessage = PickUsageMessage(totalCostUsd);

        StringBuilder report = new StringBuilder();
        report.AppendLine("🤖 Token Analyzer Bot");
        report.AppendLine(usageMessage);
        report.AppendLine();
        report.AppendLine($"📅 Periodo: {startDate:dd/MM/yyyy} ate {endDate:dd/MM/yyyy}");
        report.AppendLine($"🧮 Total de tokens: {result.TotalCredits:F2}");
        report.AppendLine($"💵 Custo estimado (USD): ${totalCostUsd:F2}");
        report.AppendLine();
        report.AppendLine(summary);
        report.AppendLine();
        report.Append(dailyReport);
        return report.ToString();
    }

    private static void PrintTableHeader(StringBuilder report)
    {
        report.AppendLine($"{"Data",-DateWidth}{"Credits",CreditsWidth}{"Cost",CostWidth}");
        report.AppendLine(new string('-', DateWidth + CreditsWidth + CostWidth));
    }

    private static string PickUsageMessage(decimal totalCostUsd)
    {
        string[] selected = totalCostUsd <= 9m
            ? LowUsageMessages
            : totalCostUsd <= 30m
                ? MediumUsageMessages
                : HighUsageMessages;

        return selected[Random.Shared.Next(selected.Length)];
    }
}

