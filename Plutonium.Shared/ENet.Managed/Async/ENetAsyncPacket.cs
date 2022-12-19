using System.Buffers;

namespace ENet.Managed.Async;

/// <summary>
///     Represents a received packet from <see cref="ENetAsyncPeer" />.
/// </summary>
public sealed class ENetAsyncPacket : IDisposable
{
    private readonly int m_PacketDataLen;
    private byte[] m_PacketDataBuffer;

    internal ENetAsyncPacket(ENetPacket packet, byte channelId)
    {
        ChannelId = channelId;
        Flags = packet.Flags & ~ENetPacketFlags.NoAllocate;

        m_PacketDataLen = packet.Data.Length;
        m_PacketDataBuffer = ArrayPool<byte>.Shared.Rent(m_PacketDataLen);
        packet.Data.CopyTo(m_PacketDataBuffer);
    }

    public byte ChannelId { get; }
    public ENetPacketFlags Flags { get; }
    public object UserData { get; set; }

    public ReadOnlyMemory<byte> Data
    {
        get
        {
            if (m_PacketDataBuffer == null)
                throw new ObjectDisposedException(nameof(ENetAsyncPacket));

            return new ReadOnlyMemory<byte>(m_PacketDataBuffer, 0, m_PacketDataLen);
        }
    }

    public void Dispose()
    {
        if (m_PacketDataBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(m_PacketDataBuffer);
            m_PacketDataBuffer = null;
        }
    }
}