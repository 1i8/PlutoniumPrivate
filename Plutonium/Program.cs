using Plutonium.Common;
using Plutonium.Entities;
using Plutonium.Framework;
using Plutonium.Gui;

namespace Plutonium;

internal static class Program
{
    private static void Main()
    {
        Directory.CreateDirectory(Core.Dir);
        Directory.CreateDirectory(Path.Combine(Core.Dir, "Scripts"));
        ItemHandler.SetupItemDefs();
        Render.Initialize();
        Console.Title = "Private Multibot Plutonium";
        Script.LoadScripts();
        MainWindow.Run();
    }
}