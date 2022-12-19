using Plutonium.Agent.BotSystem;
using Plutonium.Agent.Common;
using Plutonium.Agent.Framework;
using Plutonium.Agent.Networking;

namespace Plutonium.Agent;

internal static class Program
{
    private static void Main()
    {
        Directory.CreateDirectory(Core.DataDir);
        Directory.CreateDirectory(Core.BotsDir);
        //Start server
        ItemHandler.SetupItemDefs();
        new Thread(WebServer.RunHTTP).Start();
        Server.Start();
    }
}