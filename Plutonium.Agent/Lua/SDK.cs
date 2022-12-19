using NLua;
using Plutonium.Agent.BotSystem;
using Plutonium.Agent.Common;

namespace Plutonium.Agent.Lua;

public class SDK
{
    public static Dictionary<string, string> Hooks = new();
    public int Id;

    public SDK(int id)
    {
        Id = id;
    }

    public void AddBot(string tankIdName, string tankPass)
    {
        var lastId = Core.Bots.Max(c => c.Id) + 1;
        Core.Bots.Add(new Bot(lastId, tankIdName, tankPass, true));
        Thread.Sleep(150);
        Core.Bots[lastId].Connect();
    }

    public void RemoveBot(Bot bot)
    {
        bot.Disconnect();
        Core.Bots.Remove(bot);
    }

    public Modules.Bot? GetBot()
    {
        return Core.Bots.Find(c => c.Id == Id).LuaBot;
    }

    public Modules.Bot? GetBot(int id)
    {
        return Core.Bots.Find(c => c.Id == id).LuaBot;
    }

    public Modules.Bot[] GetBots()
    {
        var bots = new List<Modules.Bot>();
        foreach (var bot in Core.Bots) bots.Add(bot.LuaBot);

        return bots.ToArray();
    }

    public void Log(object text)
    {
        Console.WriteLine(text);
    }

    public void RunThread(LuaFunction func)
    {
        new Thread(() =>
        {
            try
            {
                func.Call();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to execute Function in thread");
            }
        }).Start();
    }

    public void Sleep(int milliseconds)
    {
        try
        {
            Thread.Sleep(milliseconds);
        }
        catch
        {
        }
    }

    public void AddHook(string hookId, string funcName)
    {
        Hooks.Add(hookId, funcName);
    }

    public void RemoveHooks()
    {
        Hooks.Clear();
    }

    public static void CallHook(string hookId, params object[] parameters)
    {
        try
        {
            var fnc = (LuaFunction)Executor.Lua[Hooks[hookId]];
            fnc.Call(parameters);
        }
        catch
        {
        }
    }
}