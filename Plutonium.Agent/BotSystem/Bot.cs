using System.Drawing;
using System.Net;
using System.Text;
using ENet.Managed;
using Newtonsoft.Json.Linq;
using Plutonium.Agent.BotSystem.Entities;
using Plutonium.Agent.BotSystem.Modules;
using Plutonium.Agent.Common;
using Plutonium.Agent.Entities;
using Plutonium.Agent.Framework;
using Plutonium.Agent.Lua.Modules;
using Plutonium.Agent.Networking;
using Plutonium.Shared;

namespace Plutonium.Agent.BotSystem;

public class Bot
{
    public Autofarm Autofarm = new();

    public BotLog BotLog = new();
    public string BotState = "Disconnected";
    public ENetClient? Client;

    public Events Events;

    public Globals Globals;

    public int Id;

    public Lua.Modules.Bot? LuaBot;

    public NetAvatar NetAvatar = new();

    public PathFinder? PathFinder;

    public ENetPeer Peer;

    public string? TankIdName, TankPass;

    public TankInfo TankInfo = new();

    public World World = new();

    public WorldStruct WorldStruct;

    public bool ChangingSubServers = false;

    public bool ForceDisconnect = false;

    public Bot(int id, string tankIdName, string tankPass, bool isLua = false)
    {
        Id = id;
        TankIdName = tankIdName;
        TankPass = tankPass;

        Events = new Events(this);

        TankInfo.TankIdName = tankIdName;
        TankInfo.TankPass = tankPass;

        LuaBot = new Lua.Modules.Bot();
        LuaBot.Id = Id;
        LuaBot.RealBot = this;

        Autofarm.Bot = this;

        //Initialize Globals
        Globals = new Globals
        {
            EnableAccess = false,
            EnableCollecting = false,
            TileAmount = 1
        };

        WorldStruct = new WorldStruct
        {
            Name = "EXIT",
            DroppedObjects = new DroppedObject[1],
            Players = new NetAvatar[1],
            Tiles = new Tile[1]
        };

        if (!isLua)
            new Thread(SyncBotDataThread).Start();
    }

    public void Reconnect()
    {
        if (!Peer.IsNull)
        {
            SendPacket(3, "action|quit");
            Disconnect();
            Thread.Sleep(150);
            Connect();
        }
    }

    public void PunchTile(int x, int y, int fistId)
    {
        var packet = new GamePacket();
        packet.type = 3;
        packet.int_data = fistId;
        packet.pos_x = NetAvatar.Pos.X;
        packet.pos_y = NetAvatar.Pos.Y;
        packet.int_x = x;
        packet.int_y = y;
        /*var legitpacket = new GamePacket();
        legitpacket.type = 0;
        legitpacket.int_data = fistId;
        legitpacket.pos_x = NetAvatar.Pos.X;
        legitpacket.pos_y = NetAvatar.Pos.Y;
        legitpacket.int_x = x;
        legitpacket.int_y = y;
        legitpacket.flags = 2560;*/
        SendPacket(4, packet);
        //SendPacket(4, legitpacket);
    }

    public void PlaceBlock(int x, int y, int itemId)
    {
        var packet = new GamePacket();
        packet.type = 3;
        packet.int_data = itemId;
        packet.pos_x = NetAvatar.Pos.X;
        packet.pos_y = NetAvatar.Pos.Y;
        packet.int_x = x;
        packet.int_y = y;
        /*var legitpacket = new GamePacket();
        legitpacket.type = 0;
        legitpacket.int_data = itemId;
        legitpacket.pos_x = NetAvatar.Pos.X;
        legitpacket.pos_y = NetAvatar.Pos.Y;
        legitpacket.int_x = x;
        legitpacket.int_y = y;
        legitpacket.flags = (1 << 5) | (1 << 10) | (1 << 11);*/
        SendPacket(4, packet);
        //SendPacket(4, legitpacket);
    }

    public void Disconnect()
    {
        if (!Peer.IsNull) Peer.DisconnectNow(0);
    }

    public void Connect()
    {
        string hstr = Utils.ParseHost();
        string host = hstr.Split(':')[0];
        ushort port = ushort.Parse(hstr.Split(':')[1]);
        Connect(host, port);
    }

    public void Connect(string host, ushort port)
    {
        Console.WriteLine($"[{TankIdName}] Connecting to [{host}:{port}]");
        Client = new ENetClient(64, 10);

        Client.Connect += ClientOnConnect;
        Client.Disconnect += ClientOnDisconnect;
        Client.Receive += ClientOnReceive;

        Client.connect(host, (short)port);
        Client.start_service();
    }

    public void Collect(int radius = 1)
    {
        foreach (var droppedObject in World.DroppedObjects)
        {
            if (Utils.isInside(new Point(droppedObject.X / 32, droppedObject.Y / 32), radius, NetAvatar.Tile))
            {
                var packet = new GamePacket();
                packet.type = 11;
                packet.pos_x = droppedObject.X;
                packet.pos_y = droppedObject.Y;
                packet.netid = -1;
                packet.int_data = droppedObject.Uid;
                SendPacket(4, packet);
            }
        }
    }

    public void SendPlayerState(int state = 0)
    {
        NetAvatar.Pos.X = NetAvatar.Pos.X / 32 * 32 + 6;
        var packet = new GamePacket();
        packet.type = 0;
        packet.pos_x = NetAvatar.Pos.X;
        packet.pos_y = NetAvatar.Pos.Y;
        packet.flags = state;
        packet.netid = -1;
        SendPacket(4, packet);
        NetAvatar.Tile.X = NetAvatar.Pos.X / 32;
        NetAvatar.Tile.Y = NetAvatar.Pos.Y / 32;

        if (Globals.EnableCollecting)
            Collect(2);
    }

    public void SyncBotDataThread()
    {
        while (true)
        {
            WorldStruct.Name = World.Name;
            WorldStruct.Players = World.Players.ToArray();
            WorldStruct.Tiles = World.Tiles;
            WorldStruct.DroppedObjects = World.DroppedObjects.ToArray();

            using (var packet = new Packet((int)ServerPackets.SyncBotData))
            {
                var netAvatarJson = JObject.FromObject(NetAvatar).ToString();
                var worldStructJson = JObject.FromObject(WorldStruct).ToString();
                var botLogsJson = JObject.FromObject(BotLog).ToString();
                packet.Write(Id);
                packet.Write(BotState);
                packet.Write(netAvatarJson);
                packet.Write(worldStructJson);
                packet.Write(botLogsJson);

                foreach (var client in Server.Clients.Keys) Networking.Events.SendPacket(client, packet);
            }

            Thread.Sleep(500);
        }
    }

    #region Event Functions

    private void ClientOnReceive(object sender, ENetPacket e)
    {
        var enetPacket = e.Data.ToArray();
        int packetType = enetPacket[0];
        Events.HandleENetPacket(packetType, enetPacket);
    }

    private void ClientOnDisconnect(object sender, uint e)
    {
        if (!ChangingSubServers &&  !ForceDisconnect)
        {
            BotLog.Append("Bot Disconnected", BotLog.LogType.Plutonium);
            BotState = "Disconnected";
            Connect();
        }
        else if (ChangingSubServers)
        {
            ChangingSubServers = false;
        }
        else if (ForceDisconnect)
        {
            ForceDisconnect = false;
        }
    }

    private void ClientOnConnect(ENetPeer peer)
    {
        BotLog.Append("Bot Connected", BotLog.LogType.Plutonium);
        peer.Timeout(1000, 4000, 6000);
        Peer = peer;
    }

    #endregion

    #region Moving Functions

    public void MoveUp()
    {
        NetAvatar.Pos.Y -= 32 * Globals.TileAmount;
        SendPlayerState((int)ENET_PACKET_FLAG.PACKET_FLAG_ON_JUMP);
    }

    public void MoveLeft()
    {
        NetAvatar.Pos.X -= 32 * Globals.TileAmount;
        SendPlayerState((int)ENET_PLAYER_STATE.UPDATE_PACKET_PLAYER_MOVING_LEFT);
    }

    public void MoveRight()
    {
        NetAvatar.Pos.X += 32 * Globals.TileAmount;
        SendPlayerState((int)ENET_PLAYER_STATE.UPDATE_PACKET_PLAYER_MOVING_RIGHT);
    }

    public void MoveDown()
    {
        NetAvatar.Pos.Y += 32 * Globals.TileAmount;
        SendPlayerState();
    }

    public void Use()
    {
        var packet = new GamePacket();
        packet.type = 7;
        packet.int_data = 18;
        packet.pos_x = NetAvatar.Pos.X;
        packet.pos_x = NetAvatar.Pos.Y;
        packet.int_x = NetAvatar.Tile.X;
        packet.int_y = NetAvatar.Tile.Y;
        SendPacket(4, packet);
        SendPlayerState((int)ENET_PACKET_FLAG.PACKET_FLAG_ROTATE_LEFT);
    }

    #endregion

    #region SendPacket Functions

    public void SendData(byte[] data)
    {
        var rand = new Random();
        if (Peer == null) return;
        if (Peer.IsNull) return;
        if (Peer.State != ENetPeerState.Connected) return;
        var x = 0;

        Peer.Send((byte)rand.Next(0, 1), data, ENetPacketFlags.Reliable);
    }

    public void SendPacket(int type, byte[] data)
    {
        if (Peer == null) return;
        if (Peer.IsNull) return;
        if (Peer.State != ENetPeerState.Connected) return;

        var packetData = new byte[data.Length + 5];
        Array.Copy(BitConverter.GetBytes(type), packetData, 4);
        Array.Copy(data, 0, packetData, 4, data.Length);
        SendData(packetData);
    }

    public void warp(string world, string id)
    {
        SendPacket(3, "action|join_request\nname|" + world + "|" + id + "\ninvitedWorld|0");
    }

    public void warp(string world)
    {
        SendPacket(3, "action|join_request\nname|" + world + "\ninvitedWorld|0");
    }

    public void SendWebhook(string webhook, string message, string username, string pp)
    {
        if (username == null) username = "Plutonium";

        using (var dcWeb = new DiscordWebhook())
        {
            try
            {
                dcWeb.ProfilePicture = pp;
                dcWeb.UserName = username;
                dcWeb.WebHook = webhook;
                dcWeb.SendMessage(message);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("\nSomething went wrong. - >\n" + e);
            }

            dcWeb.Dispose();
        }
    }

    public void download(string url, string name)
    {
        try
        {
            var client = new WebClient();
            var text = client.DownloadString(url);
            File.WriteAllText(name, text);
        }
        catch (ArgumentException e)
        {
            Console.WriteLine("\n Something went wrong :( - > \n" + e);
        }
    }


    public void SendPacket(int type, string data)
    {
        BotLog.Append($"SendPacket[{type}]\nString[{data}]", BotLog.LogType.SendPacket);
        SendPacket(type, Encoding.ASCII.GetBytes(data));
    }

    public void SendPacket(int type, GamePacket data)
    {
        SendPacket(type, data.PackForSendingRaw());
    }

    #endregion
}