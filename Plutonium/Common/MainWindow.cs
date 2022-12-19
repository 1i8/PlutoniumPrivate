using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using Plutonium.Common;
using Plutonium.Entities;
using Plutonium.Framework;
using Plutonium.Shared;

namespace Plutonium.Gui;

public static class MainWindow
{
    private static Render _render;

    private static bool _open = true;

    private static bool _allSelected;

    public static List<string> _botList = new();
    public static int _botListIndex;

    public static List<string> _scriptList = new();
    public static int _scriptListIndex;

    private static string _agentComboList = "No Agents Found0Test0Ok";
    private static int _agentComboListIndex = 0;

    private static string _tankIdNameStr = "";
    private static string _tankPassStr = "";

    private static string _agentClaimCode = "";

    public static string UsernameStr = "$username";

    private static bool _executorMode;

    private static string scriptName = "";

    private static string _scriptContent = "";

    public static string scriptCallBack = "";

    private static int _woah = 0;

    private static string WorldNameStr = "";

    private static string AgentHostString = "";

    private static string _hostLoad = "";

    public static bool _sendPacketLogs = true;
    public static bool _packetRawLogs = true;
    public static bool _gameMessageLogs = true;
    public static bool _plutoniumLogs = true;
    public static bool _luaLogs = true;
    public static bool _variangListLogs = true;

    public static string logsString = "";

    private static int _itemDBIndex;
    public static List<string> ItemsDB = new();
    private static string _itemDBSearch = "";

    public static void Exit()
    {
        _render.Window.Close();
    }

    public static void Run()
    {
        new Thread(() =>
        {
            _render = new Render("Plutonium Private", 750, 500);
            _render.ImguiRender += RenderOnRenderCall;
            _render.RunWindow();
        }).Start();
    }

    private static void RenderOnRenderCall()
    {
        Style.ApplyStyle();
        ImGui.SetNextWindowSize(new Vector2(_render.Width, _render.Height));
        ImGui.SetNextWindowPos(new Vector2(0, 0));
        ImGui.Begin(_render.Title, ref _open,
            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize);
        ImGui.BeginGroup();

        if (_executorMode && Utils.CurrentBot() != null)
        {
            if (ImGui.BeginChild("scriptList", new Vector2(200, 400), true, ImGuiWindowFlags.NoScrollbar))
            {
                ImGui.SetNextItemWidth(185);
                ImGui.ListBox("##scriptList", ref _scriptListIndex, _scriptList.ToArray(), _scriptList.Count,
                    22);
                ImGui.EndChild();
            }

            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("##sname", "Enter script name", ref scriptName, 20);

            if (ImGui.Button("Add script", new Vector2(95, 20)))
                try
                {
                    var name = scriptName;
                    if (!scriptName.Contains(".lua")) name += ".lua";
                    Core.Scripts.Add(new Script(name));
                    _scriptList.Add(name);
                }
                catch (Exception e)
                {
                }

            ImGui.SameLine();
            if (ImGui.Button("Delete", new Vector2(95, 20)))
                try
                {
                    if (File.Exists(Core.Scripts.Where(c => c.Name == _scriptList[_scriptListIndex]).First().FilePath))
                        File.Delete(Core.Scripts.Where(c => c.Name == _scriptList[_scriptListIndex]).First().FilePath);
                    Core.Scripts.Remove(Core.Scripts.Where(c => c.Name == _scriptList[_scriptListIndex]).First());
                    _scriptList.RemoveAt(_scriptListIndex);
                    if (_scriptList.Count != 0) _scriptListIndex = _scriptList.Count - 1;
                }
                catch (Exception e)
                {
                }
        }
        else
        {
            if (ImGui.BeginChild("botList", new Vector2(200, 370), true, ImGuiWindowFlags.NoScrollbar))
            {
                ImGui.SetNextItemWidth(185);
                ImGui.ListBox("##name", ref _botListIndex, _botList.ToArray(), _botList.Count,
                    20);
                ImGui.EndChild();
            }

            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("##ghost", "Agent host [ip:port]", ref AgentHostString, 30);
            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("##gid", "growid", ref _tankIdNameStr, 23);
            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("##gpass", "password", ref _tankPassStr, 23);
            if (ImGui.Button("Add bot", new Vector2(95, 20)))
            {
                var id = Core.BotsUid++;
                Core.Bots.Add(new Bot(id, AgentHostString.Split(':')[0], ushort.Parse(AgentHostString.Split(':')[1]),
                    _tankIdNameStr, _tankPassStr));
                _botList.Add($"{id}|{_tankIdNameStr}");
            }

            ImGui.SameLine();
            if (ImGui.Button("Remove bot", new Vector2(95, 20)))
                if (_botList.Count != 0 && _botListIndex >= 0)
                    using (var packet = new Packet((int)ClientPackets.RemoveBot))
                    {
                        var cbot = Utils.CurrentBot();
                        packet.Write(cbot.Id);
                        cbot.Client.SendPacket(packet);
                        Core.Bots.Remove(cbot);
                        _botList.RemoveAt(_botListIndex);
                    }
        }

        ImGui.EndGroup();
        ImGui.SameLine();
        ImGui.BeginGroup();
        if (ImGui.BeginTabBar("##tabBar"))
        {
            var bot = Utils.CurrentBot();
            if (ImGui.BeginTabItem("Main"))
            {
                if (ImGui.BeginChild("##mainPage", new Vector2(525, 442), true))
                {
                    if (bot != null)
                    {
                        ImGui.TextUnformatted(bot.BotState);
                        if (ImGui.Button("Connect"))
                        {
                            bot.BotState = "Connecting";
                            bot.Connect();
                        }

                        ImGui.SameLine();

                        if (ImGui.Button("Disconnect"))
                        {
                            bot.BotState = "Disconnected";
                            bot.Disconnect();
                        }

                        ImGui.Separator();
                        ImGui.TextUnformatted("Bot Info");
                        if (ImGui.BeginChild("##botStats", new Vector2(505, 135), true))
                        {
                            ImGui.TextUnformatted($"NetID [{bot.NetAvatar.NetId}]");
                            ImGui.TextUnformatted($"Name [{bot.NetAvatar.Name}]");
                            ImGui.TextUnformatted($"World [{bot.World.Name}]");
                            ImGui.TextUnformatted($"Position [{bot.NetAvatar.Pos.X},{bot.NetAvatar.Pos.Y}]");
                            ImGui.TextUnformatted($"Tile [{bot.NetAvatar.Tile.X},{bot.NetAvatar.Tile.Y}]");
                            ImGui.TextUnformatted($"Level [{bot.NetAvatar.Level}]");
                            ImGui.TextUnformatted($"Gems [{bot.NetAvatar.Gems}]");
                            ImGui.EndChild();
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Control"))
            {
                if (ImGui.BeginChild("##controlPage", new Vector2(525, 442), true))
                {
                    if (bot != null)
                    {
                        ImGui.TextUnformatted($"Current World [{bot.World.Name}]");
                        ImGui.InputTextWithHint("##JoinWorld", "Enter world name", ref WorldNameStr, 26);
                        ImGui.SameLine();
                        if (ImGui.Button("Warp"))
                            bot.SendPacket(3, "action|join_request\nname|" + WorldNameStr + "\ninvitedWorld|0");
                        if (ImGui.Checkbox("Enable Access", ref bot.Globals.EnableAccess)) bot.SyncBotSettings();
                        ImGui.SameLine();
                        if (ImGui.Button("Un access me"))
                        {
                            bot.SendPacket(2, "action|input\n|text|/unaccess");
                            bot.SendPacket(2, "action|dialog_return\ndialog_name|unaccess");
                        }

                        if (ImGui.Checkbox("Enable Collecting", ref bot.Globals.EnableCollecting))
                            bot.SyncBotSettings();

                        if (ImGui.SliderInt($"Move {bot.Globals.TileAmount} Block", ref bot.Globals.TileAmount, 1, 3))
                            bot.SyncBotSettings();
                        ImGui.Spacing();
                        ImGui.Spacing();
                        ImGui.Spacing();
                        ImGui.Separator();
                        ImGui.Spacing();
                        ImGui.Spacing();
                        ImGui.Spacing();
                        ImGui.InvisibleButton("x", new Vector2(42, 42));
                        ImGui.SameLine();

                        if (ImGui.Button("Up", new Vector2(42, 42))) bot.MoveUp();
                        if (ImGui.Button("Left", new Vector2(42, 42))) bot.MoveLeft();
                        ImGui.SameLine();
                        if (ImGui.Button("Use", new Vector2(42, 42))) bot.Use();
                        ImGui.SameLine();
                        if (ImGui.Button("Right", new Vector2(42, 42))) bot.MoveRight();
                        ImGui.InvisibleButton("x", new Vector2(42, 42));
                        ImGui.SameLine();
                        if (ImGui.Button("Down", new Vector2(42, 42))) bot.MoveDown();
                    }

                    ImGui.EndChild();
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Executor"))
            {
                if (ImGui.BeginChild("##executorPage", new Vector2(525, 442), true))
                {
                    if (bot != null)
                        if (_scriptList.Count != 0)
                        {
                            ImGui.InputTextMultiline("##ls",
                                ref Core.Scripts.Where(c => c.Name == _scriptList[_scriptListIndex]).First().Content,
                                9999999,
                                new Vector2(515, 400));
                            if (ImGui.Button("Save"))
                                File.WriteAllText(
                                    Path.Combine(Core.Dir, "Scripts") + "\\" + _scriptList[_scriptListIndex],
                                    Core.Scripts.Find(c => c.Name == _scriptList[_scriptListIndex]).Content);
                            ImGui.SameLine();
                            if (ImGui.Button("Execute"))
                                using (var packet = new Packet((int)ClientPackets.BotExecuteScript))
                                {
                                    packet.Write(bot.Id);
                                    var script = Core.Scripts.Find(c => c.Name == _scriptList[_scriptListIndex])
                                        .Content;
                                    packet.Write(script);
                                    bot.Client.SendPacket(packet);
                                }

                            ImGui.SameLine();
                            if (ImGui.Button("Abort"))
                                using (var packet = new Packet((int)ClientPackets.BotExecuteScript))
                                {
                                    packet.Write(123456);
                                    bot.Client.SendPacket(packet);
                                }

                            ImGui.SameLine();
                            ImGui.TextUnformatted(scriptCallBack);
                        }

                    ImGui.EndChild();
                }

                _executorMode = true;
                ImGui.EndTabItem();
            }
            else
            {
                _executorMode = false;
            }

            if (ImGui.BeginTabItem("World"))
            {
                if (ImGui.BeginChild("##worldPage", new Vector2(525, 442), true))
                {
                    if (bot != null)
                    {
                        if (ImGui.CollapsingHeader("Dropped Objects"))
                            for (var i = 0; i < bot.World.DroppedObjects.Length; i++)
                            {
                                var dobj = bot.World.DroppedObjects[i];
                                ImGui.TextUnformatted($"ID[{dobj.Id}] | AMOUNT[{dobj.Amount}] | XY[{dobj.X},{dobj.Y}]");
                            }

                        if (ImGui.CollapsingHeader("Trees"))
                            for (var i = 0; i < bot.World.Tiles.Length; i++)
                            {
                                var tile = bot.World.Tiles[i];
                                if (tile.SeedData != null)
                                {
                                    var timeLeft = tile.SeedData.GrowTime - tile.SeedData.Time;
                                    if (timeLeft < 0) timeLeft = 0;
                                    ImGui.TextUnformatted(
                                        $"ID[{tile.Foreground}] | TIME_LEFT[{timeLeft}] | XY[{tile.X},{tile.Y}]");
                                }
                            }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Inventory"))
            {
                if (ImGui.BeginChild("##inventoryPage", new Vector2(525, 442), true))
                {
                    if (bot != null)
                        if (ImGui.CollapsingHeader("Inventory Items"))
                            for (var i = 0; i < bot.NetAvatar.Inventory.Items.Count; i++)
                            {
                                var dobj = bot.NetAvatar.Inventory.Items[i];
                                ImGui.TextUnformatted($"x{dobj.Amount} {dobj.Name}");
                                ImGui.SameLine();
                                if (ImGui.Button("Drop##" + i))
                                {
                                    bot.SendPacket(2, "action|drop\n|itemID|" + dobj.Id);
                                    Thread.Sleep(100);
                                    bot.SendPacket(2,
                                        "action|dialog_return\ndialog_name|drop_item\nitemID|" + dobj.Id + "|\ncount|" +
                                        dobj.Amount);
                                }

                                ImGui.SameLine();
                                if (ImGui.Button("Trash##" + i))
                                {
                                    bot.SendPacket(2, "action|trash\n|itemID|" + dobj.Id);
                                    Thread.Sleep(100);
                                    bot.SendPacket(2,
                                        "action|dialog_return\ndialog_name|trash_item\nitemID|" + dobj.Id +
                                        "|\ncount|" + dobj.Amount);
                                }
                            }

                    ImGui.EndChild();
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Item DB"))
            {
                if (ImGui.BeginChild("##itemPage", new Vector2(525, 442), true))
                {
                    if (ImGui.BeginChild("##itemInfo", new Vector2(510, 150), true))
                    {
                        ImGui.SetNextItemWidth(495);
                        if (ImGui.InputTextWithHint("##ItemDBName", "Enter item name", ref _itemDBSearch, 200))
                        {
                            ItemsDB.Clear();
                            if (!string.IsNullOrEmpty(_itemDBSearch))
                                foreach (var idb in ItemHandler.itemDefs.Where(c =>
                                             c.name.ToLower().StartsWith(_itemDBSearch.ToLower())))
                                    ItemsDB.Add(idb.name);
                            else
                                foreach (var idb in ItemHandler.itemDefs)
                                    ItemsDB.Add(idb.name);
                        }

                        if (ItemsDB.Count > 0)
                            try
                            {
                                var idef = ItemHandler.itemDefs.Find(c => c.name == ItemsDB[_itemDBIndex]);
                                ImGui.TextUnformatted($"Item ID: {idef.id}");
                                ImGui.TextUnformatted($"Item Action Type: {idef.actionType}");
                                ImGui.TextUnformatted($"Item Collosion Type: {idef.ctype}");
                                ImGui.TextUnformatted($"Item Grow time: {idef.growTime}");
                            }
                            catch
                            {
                            }

                        ImGui.EndChild();
                    }

                    ImGui.SetNextItemWidth(510);
                    ImGui.ListBox("##itemsList", ref _itemDBIndex, ItemsDB.ToArray(), ItemsDB.Count,
                        15);
                    ImGui.EndChild();
                }

                ImGui.EndTabItem();
            }


            if (ImGui.BeginTabItem("Logs"))
            {
                if (ImGui.BeginChild("##logsPage", new Vector2(525, 442), true))
                {
                    if (bot != null)
                    {
                        bot.BotLog.SetLines();
                        ImGui.Checkbox("SendPacket", ref _sendPacketLogs);
                        ImGui.SameLine();
                        ImGui.Checkbox("PacketRaw", ref _packetRawLogs);
                        ImGui.SameLine();
                        ImGui.Checkbox("GameMessage", ref _gameMessageLogs);
                        ImGui.SameLine();
                        ImGui.Checkbox("VariantList", ref _variangListLogs);
                        ImGui.SameLine();
                        if (ImGui.Button("Clear"))
                            using (var packet = new Packet((int)ClientPackets.BotClearLogs))
                            {
                                packet.Write(bot.Id);
                                bot.Client.SendPacket(packet);
                            }


                        ImGui.InputTextMultiline("##xdLogs",
                            ref logsString,
                            9999999,
                            new Vector2(515, 400), ImGuiInputTextFlags.ReadOnly);
                    }

                    ImGui.EndChild();
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Misc"))
            {
                if (ImGui.BeginChild("##miscPage", new Vector2(525, 442), true))
                {
                    ImGui.InputTextWithHint("##LoadHost", "ip:port", ref _hostLoad, 30);
                    ImGui.SameLine();

                    if (ImGui.Button("Load bots"))
                    {
                        var ip = _hostLoad.Split(':')[0];
                        var port = int.Parse(_hostLoad.Split(':')[1]);
                        Core.Bots.Add(new Bot(9999, ip, (ushort)port, "123", "123"));
                    }

                    ImGui.Separator();
                    if (ImGui.Button("Save bots to file"))
                    {
                        var jarr = new JArray();
                        foreach (var bb in Core.Bots)
                        {
                            var jobj = new JObject();
                            jobj.Add("TankIdName", bb.TankIdName);
                            jobj.Add("TankPass", bb.TankPass);
                            jobj.Add("Host", bb.Host);
                            jobj.Add("Port", bb.Port);
                            jarr.Add(jobj);
                        }

                        File.WriteAllText("bots.json", jarr.ToString());
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Load bots from file"))
                        if (File.Exists("bots.json"))
                        {
                            var jarr = JArray.Parse(File.ReadAllText("bots.json"));
                            foreach (var jobj in jarr)
                            {
                                var tin = (string)jobj["TankIdName"];
                                var tp = (string)jobj["TankPass"];
                                var ht = (string)jobj["Host"];
                                var pt = (ushort)jobj["Port"];
                                var idd = Core.BotsUid++;
                                Core.Bots.Add(new Bot(idd, ht.Trim(), pt, tin.Trim(), tp.Trim()));
                                _botList.Add($"{idd}|{tin}");
                            }
                        }

                    ImGui.EndChild();
                }

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.EndGroup();
    }
}