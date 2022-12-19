using Plutonium.Agent.BotSystem.Entities;
using Plutonium.Agent.Framework.AStar;

namespace Plutonium.Agent.Lua.Modules;

public class Bot
{
    //Bot UID
    public int Id;

    //The REAL bot class
    public BotSystem.Bot? RealBot;

    //Connect bot
    public void Connect()
    {
        RealBot.Connect();
    }

    //Disconnect bot
    public void Disconnect()
    {
        RealBot.Disconnect();
    }

    //Gets bot status
    public string GetStatus()
    {
        return RealBot.BotState;
    }

    //Find path to x,y
    public void FindPath(int x, int y)
    {
        RealBot.PathFinder.FindPath(new Point(x, y));
    }

    //Move bot to right by 1 block
    public void MoveRight()
    {
        RealBot.MoveRight();
    }

    //Move bot to left by 1 block
    public void MoveLeft()
    {
        RealBot.MoveLeft();
    }

    //Move bot to up by 1 block
    public void MoveUp()
    {
        RealBot.MoveUp();
    }

    //Move bot to down by 1 block
    public void MoveDown()
    {
        RealBot.MoveDown();
    }

    public void Warp(string world)
    {
        RealBot.SendPacket(3, "action|join_request\nname|" + world + "\ninvitedWorld|0");
    }

    public void Warp(string world, string id)
    {
        RealBot.SendPacket(3, $"action|join_request\nname|{world}|{id}\ninvitedWorld|0");
    }

    //Use tile in same pos as bot (E.g Enter in door)
    public void Use()
    {
        RealBot.Use();
    }

    //Reconnect the bot
    public void Reconnect()
    {
        RealBot.Reconnect();
    }

    //Gets NetAvatar Struct
    public NetAvatar GetNetAvatar()
    {
        return RealBot.NetAvatar;
    }

    //Gets World Class
    public World GetWorld()
    {
        return RealBot.World;
    }

    //Collect items what are inside the radius by default radius is 32 (1Block)
    public void Collect(int radius = 1)
    {
        RealBot.Collect(radius);
    }

    //Send packet with type and byte array
    public void SendPacket(int type, byte[] buffer)
    {
        RealBot.SendPacket(type, buffer);
    }

    //Send packet with type and GamePacket struct
    public void SendPacket(int type, GamePacket packet)
    {
        RealBot.SendPacket(type, packet);
    }

    //Send packet with type and string
    public void SendPacket(int type, string str)
    {
        RealBot.SendPacket(type, str);
    }

    //Place block to selected x,y tile with itemID
    public void PlaceBlock(int x, int y, int itemId)
    {
        RealBot.PlaceBlock(x, y, itemId);
    }

    //Punch tile to selected x,y with handID default(Fist)
    public void PunchTile(int x, int y, int handId = 18)
    {
        RealBot.PunchTile(x, y, handId);
    }

    //GetTile where bot stands
    public Tile GetTile()
    {
        return RealBot.World.Tiles.First(c => c.X == RealBot.NetAvatar.Tile.X && c.Y == RealBot.NetAvatar.Tile.Y);
    }

    //GetTile by x and y
    public Tile GetTile(int x, int y)
    {
        return RealBot.World.Tiles.First(c => c.X == x && c.Y == y);
    }

    //Get world tiles
    public Tile[] GetTiles()
    {
        return RealBot.World.Tiles;
    }

    //Get tile by id
    public Tile FindTile(int id)
    {
        return RealBot.World.Tiles.First(c => c.Foreground == id);
    }

    //Gets the world seed amount
    public int GetSeedCount()
    {
        var count = 0;
        foreach (var seed in RealBot.World.Tiles.Where(c => c.SeedData != null)) count++;

        return count;
    }

    public void DoAutofarm(int x, int y, int id, int delay = 170)
    {
        RealBot.Autofarm.DoAutoFarm(x, y, id, delay);
    }


    //Gets the first first tile where is seed
    public Tile GetFirstSeedTile()
    {
        return RealBot.World.Tiles.First(c => c.SeedData != null);
    }

    //Gets the first first tile where is seed and its ready
    public Tile GetFirstReadySeedTile()
    {
        return RealBot.World.Tiles.First(c => c.SeedData != null && c.SeedData.isReady());
    }

    //Get world dropped items
    public DroppedObject[] GetObjects()
    {
        return RealBot.World.DroppedObjects.ToArray();
    }

    //Get Inventory
    public Inventory GetInventory()
    {
        return RealBot.NetAvatar.Inventory;
    }

    public int GetItemAmount(int id)
    {
        try
        {
            return RealBot.NetAvatar.Inventory.Items.First(c => c.Id == id).Amount;
        }
        catch
        {
            //Returns nil to lua if item not found.
            return 0;
        }
    }

    //Find inventory item
    public InventoryItem? FindItem(int id)
    {
        try
        {
            return RealBot.NetAvatar.Inventory.Items.First(c => c.Id == id);
        }
        catch
        {
            //Returns nil to lua if item not found.
            return null;
        }
    }


    //Makes bot to say the str
    public void Say(string str)
    {
        RealBot.SendPacket(2, "action|input\n|text|" + str);
    }


    //Send Webhook with username
    public void SendWebhook(string webhook, string message, string username)
    {
        RealBot.SendWebhook(webhook, message, username, null);
    }

    //Send Webhook without username
    public void SendWebhook(string webhook, string message)
    {
        RealBot.SendWebhook(webhook, message, null, null);
    }

    //Download file from url
    public void download(string url, string name)
    {
        RealBot.download(url, name);
    }
}