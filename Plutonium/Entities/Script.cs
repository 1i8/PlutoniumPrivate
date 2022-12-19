using Plutonium.Common;
using Plutonium.Gui;

namespace Plutonium.Entities;

public class Script
{
    public string Content = "";

    public Script(string name, string content = "")
    {
        Name = name;
        FilePath = Path.Combine(Core.Dir, "Scripts", name);
        Content = content;
    }

    public string? Name { get; set; }

    public string? FilePath { get; set; }

    public static void LoadScripts()
    {
        foreach (var fullName in Directory.GetFiles(Path.Combine(Core.Dir, "Scripts"), "*.lua"))
        {
            var content = File.ReadAllText(fullName);
            var st = fullName.Replace(Path.Combine(Core.Dir, "Scripts") + @"\", "");
            Core.Scripts.Add(new Script(st, content));
            MainWindow._scriptList.Add(st);
        }
    }
}