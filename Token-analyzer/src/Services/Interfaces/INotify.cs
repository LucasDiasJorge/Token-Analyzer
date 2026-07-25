namespace TokenAnalyzer.Services.Interfaces;

public interface INotify
{
    Task Notify(string message, CancellationToken cancellationToken = default);
}