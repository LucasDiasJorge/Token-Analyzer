namespace TokenAnalyzer.Infrastructure;

public static class WorkspaceStoragePathProvider
{
    public static string GetWorkspaceStoragePath(string ide)
    {
        string userName = Environment.UserName;
        return Path.Combine("C:\\Users", userName, "AppData", "Roaming", ide, "User", "workspaceStorage");
    }

    public static IReadOnlyList<string> GetWorkspaceStoragePaths(params string[] ideNames)
    {
        return ideNames
            .Select(GetWorkspaceStoragePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}