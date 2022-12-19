using System.Net;
using System.Text;
using Plutonium.Agent.Common;

namespace Plutonium.Agent.Framework;

public static class WebServer
{
    private static readonly HttpListener _httpListener = new();

    public static void RunHTTP()
    {
        _httpListener.Prefixes.Add(string.Format("http://*:{0}/", 8080));
        _httpListener.Start();
        Thread.Sleep(500);
        while (_httpListener.IsListening)
            try
            {
                var context = _httpListener.GetContext();
                var request = context.Request;
                var response = context.Response;

                if (request.HttpMethod == "GET")
                {
                    var docSite = "<!DOCTYPE html>\n" +
                                  "<html>\n" +
                                  "<body>\n" +
                                  "<h1>Plutonium Agent</h1>\n";

                    foreach (var bot in Core.Bots)
                    {
                        docSite += $"<hr>\n<h2>{bot.TankIdName}</h2>\n";
                        docSite += $"<p>State [{bot.BotState}]</p>\n" +
                                   $"<p>World [{bot.World.Name}]</p>\n" +
                                   $"<p>Gems [{bot.NetAvatar.Gems}]</p>\n" +
                                   $"<p>Level [{bot.NetAvatar.Level}]</p>\n" +
                                   $"<p>Position [{bot.NetAvatar.Pos.X},{bot.NetAvatar.Pos.Y}]</p>\n" +
                                   $"<p>Tile [{bot.NetAvatar.Tile.X},{bot.NetAvatar.Tile.Y}]</p>\n";
                    }

                    docSite += "</body>\n" +
                               "</html>\n";

                    var buffer = Encoding.UTF8.GetBytes(docSite);

                    response.ContentLength64 = buffer.Length;
                    var output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                    response.Close();
                }
            }
            catch
            {
                //Ignore if something goes  wrong
            }
    }
}