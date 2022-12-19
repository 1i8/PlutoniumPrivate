using System.Diagnostics;
using NLua.Exceptions;
using ThreadState = System.Threading.ThreadState;

namespace Plutonium.Agent.Lua;

public class Executor
{
    public static NLua.Lua Lua;

    public static Thread executeThread;

    public static ExecutingCompleted Execute(string script, int id)
    {
        if (string.IsNullOrEmpty(script))
            return new ExecutingCompleted { Success = false, Message = "Cannot execute empty script." };

        script = $"import('Plutonium.Agent', 'Plutonium.Agent.Lua')\nPlutonium = SDK({id})\n{script}";

        var watch = new Stopwatch();

        Lua = new NLua.Lua();

        Lua.LoadCLRPackage();

        var errx = new ExecutingCompleted { Success = true, Message = "w" };

        executeThread = new Thread(() =>
        {
            watch.Start();
            try
            {
                Lua.DoString(script);
            }
            catch (LuaScriptException err)
            {
                errx = new ExecutingCompleted { Success = false, Message = err.Message };
            }

            watch.Stop();
        });
        executeThread.Start();

        while (executeThread.ThreadState == ThreadState.Running) Thread.Sleep(1);

        if (errx.Success)
            return new ExecutingCompleted { Success = true, Message = $"Executed in {watch.ElapsedMilliseconds}ms" };

        return errx;
    }
}

public struct ExecutingCompleted
{
    public bool Success;
    public string Message;
}