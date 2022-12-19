namespace ENet.Managed;

public static class ENetWrapper
{
    private static bool initialized;

    /// <summary>
    ///     Loads the native enet library
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static int enet_initialize(string path)
    {
        if (initialized) return 0;
        ManagedENet.Startup(new ENetStartupOptions { ModulePath = path });
        ManagedENet.Shutdown(false);
        initialized = true;
        return 0;
    }
}