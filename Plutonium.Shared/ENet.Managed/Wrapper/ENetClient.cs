using System.Net;

namespace ENet.Managed;

public class ENetClient
{
    public delegate void ConnectDelegate(ENetPeer peer);

    public delegate void DisconnectDelegate(object sender, uint e);

    public delegate void ReceiveDelegate(object sender, ENetPacket e);

    public ENetHost client;
    public ENetPeer peer;

    public bool running;
    public Thread service_thread;

    /// <summary>
    ///     Initialize new enet client with new packet system to work with growtopia server
    /// </summary>
    /// <param name="peer_count"></param>
    /// <param name="income_band"></param>
    public ENetClient(short peer_count, short income_band)
    {
        client = new ENetHost(null, peer_count, 2, income_band);
        client.ChecksumWithCRC32();
        client.CompressWithRangeCoder();
        client.UseNewPacket(1);
    }

    public event ConnectDelegate Connect;
    public event DisconnectDelegate Disconnect;
    public event ReceiveDelegate Receive;

    /// <summary>
    ///     Connect client to host
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    public void connect(string host, short port)
    {
        client.Connect(new IPEndPoint(IPAddress.Parse(host), port), 2, 0);
    }

    public void start_service()
    {
        running = true;
        service_thread = new Thread(service);
        service_thread.Start();
    }

    public void destroy_client()
    {
        if (client == null) return;
        foreach (var p in client.PeerList)
            if (!p.IsNull)
                p.DisconnectNow(0);
        running = false;
        service_thread = null;
        client = null;
    }

    private void service()
    {
        while (running)
        {
            if (client.Disposed) return;
            var Event = client.Service(TimeSpan.FromMilliseconds(0));

            switch (Event.Type)
            {
                case ENetEventType.None:
                    break;
                case ENetEventType.Connect:
                    Connect(Event.Peer);
                    break;
                case ENetEventType.Disconnect:
                    Disconnect(Event.Peer, 0);
                    Event.Peer.UnsetUserData();
                    break;
                case ENetEventType.Receive:
                    Receive(Event.Peer, Event.Packet);
                    Event.Packet.Destroy();
                    break;
                default:
                    throw new NotImplementedException();
            }

            Thread.Sleep(1);
        }
    }
}