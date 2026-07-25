using SlackNet;
using SlackNet.WebApi;
using TokenAnalyzer.Services.Interfaces;

namespace TokenAnalyzer.Services;

public sealed class SlackNotify : INotify
{
    private readonly string _email;
    private readonly ISlackApiClient _slackApiClient;

    public SlackNotify(string email, string token)
    {
        _email = email;
        _slackApiClient = new SlackServiceBuilder()
            .UseApiToken(token)
            .GetApiClient();
    }

    public async Task Notify(string message, CancellationToken cancellationToken = default)
    {
        User user = await _slackApiClient.Users.LookupByEmail(_email, cancellationToken);
        string channelId = await _slackApiClient.Conversations.Open(new[] { user.Id }, cancellationToken);
        await _slackApiClient.Chat.PostMessage(
            new Message
            {
                Channel = channelId,
                Text = message
            },
            cancellationToken);
    }
}