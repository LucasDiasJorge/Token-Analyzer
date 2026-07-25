using System.Reflection;
using Microsoft.Win32.TaskScheduler;

namespace TokenAnalyzer.Infrastructure;

public static class DailyTaskRegistrar
{
    public const string TaskName = "TokenAnalyzerDailyReport";
    private const string ExecuteJobArgument = "--executar-job";

    public static void Register(string rootPath)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Windows Task Scheduler is only available on Windows.");
        }

        using TaskService taskService = new TaskService();
        TaskDefinition taskDefinition = taskService.NewTask();
        taskDefinition.RegistrationInfo.Description = "Executa o relatorio do Token Analyzer diariamente as 18:00.";
        taskDefinition.Settings.StartWhenAvailable = true;

        DateTime startBoundary = DateTime.Today.AddHours(18);
        if (startBoundary <= DateTime.Now)
        {
            startBoundary = startBoundary.AddDays(1);
        }

        taskDefinition.Triggers.Add(new DailyTrigger
        {
            StartBoundary = startBoundary,
            DaysInterval = 1
        });

        (string executablePath, string arguments) = GetExecutionCommand(rootPath);
        taskDefinition.Actions.Add(new ExecAction(executablePath, arguments, AppContext.BaseDirectory));
        taskService.RootFolder.RegisterTaskDefinition(TaskName, taskDefinition);
    }

    private static (string ExecutablePath, string Arguments) GetExecutionCommand(string rootPath)
    {
        string executablePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Nao foi possivel determinar o caminho do executavel.");
        string jobArguments = $"{ExecuteJobArgument} {QuoteArgument(Path.GetFullPath(rootPath))}";

        if (!string.Equals(Path.GetFileNameWithoutExtension(executablePath), "dotnet", StringComparison.OrdinalIgnoreCase))
        {
            return (executablePath, jobArguments);
        }

        string assemblyPath = Assembly.GetEntryAssembly()?.Location
            ?? throw new InvalidOperationException("Nao foi possivel determinar o assembly da aplicacao.");
        return (executablePath, $"{QuoteArgument(assemblyPath)} {jobArguments}");
    }

    private static string QuoteArgument(string value)
        => $"\"{value.Replace("\"", "\\\"")}\"";
}