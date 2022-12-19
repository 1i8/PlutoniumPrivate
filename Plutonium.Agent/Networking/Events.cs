using ENet.Managed;
using Newtonsoft.Json.Linq;
using Plutonium.Agent.BotSystem;
using Plutonium.Agent.BotSystem.Entities;
using Plutonium.Agent.Common;
using Plutonium.Agent.Lua;
using Plutonium.Shared;

namespace Plutonium.Agent.Networking;

public static class Events
{
    public static void SendPacket(uint toClient, Packet packet)
    {
        packet.WriteLength();
        var peer = Server.Clients[toClient];
        peer.Send(0, packet.ToArray(), ENetPacketFlags.Reliable);
    }

    public static void SendHello(uint toClient)
    {
        using (var packet = new Packet((int)ServerPackets.Hello))
        {
            var jarr = new JArray();
            foreach (var bot in Core.Bots)
            {
                var job = new JObject();
                job.Add("id", bot.Id);
                job.Add("name", bot.TankIdName);
                jarr.Add(job);
            }

            packet.Write(jarr.ToString());
            SendPacket(toClient, packet);
        }
    }

    public static void AddBotReceived(uint fromClient, Packet packet)
    {
        var id = packet.ReadInt();
        var tankIdName = packet.ReadString();
        var tankPass = packet.ReadString();
        Core.Bots.Add(new Bot(id, tankIdName, tankPass));
    }

    public static void RemoveBotReceived(uint fromClient, Packet packet)
    {
        var id = packet.ReadInt();
        var bot = Utils.FindBot(id);
        if (bot == null)
        {
            Console.WriteLine("Bot with uid {id} not found!");
            return;
        }

        if (!bot.Peer.IsNull)
            bot.Peer.DisconnectNow(0);

        Core.Bots.Remove(bot);
    }

    public static void BotConnectReceived(uint fromClient, Packet packet)
    {
        var id = packet.ReadInt();
        var bot = Utils.FindBot(id);
        if (bot == null)
        {
            Console.WriteLine("Bot with uid {id} not found!");
            return;
        }

        bot.Connect();
    }

    public static void BotDisconnectReceived(uint fromClient, Packet packet)
    {
        var id = packet.ReadInt();
        var bot = Utils.FindBot(id);
        if (bot == null)
        {
            Console.WriteLine("Bot with uid {id} not found!");
            return;
        }

        if (!bot.Peer.IsNull)
        {
            bot.ForceDisconnect = true;
            bot.Peer.DisconnectNow(0);
        }
        bot.BotState = "Disconnected";
    }

    public static void BotSendPacketReceived(uint fromClient, Packet packet)
    {
        var id = packet.ReadInt();
        var type = packet.ReadInt();
        var bufferLength = packet.ReadInt();
        var buffer = packet.ReadBytes(bufferLength);
        var bot = Utils.FindBot(id);
        if (bot == null)
        {
            Console.WriteLine("Bot with uid {id} not found!");
            return;
        }

        bot.SendPacket(type, buffer);
    }

    public static void BotSettingsReceived(uint fromClient, Packet packet)
    {
        var id = packet.ReadInt();
        var bot = Utils.FindBot(id);
        if (bot == null)
        {
            Console.WriteLine("Bot with uid {id} not found!");
            return;
        }

        bot.Globals = JObject.Parse(packet.ReadString()).ToObject<Globals>();
    }

    public static void MovePacketReceived(uint fromClient, Packet packet)
    {
        var id = packet.ReadInt();
        var bot = Utils.FindBot(id);
        if (bot == null)
        {
            Console.WriteLine("Bot with uid {id} not found!");
            return;
        }

        var type = packet.ReadInt();
        switch (type)
        {
            case 0:
                bot.MoveUp();
                break;
            case 1:
                bot.MoveDown();
                break;
            case 2:
                bot.MoveLeft();
                break;
            case 3:
                bot.MoveRight();
                break;
            case 4:
                bot.Use();
                break;
        }
    }

    public static void BotExecuteScriptReceived(uint fromClient, Packet packet)
    {
        var id = packet.ReadInt();
        if (id == 123456)
        {
            Executor.executeThread.Interrupt();
        }
        else
        {
            var script = packet.ReadString();
            var cb = Executor.Execute(script, id);
            using (var spacket = new Packet((int)ServerPackets.ExecutorCallback))
            {
                spacket.Write(cb.Message);
                SendPacket(fromClient, spacket);
            }
        }
    }

    public static void BotClearLogsReceived(uint fromClient, Packet packet)
    {
        var id = packet.ReadInt();
        var bot = Utils.FindBot(id);
        if (bot == null) return;
        bot.BotLog.lines.Clear();
    }
}