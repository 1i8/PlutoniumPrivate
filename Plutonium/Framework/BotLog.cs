using Plutonium.Common;
using Plutonium.Gui;

namespace Plutonium.Framework;

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

    public void SetLines()
    {
        var str = string.Empty;
        foreach (var line in lines)
        {
            var st = line.Split(':')[0];
            var type = Utils.Between(st, "[", "]");
            if (type == "SendPacket" && MainWindow._sendPacketLogs)
            {
                str += line + Environment.NewLine;
                str += "-------------------------------\n";
            }

            if (type == "PacketRaw" && MainWindow._packetRawLogs)
            {
                str += line + Environment.NewLine;
                str += "-------------------------------\n";
            }

            if (type == "GameMessage" && MainWindow._gameMessageLogs)
            {
                str += line + Environment.NewLine;
                str += "-------------------------------\n";
            }

            if (type == "Plutonium" && MainWindow._plutoniumLogs)
            {
                str += line + Environment.NewLine;
                str += "-------------------------------\n";
            }

            if (type == "Lua" && MainWindow._luaLogs)
            {
                str += line + Environment.NewLine;
                str += "-------------------------------\n";
            }

            if (type == "VariantList" && MainWindow._variangListLogs)
            {
                str += line + Environment.NewLine;
                str += "-------------------------------\n";
            }
        }

        MainWindow.logsString = str;
    }

    public void Append(string line, LogType type)
    {
        lines.Add($"[{type}]: {line}");
    }
}