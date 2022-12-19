using System.Net;
using ENet.Managed;
using Plutonium.Agent.Common;
using Plutonium.Shared;

namespace Plutonium.Agent.Networking;

public class Server
{
    public static ENetHost? Host;

    //keep track of clients NEVER reset this in runtime.
    private static uint ClientsUid;

    public static Dictionary<uint, ENetPeer> Clients = new();

    public static void Start()
    {
        Console.WriteLine("Starting server");
        //Initialize enet from native c++ enet.dll
        ENetWrapper.enet_initialize(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "enet.dll"));

        //Create new enet host on ip any port 1300 and max peers (clients) 10 connected same time.
        Host = new ENetHost(new IPEndPoint(IPAddress.Any, 1300), 10, 1);
        Host.ChecksumWithCRC32();
        Host.CompressWithRangeCoder();
        new Thread(EventThread).Start();
        Console.WriteLine("Server running");
    }


    private static void OnReceive(ENetEvent Event)
    {
        var buffer = Event.Packet.Data.ToArray();
        var peer = Event.Peer;

        if (peer.State != ENetPeerState.Connected) return;

        var receivedPacket = new Packet();
        receivedPacket.SetBytes(buffer);

        var length = 0;

        if (receivedPacket.UnreadLength() >= 4)
        {
            length = receivedPacket.ReadInt();
            if (length <= 0)
                return;
        }

        while (length > 0 && length <= receivedPacket.UnreadLength())
        {
            using (var packet = new Packet(receivedPacket.ReadBytes(length)))
            {
                var id = packet.ReadInt();
                Core.PacketHandlers[id](Clients.First(c => c.Value == peer).Key, packet);
            }

            length = 0;

            if (receivedPacket.UnreadLength() < 4) continue;

            length = receivedPacket.ReadInt();
        }
    }

    private static void OnDisconnect(ENetEvent Event)
    {
        var peer = Event.Peer;
        var client = Clients.First(c => c.Value == peer);
        Console.WriteLine($"Client {client.Key} Disconnected.");
        Clients.Remove(client.Key);
    }

    private static void OnConnect(ENetEvent Event)
    {
        Console.WriteLine($"New client {ClientsUid} connected from {Event.Peer.GetRemoteEndPoint()}");
        var peer = Event.Peer;
        var id = ClientsUid++;
        Clients.Add(id, peer);
        Events.SendHello(id);
    }

    //Listen ENet Events
    private static void EventThread()
    {
        while (true)
        {
            var Event = Host.Service(TimeSpan.FromMilliseconds(15));
            switch (Event.Type)
            {
                case ENetEventType.Connect:
                    OnConnect(Event);
                    break;
                case ENetEventType.Disconnect:
                    OnDisconnect(Event);
                    break;
                case ENetEventType.Receive:
                    OnReceive(Event);
                    //Destroy packet after its handled
                    Event.Packet.Destroy();
                    break;
            }

            Thread.Sleep(1);
        }
    }
}