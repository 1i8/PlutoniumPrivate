using System.Drawing;

namespace Plutonium.Agent.BotSystem.Entities;

public class NetAvatar
{
    public int Gems;
    public Inventory Inventory = new();
    public int Level;
    public string Name;
    public int NetId;
    public Point Pos;
    public Point Tile;

    public void SerializeInventory(byte[] buffer)
    {
        //TODO FIX WEARED ITEMS SHOWING WRONG AMOUNT
        Inventory = new Inventory();
        Inventory.Version = buffer[0];
        Inventory.BackpackSpace = BitConverter.ToInt16(buffer, 53); //57 is old
        int itemCount = BitConverter.ToInt16(buffer, 57); // 61 is old
        Inventory.Items = new List<InventoryItem>();
        for (var i = 0; i < itemCount; i++)
        {
            var pos = 59 + i * 4; //63 is old
            var item = new InventoryItem();
            item.Id = BitConverter.ToUInt16(buffer, pos);
            item.Amount = BitConverter.ToInt16(buffer, pos + 2);
            item.Name =
                ItemHandler.itemDefs.Find(c => c.id == BitConverter.ToUInt16(buffer, pos)).name;
            Inventory.Items.Add(item);
        }
    }
}

public class InventoryItem
{
    public short Amount;
    public byte Flags;
    public ushort Id;
    public string Name;
}

public class Inventory
{
    public short BackpackSpace;
    public List<InventoryItem> Items = new();
    public byte Version;

    public bool ItemExists(int id)
    {
        var count = Items.Count(item => item.Id == id);
        return count > 0;
    }

    public int GetItemCount(int id)
    {
        return ItemExists(id) ? Items.FirstOrDefault(c => c.Id == id).Amount : 0;
    }
    
    public int GetObjectAmountToPickUp(DroppedObject obj)
    {
        var count = GetItemCount(obj.Id);

        return count != 0 ? count < 200 ? 200 - count < obj.Amount ? 200 - count : obj.Amount : 0 :
            BackpackSpace > Items.Count ? obj.Amount : 0;
    }
}