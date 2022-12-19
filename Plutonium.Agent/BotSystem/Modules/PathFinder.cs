using Plutonium.Agent.Framework.AStar;

namespace Plutonium.Agent.BotSystem.Modules;

public class PathFinder
{
    private const int COOLDOWN = 10;
    private readonly Bot? _bot;

    public PathFinder(Bot bot)
    {
        _bot = bot;
    }

    public void FindPath(Point targetPos)
    {
        //Normal Growtopia World is 6000Tiles so its 100x60
        var tilesMap = new float[100, 60];
        foreach (var tile in _bot.World.Tiles)
        {
            var itemDef = ItemHandler.itemDefs.Find(c => c.id == tile.Foreground);
            var isWalkAble = itemDef.ctype != 1;
            tilesMap[tile.X, tile.Y] = isWalkAble ? 1.0f : 0.0f;
        }

        var paths =
            Pathfinding.FindPath(new Grid(tilesMap), new Point(_bot.NetAvatar.Pos.X / 32, _bot.NetAvatar.Pos.Y / 32),
                targetPos);

        foreach (var path in paths)
        {
            _bot.NetAvatar.Pos.X = path.x * 32;
            _bot.NetAvatar.Pos.Y = path.y * 32;
            _bot.SendPlayerState();
            Thread.Sleep(COOLDOWN);
        }
    }
}