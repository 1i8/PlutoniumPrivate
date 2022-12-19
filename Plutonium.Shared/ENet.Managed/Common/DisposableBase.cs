namespace ENet.Managed.Common;

public abstract class DisposableBase : IDisposable
{
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        if (Disposed)
            return;
        Disposed = true;

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~DisposableBase()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}