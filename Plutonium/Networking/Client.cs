using System.Net;
using ENet.Managed;
using Plutonium.Shared;

namespace Plutonium.Networking;

public class Client
{
    public delegate void PacketReceivedDelegate(object id, Packet packet);

    public ENetHost? Host;

    public string Ip = "";

    public bool IsConnected;

    public ENetPeer Peer;

    public event PacketReceivedDelegate PacketReceived;

    /// <summary>
    ///     Connects client to ip and port
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public void Connect(string ip, ushort port)
    {
        Ip = ip;
        ENetWrapper.enet_initialize(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "enet.dll"));
        Host = new ENetHost(null, 2, 1);
        Host.ChecksumWithCRC32();
        Host.CompressWithRangeCoder();
        Peer = Host.Connect(new IPEndPoint(IPAddress.Parse(ip), port), 1, 0);
        new Thread(EventThread).Start();
    }

    public void SendPacket(Packet packet)
    {
        packet.WriteLength();
        Peer.Send(0, packet.ToArray(), ENetPacketFlags.Reliable);
    }


    private void OnReceive(ENetEvent Event)
    {
        var buffer = Event.Packet.Data.ToArray();
        var peer = Event.Peer;

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
                PacketReceived(id, packet);
            }

            length = 0;

            if (receivedPacket.UnreadLength() < 4) continue;

            length = receivedPacket.ReadInt();
        }
    }

    private void OnDisconnect(ENetEvent Event)
    {
        Console.WriteLine("Disconnected");
    }

    private void OnConnect(ENetEvent Event)
    {
        Peer.Timeout(1000, 4000, 6000);
        Console.WriteLine("Connected");
        IsConnected = true;
    }

    //Listen ENet Events
    private void EventThread()
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
        }
    }
}