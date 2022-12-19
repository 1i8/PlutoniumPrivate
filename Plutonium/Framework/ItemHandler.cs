using Plutonium.Gui;

namespace Plutonium.Framework;

public class ItemHandler
{
    public static List<ItemDefinition> itemDefs = new();

    public static bool isBackground(int itemID)
    {
        var def = GetItemDef(itemID);
        var actType = def.actionType;
        return actType == 18 || actType == 23 || actType == 28;
    }

    public static ItemDefinition GetItemDef(int itemID)
    {
        if (itemID < 0 || itemID > itemDefs.Count) return itemDefs[0];
        var def = itemDefs[itemID];
        if (def.id != itemID)
            foreach (var d in itemDefs)
                if (d.id == itemID)
                    return d;
        return def;
    }

    public static void SetupItemDefs()
    {
        var str = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.txt"));
        var lines = str.Split('\n');
        var xx = 0;

        foreach (var line in lines)
        {
            if (line.Length < 2) continue;
            var infos = line.Split(@"\");
            if (infos[0] != "add_item") continue;
            var def = new ItemDefinition();
            def.id = short.Parse(infos[1]);
            def.actionType = short.Parse(infos[4]);
            def.name = infos[6];
            def.growTime = int.Parse(infos[34]);
            def.ctype = int.Parse(infos[15]); //1 is block you cant pass

            itemDefs.Add(def);
            MainWindow.ItemsDB.Add(def.name);
            xx++;
        }

        Console.WriteLine($"Parsed {xx} items");
    }

    public struct ItemDefinition
    {
        public int id;
        public int actionType;
        public string name;
        public int growTime;
        public int ctype;
    }
}