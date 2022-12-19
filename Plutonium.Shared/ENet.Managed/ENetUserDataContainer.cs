namespace ENet.Managed;

internal sealed class ENetUserDataContainer<T> : IENetUserDataContainer
{
    public ENetUserDataContainer(T state)
    {
        Data = state;
    }

    public T Data { get; set; }

    public object GetData()
    {
        return Data;
    }
}