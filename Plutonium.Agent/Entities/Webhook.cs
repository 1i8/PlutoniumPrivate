using System.Collections.Specialized;
using System.Net;

namespace Plutonium.Agent.Entities;

public class DiscordWebhook : IDisposable
{
    private static readonly NameValueCollection postValues = new();
    private readonly WebClient _webClient;

    public DiscordWebhook()
    {
        _webClient = new WebClient();
    }

    public string WebHook { get; set; }
    public string UserName { get; set; }
    public string ProfilePicture { get; set; }

    public void Dispose()
    {
        _webClient.Dispose();
        postValues.Clear();
    }

    public void SendMessage(string message)
    {
        postValues.Add("username", UserName);
        postValues.Add("avatar_url", ProfilePicture);
        postValues.Add("content", message);

        _webClient.UploadValues(WebHook, postValues);
    }
}