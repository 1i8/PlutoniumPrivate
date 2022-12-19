namespace Plutonium.Agent.BotSystem;

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

    public static bool RequiresTileExtra(int id)
    {
        var def = GetItemDef(id);
        return
            def.actionType == 2 || // Door
            def.actionType == 3 || // Lock
            def.actionType == 10 || // Sign
            def.actionType == 13 || // Main Door
            def.actionType == 19 || // Seed
            def.actionType == 26 || // Portal
            def.actionType == 33 || // Mailbox
            def.actionType == 34 || // Bulletin Board
            def.actionType == 36 || // Dice Block
            def.actionType == 36 || // Roshambo Block
            def.actionType == 38 || // Chemical Source
            def.actionType == 40 || // Achievement Block
            def.actionType == 43 || // Sungate
            def.actionType == 46 ||
            def.actionType == 47 ||
            def.actionType == 49 ||
            def.actionType == 50 ||
            def.actionType == 51 || // Bunny Egg
            def.actionType == 52 ||
            def.actionType == 53 ||
            def.actionType == 54 || // Xenonite
            def.actionType == 55 || // Phone Booth
            def.actionType == 56 || // Crystal
            def.id == 2246 || // Crystal
            def.actionType == 57 || // Crime In Progress
            def.actionType == 59 || // Spotlight
            def.actionType == 61 ||
            def.actionType == 62 ||
            def.actionType == 63 || // Fish Wall Port
            def.id == 3760 || // Data Bedrock
            def.actionType == 66 || // Forge
            def.actionType == 67 || // Giving Tree
            def.actionType == 73 || // Sewing Machine
            def.actionType == 74 ||
            def.actionType == 76 || // Painting Easel
            def.actionType == 78 || // Pet Trainer (WHY?!)
            def.actionType == 80 || // Lock-Bot (Why?!)
            def.actionType == 81 ||
            def.actionType == 83 || // Display Shelf
            def.actionType == 84 ||
            def.actionType == 85 || // Challenge Timer
            def.actionType == 86 || // Challenge Start/End Flags
            def.actionType == 87 || // Fish Wall Mount
            def.actionType == 88 || // Portrait
            def.actionType == 89 ||
            def.actionType == 91 || // Fossil Prep Station
            def.actionType == 93 || // Howler
            def.actionType == 97 || // Storage Box Xtreme / Untrade-a-box
            def.actionType == 100 || // Geiger Charger
            def.actionType == 101 ||
            def.actionType == 111 || // Magplant
            def.actionType == 113 || // CyBot
            def.actionType == 115 || // Lucky Token
            def.actionType == 116 || // GrowScan 9000 ???
            def.actionType == 127 || // Temp. Platform
            def.actionType == 130 ||
            (def.id % 2 == 0 && def.id >= 5818 && def.id <= 5932) ||
            // ...
            false;
    }

    //add_item\3\0\0\19\0\Dirt Seed\tiles_page1.rttex\91467596\0\-1\8\20\2\0\0\20\2\0\1\200\\0\400\\\\\1\2\1\2\255,96,57,19\255,166,124,82\31\0\0\\\\0\0\\0\0
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