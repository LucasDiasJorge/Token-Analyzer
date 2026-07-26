using System.Reflection;
using Microsoft.Win32.TaskScheduler;

namespace TokenAnalyzer.Infrastructure;

public static class DailyTaskRegistrar
{
    public const string TaskName = "TokenAnalyzerDailyReport";

    private const string TaskDescription = "Executa o relatorio do Token Analyzer diariamente as 18:00.";
    private const string ExecuteJobArgument = "--executar-job";
    private const int DailyExecutionHour = 18;
    private const int DailyExecutionMinute = 11;
    private const string LauncherFolderName = "TokenAnalyzer";
    private const string LauncherFileName = "run-token-analyzer-hidden.vbs";

    public static void Register(string rootPath)
    {
        EnsureWindowsPlatform();

        using TaskService taskService = new TaskService();
        TaskDefinition taskDefinition = BuildTaskDefinition(taskService, rootPath);
        taskService.RootFolder.RegisterTaskDefinition(TaskName, taskDefinition);
    }

    private static void EnsureWindowsPlatform()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Windows Task Scheduler is only available on Windows.");
        }
    }

    private static TaskDefinition BuildTaskDefinition(TaskService taskService, string rootPath)
    {
        TaskDefinition taskDefinition = taskService.NewTask();
        taskDefinition.RegistrationInfo.Description = TaskDescription;
        taskDefinition.Settings.StartWhenAvailable = true;
        taskDefinition.Settings.Hidden = true;

        taskDefinition.Triggers.Add(CreateDailyTrigger());

        (string executablePath, string arguments, string workingDirectory) = BuildDoubleHiddenExecutionAction(rootPath);
        taskDefinition.Actions.Add(new ExecAction(executablePath, arguments, workingDirectory));

        return taskDefinition;
    }

    private static DailyTrigger CreateDailyTrigger()
    {
        return new DailyTrigger
        {
            StartBoundary = GetNextStartBoundary(),
            DaysInterval = 1
        };
    }

    private static DateTime GetNextStartBoundary()
    {
        DateTime startBoundary = DateTime.Today.AddHours(DailyExecutionHour).AddMinutes(DailyExecutionMinute);
        if (startBoundary <= DateTime.Now)
        {
            startBoundary = startBoundary.AddDays(1);
        }

        return startBoundary;
    }

    private static (string ExecutablePath, string Arguments, string WorkingDirectory) BuildDoubleHiddenExecutionAction(string rootPath)
    {
        (string executablePath, string arguments) = BuildHiddenExecutionCommand(rootPath);
        string launcherPath = EnsureVbScriptLauncher(executablePath, arguments, AppContext.BaseDirectory);
        return ("wscript.exe", $"//B //NoLogo {QuoteArgument(launcherPath)}", AppContext.BaseDirectory);
    }

    private static string EnsureVbScriptLauncher(string executablePath, string arguments, string workingDirectory)
    {
        string launcherDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            LauncherFolderName);
        Directory.CreateDirectory(launcherDirectory);

        string launcherPath = Path.Combine(launcherDirectory, LauncherFileName);
        string escapedExecutablePath = EscapeForVbString(executablePath);
        string escapedArguments = EscapeForVbString(arguments);
        string escapedWorkingDirectory = EscapeForVbString(workingDirectory);

        string launcherScript =
            "Set shell = CreateObject(\"WScript.Shell\")" + Environment.NewLine +
            $"shell.CurrentDirectory = \"{escapedWorkingDirectory}\"" + Environment.NewLine +
            $"command = \"\"\"{escapedExecutablePath}\"\"\"" + Environment.NewLine +
            $"arguments = \"{escapedArguments}\"" + Environment.NewLine +
            "If Len(arguments) > 0 Then" + Environment.NewLine +
            "  command = command & \" \" & arguments" + Environment.NewLine +
            "End If" + Environment.NewLine +
            "shell.Run command, 0, True" + Environment.NewLine;

        File.WriteAllText(launcherPath, launcherScript);
        return launcherPath;
    }

    private static (string ExecutablePath, string Arguments) BuildHiddenExecutionCommand(string rootPath)
    {
        (string executablePath, string arguments) = BuildExecutionCommand(rootPath);
        string command = $"& {QuoteArgument(executablePath)} {arguments}";
        string hiddenPowerShellArguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -WindowStyle Hidden -Command {QuoteArgument(command)}";
        return ("powershell.exe", hiddenPowerShellArguments);
    }

    private static (string ExecutablePath, string Arguments) BuildExecutionCommand(string rootPath)
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

    private static string EscapeForVbString(string value)
        => value.Replace("\"", "\"\"");
}