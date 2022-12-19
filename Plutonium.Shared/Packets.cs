namespace Plutonium.Shared;

/// <summary>
///     Sent from client to agent
/// </summary>
public enum ClientPackets
{
    AddBot = 0,
    RemoveBot = 1,
    ConnectBot = 2,
    DisconnectBot = 3,
    BotSendPacket = 4,
    SyncBotSettings = 5,
    BotMove = 6,
    BotExecuteScript = 7,
    BotClearLogs = 8
}

/// <summary>
///     Sent from agent to client
/// </summary>
public enum ServerPackets
{
    Hello = 0,
    SyncBotData = 1,
    ExecutorCallback = 2
}