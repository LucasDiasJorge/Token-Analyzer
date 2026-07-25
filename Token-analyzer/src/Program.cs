using TokenAnalyzer.Infrastructure;
using TokenAnalyzer.Presentation;
using TokenAnalyzer.Services;
using TokenAnalyzer.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace TokenAnalyzer;

public static class Program
{
    private static async Task<int> Main(string[] args)
    {

        (string rootPath, DateTime startDate, DateTime endDate) = ArgumentParser.ParseArguments(args);
        if (!InputValidator.ValidateInputs(rootPath, startDate, endDate))
            return 1;

        ScanResult result = RunAnalysis(rootPath, startDate, endDate);
        string summary = ConsoleReportPrinter.PrintSummary(result, rootPath, startDate, endDate);
        string dailyReport = ConsoleReportPrinter.PrintDailyReport(result);
        string? slackToken, email;
        Configure(out slackToken, out email);

        if (!string.IsNullOrWhiteSpace(slackToken) && !string.IsNullOrWhiteSpace(email))
        {
            INotify notify = new SlackNotify(email, slackToken);
            await notify.Notify($"Relatório de gasto diário:\n{summary}\n{dailyReport}");
        }
        else
        {
            Console.WriteLine("Slack notification skipped because Slack settings are incomplete.");
        }

        return 0;
    }

    private static void Configure(out string? slackToken, out string? email)
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();
        IConfiguration configuration = builder.Build();
        slackToken = configuration["Slack:Token"];
        email = configuration["Slack:Email"];
    }

    private static ScanResult RunAnalysis(string rootPath, DateTime startDate, DateTime endDate)
    {
        ChatSessionAnalyzer analyzer = new ChatSessionAnalyzer();
        return analyzer.Scan(rootPath, startDate, endDate);
    }
}