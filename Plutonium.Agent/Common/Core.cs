using Plutonium.Agent.BotSystem;
using Plutonium.Shared;
using Events = Plutonium.Agent.Networking.Events;

namespace Plutonium.Agent.Common;

public class Core
{
    public delegate void PacketHandler(uint fromClient, Packet packet);

    public static List<Bot> Bots = new();

    public static string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");

    public static string BotsDir = Path.Combine(DataDir, "Bots");

    public static List<PacketHandler> PacketHandlers = new()
    {
        Events.AddBotReceived,
        Events.RemoveBotReceived,
        Events.BotConnectReceived,
        Events.BotDisconnectReceived,
        Events.BotSendPacketReceived,
        Events.BotSettingsReceived,
        Events.MovePacketReceived,
        Events.BotExecuteScriptReceived,
        Events.BotClearLogsReceived
    };
}