using TokenAnalyzer.Infrastructure;
using TokenAnalyzer.Presentation;
using TokenAnalyzer.Services;
using TokenAnalyzer.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace TokenAnalyzer;

public static class Program
{
    private const string ExecuteJobArgument = "--executar-job";

    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            return RegisterDailyTask();
        }

        if (string.Equals(args[0], ExecuteJobArgument, StringComparison.OrdinalIgnoreCase))
        {
            return await ExecuteScheduledJob();
        }

        return await ExecuteAnalysis();
    }

    private static int RegisterDailyTask()
    {
        try
        {
            string rootPath = Directory.GetCurrentDirectory();
            DailyTaskRegistrar.Register(rootPath);
            Console.WriteLine($"Tarefa '{DailyTaskRegistrar.TaskName}' registrada para executar diariamente as 18:00.");
            Console.WriteLine($"Raiz que sera analisada: {rootPath}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Nao foi possivel registrar a tarefa: {exception.Message}");
            return 1;
        }
    }

    private static async Task<int> ExecuteScheduledJob()
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Iniciando o job diario...");

        try
        {
            int exitCode = await ExecuteAnalysis();
            Console.WriteLine(exitCode == 0 ? "Job concluido com sucesso." : "Job concluido com erro.");
            return exitCode;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Erro durante a execucao do job: {exception.Message}");
            return 1;
        }
    }

    private static async Task<int> ExecuteAnalysis()
    {
        string rootPath = Directory.GetCurrentDirectory();
        DateTime startDate = DateTime.Now;
        DateTime endDate = startDate.AddDays(1);
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
            string slackMessage = ConsoleReportPrinter.BuildSlackMessage(result, startDate, endDate, summary, dailyReport);
            await notify.Notify(slackMessage);
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