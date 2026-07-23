using System.Globalization;

namespace TokenAnalyzer;

public static class Helper
{
    public static (string RootPath, DateTime StartDate, DateTime EndDate) ParseArguments(string[] args)
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

    public static DateTime ParseDate(string value, string argName)
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