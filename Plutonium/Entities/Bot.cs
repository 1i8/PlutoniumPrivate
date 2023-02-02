using System.Text;
using Newtonsoft.Json.Linq;
using Plutonium.Common;
using Plutonium.Framework;
using Plutonium.Gui;
using Plutonium.Networking;
using Plutonium.Shared;

namespace Plutonium.Entities;

public class Bot
{
    public BotLog BotLog = new();
    public string BotState = "Disconnected";

    public Client Client = new();

    public Globals Globals;

    public string Host;
    public int Id;

    public NetAvatar NetAvatar;

    public ushort Port;

    public string TankIdName;

    public string TankPass;

    public WorldStruct World;

    public Bot(int id, string host, ushort port, string tankIdName, string tankPass)
    {
        Id = id;
        Host = host;
        Port = port;
        TankIdName = tankIdName;
        TankPass = tankPass;

        //Initialize Globals
        Globals = new Globals
        {
            EnableAccess = false,
            EnableCollecting = false,
            TileAmount = 1
        };

        Client.PacketReceived += ClientOnPacketReceived;

        //Connect new  client
        if (Core.Bots.Find(c => c.Client.Ip == host) == null)
        {
            Client.Connect(host, port);

            //Wait client to Connect
            while (!Client.IsConnected) Thread.Sleep(25);
        }
        else
        {
            Client = Core.Bots.Find(c => c.Client.Ip == host).Client;
        }

        //Add bot to exsisting connection
        if (!string.IsNullOrEmpty(tankPass))
            using (var packet = new Packet((int)ClientPackets.AddBot))
            {
                packet.Write(Id);
                packet.Write(TankIdName);
                packet.Write(TankPass);
                Client.SendPacket(packet);
            }
    }

    private void ClientOnPacketReceived(object id, Packet packet)
    {
        switch ((ServerPackets)id)
        {
            case ServerPackets.Hello:
            {
                if (TankIdName == "123")
                {
                    var bId = 0;
                    var jarr = JArray.Parse(packet.ReadString());
                    foreach (JObject jobj in jarr)
                    {
                        var bb = (int)jobj["id"];
                        var name = (string)jobj["name"];
                        var exists = false;
                        foreach (var bot in Core.Bots)
                            if (bot.TankIdName.ToLower() == name.ToLower())
                                exists = true;

                        if (!exists)
                        {
                            if (bb > bId && bb != 999) bId = bb;
                            Core.Bots.Add(new Bot(bb, Host, Port, name, ""));
                            MainWindow._botList.Add(bb + "|" + name);
                        }
                    }

                    Core.BotsUid = bId + 1;
                }

                break;
            }
            case ServerPackets.SyncBotData:
            {
                try
                {
                    var botID = packet.ReadInt();

                    var rb = Utils.FindBot(botID);
                    rb.BotState = packet.ReadString();
                    var netAvatarJson = packet.ReadString();
                    var worldStructJson = packet.ReadString();
                    var botLogJson = packet.ReadString();

                    rb.NetAvatar = JObject.Parse(netAvatarJson).ToObject<NetAvatar>();
                    rb.World = JObject.Parse(worldStructJson).ToObject<WorldStruct>();
                    rb.BotLog = JObject.Parse(botLogJson).ToObject<BotLog>();
                }
                catch
                {
                }

                break;
            }
            case ServerPackets.ExecutorCallback:
            {
                MainWindow.scriptCallBack = packet.ReadString();
                break;
            }
        }
    }

    public void SyncBotSettings()
    {
        using (var packet = new Packet((int)ClientPackets.SyncBotSettings))
        {
            packet.Write(Id);
            packet.Write(JObject.FromObject(Globals).ToString());
            Client.SendPacket(packet);
        }
    }

    public void MoveUp()
    {
        SendMovePacket(0);
    }

    public void MoveDown()
    {
        SendMovePacket(1);
    }

    public void MoveLeft()
    {
        SendMovePacket(2);
    }

    public void MoveRight()
    {
        SendMovePacket(3);
    }

    public void Use()
    {
        SendMovePacket(4);
    }

    private void SendMovePacket(int type)
    {
        using (var packet = new Packet((int)ClientPackets.BotMove))
        {
            packet.Write(Id);
            packet.Write(type);
            Client.SendPacket(packet);
        }
    }

    public void Connect()
    {
        using (var packet = new Packet((int)ClientPackets.ConnectBot))
        {
            packet.Write(Id);
            Client.SendPacket(packet);
        }
    }

    public void Disconnect()
    {
        using (var packet = new Packet((int)ClientPackets.DisconnectBot))
        {
            packet.Write(Id);
            Client.SendPacket(packet);
        }
    }

    public void SendPacket(int type, string content)
    {
        SendPacket(type, Encoding.ASCII.GetBytes(content));
    }

    public void SendPacket(int type, byte[] buffer)
    {
        using (var packet = new Packet((int)ClientPackets.BotSendPacket))
        {
            packet.Write(Id);
            packet.Write(type);
            packet.Write(buffer.Length);
            packet.Write(buffer);
            Client.SendPacket(packet);
        }
    }
}