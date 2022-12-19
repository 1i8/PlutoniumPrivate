using System.Collections;

namespace ENet.Managed;

/// <summary>
///     Represents ENet host's allocated peers.
/// </summary>
public sealed class ENetHostPeerList : IReadOnlyList<ENetPeer>
{
    internal ENetHostPeerList(ENetHost host)
    {
        Host = host;
    }

    public ENetHost Host { get; }
    public int Count => Host.PeersCount;

    public ENetPeer this[int index]
    {
        get
        {
            if (index < 0 || Host.PeersCount <= index)
                throw new ArgumentOutOfRangeException("Peer index is out of range.");

            return UnsafeGetPeerByIndex(index);
        }
    }

    public IEnumerator<ENetPeer> GetEnumerator()
    {
        return EnumPeers();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return EnumPeers();
    }

    private IEnumerator<ENetPeer> EnumPeers()
    {
        var count = Count;
        for (var i = 0; i < count; i++)
        {
            Host.CheckDispose();
            yield return UnsafeGetPeerByIndex(i);
        }
    }

    private unsafe ENetPeer UnsafeGetPeerByIndex(int index)
    {
        return new ENetPeer(&Host.PeersStartPtr[index]);
    }
}