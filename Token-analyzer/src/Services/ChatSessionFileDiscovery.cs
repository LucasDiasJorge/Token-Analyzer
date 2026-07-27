namespace TokenAnalyzer.Services;

internal sealed class ChatSessionFileDiscovery
{
    public IEnumerable<string> FindSessionDirectories(string rootPath)
        => FindDirectoriesByName(rootPath, "chatSessions");

    public IReadOnlyList<ChatSessionFile> FindFilesInRange(
        string rootDirectory,
        DateTime startDate,
        DateTime endDate)
    {
        return EnumerateFilesSafe(rootDirectory)
            .Select(path => new ChatSessionFile(path, File.GetLastWriteTime(path)))
            .Where(file => file.LastWriteTime >= startDate && file.LastWriteTime <= endDate)
            .ToArray();
    }

    public DateTime GetLastWriteTime(string directoryPath)
        => Directory.GetLastWriteTime(directoryPath);

    private static IEnumerable<string> FindDirectoriesByName(string rootPath, string targetDirectoryName)
    {
        Stack<string> stack = new Stack<string>();
        stack.Push(rootPath);

        while (stack.Count > 0)
        {
            string current = stack.Pop();

            IEnumerable<string> subDirectories;
            try
            {
                subDirectories = Directory.EnumerateDirectories(current);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }

            foreach (string subDir in subDirectories)
            {
                if (string.Equals(Path.GetFileName(subDir), targetDirectoryName, StringComparison.OrdinalIgnoreCase))
                {
                    yield return subDir;
                }

                stack.Push(subDir);
            }
        }
    }

    private static IEnumerable<string> EnumerateFilesSafe(string rootDirectory)
    {
        Stack<string> stack = new Stack<string>();
        stack.Push(rootDirectory);

        while (stack.Count > 0)
        {
            string current = stack.Pop();

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(current);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }

            foreach (string file in files)
            {
                yield return file;
            }

            IEnumerable<string> subDirectories;
            try
            {
                subDirectories = Directory.EnumerateDirectories(current);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }

            foreach (string subDir in subDirectories)
            {
                stack.Push(subDir);
            }
        }
    }
}

internal readonly record struct ChatSessionFile(string Path, DateTime LastWriteTime);