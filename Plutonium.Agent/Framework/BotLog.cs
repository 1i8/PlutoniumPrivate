namespace Plutonium.Agent.Framework;

public class BotLog
{
    public enum LogType
    {
        SendPacket = 0,
        PacketRaw = 1,
        GameMessage = 2,
        Plutonium = 3,
        Lua = 4,
        VariantList = 5
    }

    public List<string> lines = new();

    public void Append(string line, LogType type)
    {
        lines.Add($"[{type}]: {line}");
    }
}