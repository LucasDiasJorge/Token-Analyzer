using TokenAnalyzer.Infrastructure;
using TokenAnalyzer.Presentation;
using TokenAnalyzer.Services;
using TokenAnalyzer.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace TokenAnalyzer;

public static class Program
{
    private const string ExecuteJobArgument = "--executar-job";
    private static readonly bool debug = Environment.GetCommandLineArgs().Contains("--debug");

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
            List<string> rootPaths = GetWorkspaceRoots();
            string rootPath = rootPaths.First();
            DailyTaskRegistrar.Register(rootPath);
            Console.WriteLine($"Tarefa '{DailyTaskRegistrar.TaskName}' registrada para executar diariamente as 18:00.");
            Console.WriteLine("Raizes que serao analisadas:");
            foreach (string path in rootPaths)
            {
                Console.WriteLine($"- {path}");
            }
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
        List<string> rootPaths = GetWorkspaceRoots();
        DateTime startDate = DateTime.Now.AddHours(-8);
        DateTime endDate = startDate.AddDays(1);
        if (!InputValidator.ValidateInputs(rootPaths, startDate, endDate))
            return 1;

        List<string> existingRootPaths = rootPaths
            .Where(Directory.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        ScanResult result = RunAnalysis(existingRootPaths, startDate, endDate);
        
        string summary = "";
        string dailyReport = "";
        
        if (debug)
        {
            summary = ConsoleReportPrinter.PrintSummary(result, existingRootPaths, startDate, endDate);
            dailyReport = ConsoleReportPrinter.PrintDailyReport(result);
        }

        string? slackToken, email;
        Configure(out slackToken, out email);

        if (!string.IsNullOrWhiteSpace(slackToken) && !string.IsNullOrWhiteSpace(email))
        {
            INotify notify = new SlackNotify(email, slackToken);
            string slackMessage = ConsoleReportPrinter.BuildSlackMessage(result, startDate, endDate, debug ? summary : "", debug ? dailyReport : "");
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

    private static ScanResult RunAnalysis(IEnumerable<string> rootPaths, DateTime startDate, DateTime endDate)
    {
        ChatSessionAnalyzer analyzer = new ChatSessionAnalyzer();
        return analyzer.Scan(rootPaths, startDate, endDate);
    }

    private static List<string> GetWorkspaceRoots()
    {
        return ChatSessionAnalyzer
            .GetWorkspaceStoragePaths("Code", "Code - Insiders")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}