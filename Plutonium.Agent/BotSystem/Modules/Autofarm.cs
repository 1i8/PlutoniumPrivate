namespace Plutonium.Agent.BotSystem.Modules;

public class Autofarm
{
    public Bot? Bot;

    public void DoAutoFarm(int x, int y, int id, int delay = 170, int fist = 18)
    {
        var tile = Bot.World.Tiles.First(c => c.X == x && c.Y == y);
        var isBackground = ItemHandler.isBackground(id);
        if (isBackground)
        {
            if (tile.Background == 0)
            {
                Thread.Sleep(10);
                Bot.PlaceBlock(x, y, id);
                Thread.Sleep(delay);
            }
            else
            {
                Bot.PunchTile(x, y, fist);
                Thread.Sleep(delay);
            }
        }
        else
        {
            if (tile.Foreground == 0)
            {
                Thread.Sleep(10);
                Bot.PlaceBlock(x, y, id);
                Thread.Sleep(delay);
            }
            else
            {
                Bot.PunchTile(x, y, 18);
                Thread.Sleep(delay);
            }
        }
    }
}