namespace Plutonium.Entities;

public struct WorldStruct
{
    public string Name;
    public Tile[] Tiles;
    public DroppedObject[] DroppedObjects;
    public NetAvatar[] Players;
}

public class Tile
{
    public ushort Foreground, Background;
    public SeedData SeedData;
    public int TileState;
    public int Type;
    public int X, Y;

    public bool IsReady()
    {
        if (SeedData != null) return SeedData.isReady();

        Console.WriteLine("No seed in this tile");
        return false;
    }
}

public struct DroppedObject
{
    public int Uid;
    public int X, Y;
    public ushort Amount;
    public int Id;
}

public class SeedData
{
    public int GrowTime;
    public int Time;

    public SeedData(int growTime)
    {
        GrowTime = growTime;
    }

    public bool isReady()
    {
        return Time >= GrowTime;
    }

    public void TimeThread()
    {
        while (!isReady())
        {
            Time++;
            Thread.Sleep(1000);
        }
    }
}