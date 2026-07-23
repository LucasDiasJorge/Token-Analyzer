namespace TokenAnalyzer;

public static class Validator
{
    public static bool ValidateInputs(string rootPath, DateTime startDate, DateTime endDate)
    {
        if (!Directory.Exists(rootPath))
        {
            Console.Error.WriteLine($"Diretorio nao encontrado: {rootPath}");
            return false;
        }

        if (startDate > endDate)
        {
            Console.Error.WriteLine("A data inicial nao pode ser maior que a data final.");
            return false;
        }

        return true;
    }
}