using System.Globalization;
using TokenAnalyzer;

var rootPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
var startDate = args.Length > 1 ? ParseDate(args[1], "data inicial") : new DateTime(DateTime.Today.Year, 6, 1);
var endDate = args.Length > 2 ? ParseDate(args[2], "data final") : DateTime.Today;

if (!Directory.Exists(rootPath))
{
    Console.Error.WriteLine($"Diretorio nao encontrado: {rootPath}");
    Environment.Exit(1);
}

if (startDate > endDate)
{
    Console.Error.WriteLine("A data inicial nao pode ser maior que a data final.");
    Environment.Exit(1);
}

var analyzer = new ChatSessionAnalyzer();
var result = analyzer.Scan(rootPath, startDate, endDate);

Console.WriteLine("=== Relatorio de Gasto Diario (credits) ===");
Console.WriteLine($"Raiz analisada: {rootPath}");
Console.WriteLine($"Periodo: {startDate:dd/MM/yyyy} ate {endDate:dd/MM/yyyy}");
Console.WriteLine($"Pastas chatSessions encontradas: {result.ChatSessionDirectoriesFound}");
Console.WriteLine($"Pastas consideradas no periodo: {result.ChatSessionDirectoriesProcessed}");
Console.WriteLine($"Arquivos analisados: {result.FilesAnalyzed}");
Console.WriteLine($"Entradas de credits encontradas: {result.CreditEntriesFound}");
Console.WriteLine();

if (result.DailyCredits.Count == 0)
{
    Console.WriteLine("Nenhum gasto encontrado no periodo informado.");
    return;
}

const int dateWidth = 12;
const int creditsWidth = 16;

Console.WriteLine($"{"Data".PadRight(dateWidth)}{"Credits".PadLeft(creditsWidth)}");
Console.WriteLine(new string('-', dateWidth + creditsWidth));

foreach (var row in result.DailyCredits)
{
    Console.WriteLine($"{row.Key:dd/MM/yyyy}".PadRight(dateWidth) + $"{row.Value,16:F2}");
}

Console.WriteLine(new string('-', dateWidth + creditsWidth));
Console.WriteLine($"{"TOTAL".PadRight(dateWidth)}{result.TotalCredits,16:F2}");

static DateTime ParseDate(string value, string argName)
{
    var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy" };

    if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
    {
        return parsed.Date;
    }

    Console.Error.WriteLine($"Formato invalido para {argName}: {value}");
    Console.Error.WriteLine("Use: yyyy-MM-dd, dd/MM/yyyy ou dd-MM-yyyy");
    Environment.Exit(1);
    return DateTime.MinValue;
}
