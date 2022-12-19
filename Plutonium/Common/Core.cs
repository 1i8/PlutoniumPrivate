using Plutonium.Entities;

namespace Plutonium.Common;

public class Core
{
    public static List<Bot> Bots = new();

    public static int BotsUid;

    public static List<Script> Scripts = new();

    public static string Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "Plutonium");
}