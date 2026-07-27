namespace TokenAnalyzer.Infrastructure;

public static class InputValidator
{
    public static bool ValidateInputs(IEnumerable<string> rootPaths, DateTime startDate, DateTime endDate)
    {
        string[] roots = rootPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (roots.Length == 0)
        {
            Console.Error.WriteLine("Nenhum diretorio raiz foi informado para analise.");
            return false;
        }

        string[] missingRoots = roots
            .Where(path => !Directory.Exists(path))
            .ToArray();

        foreach (string missingRoot in missingRoots)
        {
            Console.Error.WriteLine($"Aviso: diretorio nao encontrado e sera ignorado: {missingRoot}");
        }

        if (missingRoots.Length == roots.Length)
        {
            Console.Error.WriteLine("Nenhum dos diretorios raiz existe. Nada para analisar.");
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