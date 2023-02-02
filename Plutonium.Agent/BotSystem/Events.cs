using System.Drawing;
using System.Text;
using Plutonium.Agent.BotSystem.Entities;
using Plutonium.Agent.BotSystem.Modules;
using Plutonium.Agent.Common;
using Plutonium.Agent.Framework;
using Plutonium.Agent.Lua;

namespace Plutonium.Agent.BotSystem;

public class Events
{
    private readonly Bot? _bot;

    public Events(Bot bot)
    {
        _bot = bot;
    }

    private string GetProperGenericText(byte[] data)
    {
        var growtopia_text = string.Empty;
        if (data.Length > 5)
        {
            var len = data.Length - 5;
            var croppedData = new byte[len];
            Array.Copy(data, 4, croppedData, 0, len);
            growtopia_text = Encoding.ASCII.GetString(croppedData);
        }

        return growtopia_text;
    }

    public void HandleENetPacket(int ept, byte[] buffer)
    {
        var packetType = (PacketTypes.ENET_MESSAGE_TYPE)ept;


        switch (packetType)
        {
            case PacketTypes.ENET_MESSAGE_TYPE.NET_MESSAGE_SERVER_HELLO:
                SERVER_HELLO();
                break;
            case PacketTypes.ENET_MESSAGE_TYPE.NET_MESSAGE_GENERIC_TEXT:
                try
                {
                    var str = GetProperGenericText(buffer);
                    _bot.BotLog.Append(str, BotLog.LogType.GameMessage);
                    if (str.Contains("`4Sorry, this account, device or location has been tempora"))
                        _bot.BotState = "Banned";

                    SDK.CallHook("OnPacket", str, packetType);
                }
                catch
                {
                }

                break;
            case PacketTypes.ENET_MESSAGE_TYPE.NET_MESSAGE_GAME_MESSAGE:
                try
                {
                    var str = GetProperGenericText(buffer);
                    _bot.BotLog.Append(str, BotLog.LogType.GameMessage);
                    if (str.Contains("`4Sorry, this account, device or location has been tempora"))
                        _bot.BotState = "Banned";

                    SDK.CallHook("OnPacket", str, packetType);
                }
                catch
                {
                }

                break;
            case PacketTypes.ENET_MESSAGE_TYPE.NET_MESSAGE_GAME_PACKET:
                HandleGamePacket(buffer);
                break;
            case PacketTypes.ENET_MESSAGE_TYPE.NET_MESSAGE_TRACK:
                NET_MESSAGE_TRACK(buffer);
                break;
        }
    }

    private void NET_MESSAGE_TRACK(byte[] buffer)
    {
        var trackMessage = Encoding.ASCII.GetString(buffer.Skip(4).ToArray());
        if (trackMessage.Contains("Friends_nb|"))
        {
            var lines = trackMessage.Split('\n');
            foreach (var line in lines)
            {
                var key = line.Split('|')[0];
                if (!line.Contains("^"))
                    try
                    {
                        var value = line.Split('|')[1];
                        switch (key)
                        {
                            case "Gems_balance":
                                _bot.NetAvatar.Gems = int.Parse(value);
                                break;
                            case "Level":
                                _bot.NetAvatar.Level = int.Parse(value);
                                break;
                        }
                    }
                    catch
                    {
                    }
            }
        }

        if (trackMessage.Contains("Authenication_Error|6")) _bot.BotState = "Login Failed";
    }

    private void SERVER_HELLO()
    {
        //Send Login Packet
        _bot.BotState = "Logging in";
        _bot.SendPacket(2, _bot.TankInfo.MakeLogon());
    }

    private void HandleGamePacket(byte[] buffer)
    {
        var tankPacket = Variant.get_struct_data(buffer);
        if (tankPacket == null) return;
        var tankPacketType = tankPacket[0];
        var varType = (PacketTypes.ENET_PACKET_TYPE)tankPacketType;
        var tPacket = GamePacket.UnpackFromPacket(buffer);
        SDK.CallHook("OnRawPacket", tPacket);
        switch (varType)
        {
            case PacketTypes.ENET_PACKET_TYPE.PACKET_STATE:
                PACKET_STATE(tPacket);
                break;
            case PacketTypes.ENET_PACKET_TYPE.PACKET_CALL_FUNCTION:
                PACKET_CALL_FUNCTION(tankPacket);
                break;
            case PacketTypes.ENET_PACKET_TYPE.PACKET_PING_REQUEST:
                PING_REQUEST(tPacket);
                break;
            case PacketTypes.ENET_PACKET_TYPE.PACKET_PING_REPLY:
                //Do nothing?
                break;
            case PacketTypes.ENET_PACKET_TYPE.PACKET_SEND_MAP_DATA:
                PACKET_SEND_MAP(tankPacket);
                break;
            case PacketTypes.ENET_PACKET_TYPE.PACKET_ITEM_CHANGE_OBJECT:
                PACKET_ITEM_CHANGE_OBJECT(tPacket, buffer);
                break;
            case PacketTypes.ENET_PACKET_TYPE.PACKET_SEND_INVENTORY_STATE:
                PACKET_SEND_INVENTORY_STATE(tankPacket);
                break;
            case PacketTypes.ENET_PACKET_TYPE.PACKET_MODIFY_ITEM_INVENTORY:
                PACKET_MODIFY_ITEM_INVENTORY();
                break;
            case PacketTypes.ENET_PACKET_TYPE.PACKET_SEND_TILE_TREE_STATE:
                PACKET_SEND_TILE_TREE_STATE(tPacket);
                break;
            case PacketTypes.ENET_PACKET_TYPE.PACKET_TILE_CHANGE_REQUEST:
                PACKET_TILE_CHANGE_REQUEST(tPacket);
                break;
        }
    }

    private void PACKET_TILE_CHANGE_REQUEST(GamePacket tPacket)
    {
        var tile = _bot.World.Tiles.First(c => c.X == tPacket.int_x && c.Y == tPacket.int_y);
        if (tPacket.int_data == 18)
        {
            if (tile.Foreground != 0)
                tile.Foreground = 0;
            else
                tile.Background = 0;
        }
        else
        {
            var item = ItemHandler.itemDefs.Find(c => c.id == tPacket.int_data);
            if (item.actionType == 18)
            {
                tile.Background = (ushort)tPacket.int_data;
            }
            else if (item.actionType == 19)
            {
                tile.Foreground = (ushort)tPacket.int_data;
                tile.SeedData = new SeedData(item.growTime);
                new Thread(tile.SeedData.TimeThread).Start();

                _bot.WorldStruct.Tiles.First(c => c.X == tPacket.int_x && c.Y == tPacket.int_y).SeedData =
                    tile.SeedData;
            }
            else
            {
                tile.Foreground = (ushort)tPacket.int_data;
            }
        }

        if (tPacket.netid == _bot.NetAvatar.NetId)
        {
            var items = _bot.NetAvatar.Inventory.Items;
            foreach (var item in items)
                if (item.Id == tPacket.int_data && tPacket.int_data != 18)
                {
                    if (item.Amount > 1)
                    {
                        var inventoryItem =
                            _bot.NetAvatar.Inventory.Items.Find(c => c.Id == item.Id && c.Amount == item.Amount);
                        inventoryItem.Amount -= 1;
                    }
                    else
                    {
                        var il = _bot.NetAvatar.Inventory.Items.Remove(item);
                    }
                }
        }
    }

    private void PACKET_SEND_TILE_TREE_STATE(GamePacket tPacket)
    {
        var tile = _bot.World.Tiles.First(c => c.X == tPacket.int_x && c.Y == tPacket.int_y);
        if (tPacket.item == -1)
        {
            tile.Foreground = 0;
            tile.SeedData = null;
        }
    }


    private void PACKET_MODIFY_ITEM_INVENTORY()
    {
        //TODO Debug the packet to see what values does change and convert the c++ code to c#
        /*	if (s_ptr->operator[](packet->m_int_data).count > packet->m_jump_amount) s_ptr->operator[](packet->m_int_data).count -= packet->m_jump_amount;
else  s_ptr->erase(packet->m_int_data);*/
        var items = _bot.NetAvatar.Inventory.Items;
    }


    private void PACKET_SEND_INVENTORY_STATE(byte[] tankPacket)
    {
        _bot.NetAvatar.SerializeInventory(Variant.get_struct_data(tankPacket));
    }

    private void PACKET_ITEM_CHANGE_OBJECT(GamePacket tPacket, byte[] data)
    {
        if (tPacket.pos_x == 0 && tPacket.pos_y == 0)
        {
            var droppedIem = _bot.World.DroppedObjects.Find(c => c.Uid == tPacket.int_data);
            var pa = _bot.NetAvatar.Inventory.GetObjectAmountToPickUp(droppedIem);
            if (tPacket.netid == _bot.NetAvatar.NetId || tPacket.netid == -1)
            {
                if (droppedIem.Id != 112)
                {
                    var itemExsits = _bot.NetAvatar.Inventory.Items.Exists(c => c.Id == droppedIem.Id);
                    if (itemExsits)
                    {
                        for (var i = 0; i < _bot.NetAvatar.Inventory.Items.Count; i++)
                            if (_bot.NetAvatar.Inventory.Items[i].Id == droppedIem.Id)
                                _bot.NetAvatar.Inventory.Items[i].Amount += (short)pa;
                    }
                    else
                    {
                        var item = new InventoryItem();
                        item.Name = ItemHandler.itemDefs.Find(c => c.id == droppedIem.Id).name;
                        item.Id = (ushort)droppedIem.Id;
                        item.Amount = (short)pa;
                        _bot.NetAvatar.Inventory.Items.Add(item);
                    }
                }
                else
                {
                    _bot.NetAvatar.Gems++;
                }
            }

            _bot.World.DroppedObjects.Remove(droppedIem);
        }
        else
        {
            var item = new DroppedObject();
            item.Id = tPacket.int_data;
            item.X = (int)tPacket.pos_x;
            item.Y = (int)tPacket.pos_y;
            item.Amount = (ushort)BitConverter.ToSingle(data, 20);
            item.Uid = ++_bot.World.LastDroppedUid;
            _bot.World.DroppedObjects.Add(item);
        }
    }

    private void PACKET_SEND_MAP(byte[] tankPacket)
    {
        _bot.World = _bot.World.Load(tankPacket);
        _bot.PathFinder = new PathFinder(_bot);
    }

    private void PING_REQUEST(GamePacket tPacket)
    {
        var packet = new GamePacket();
        packet.type = (int)PacketTypes.ENET_PACKET_TYPE.PACKET_PING_REPLY;
        packet.int_x = (int)1000.0f;
        packet.int_y = (int)250.0f;
        packet.pos_x = 64.0f;
        packet.pos_y = 64.0f;
        packet.int_data = tPacket.int_data;
        packet.item = (int)Utils.HashBytes(BitConverter.GetBytes(tPacket.int_data));
        _bot.SendPacket(4, packet);
    }

    private void PACKET_STATE(GamePacket tPacket)
    {
        //Track bot positions
        foreach (var bot in Core.Bots)
            if (bot.World.Name != "" && bot.NetAvatar.NetId == tPacket.netid)
            {
                bot.NetAvatar.Pos.X = (int)tPacket.pos_x;
                bot.NetAvatar.Pos.Y = (int)tPacket.pos_y;

                bot.NetAvatar.Tile.X = (int)tPacket.pos_x / 32;
                bot.NetAvatar.Tile.Y = (int)tPacket.pos_y / 32;
            }

        if (_bot.NetAvatar.NetId == tPacket.netid)
        {
            _bot.NetAvatar.Pos.X = (int)tPacket.pos_x;
            _bot.NetAvatar.Pos.Y = (int)tPacket.pos_y;

            _bot.NetAvatar.Tile.X = (int)tPacket.pos_x / 32;
            _bot.NetAvatar.Tile.Y = (int)tPacket.pos_y / 32;
        }
    }

    private void PACKET_CALL_FUNCTION(byte[] tankPacket)
    {
        var VarListFetched = Variant.GetCall(Variant.extended_data(tankPacket));
        VarListFetched.NetId = BitConverter.ToInt32(tankPacket, 4); // add netid
        VarListFetched.Delay =
            BitConverter.ToUInt32(tankPacket, 20); // add keep track of delay modifier
        SDK.CallHook("OnVarList", VarListFetched);
        HandleVariant(VarListFetched);

        if (VarListFetched.Name == "onShowCaptcha" || VarListFetched.Name == "OnShowCaptcha")
        {
            var OnShowCaptchaMSG = (string)VarListFetched.Args[1];
            var splitted = OnShowCaptchaMSG.Split("|");
            var captchaid = splitted[4];
            var captcha = splitted[1].Replace("0098/captcha/generated/", "");
            captcha = captcha.Replace("-PuzzleWithMissingPiece.rttex", "");
            //Implement here your captcha solver
            return;
        }

        if (VarListFetched.Name == "OnDialogRequest" &&
            ((string)VarListFetched.Args[1]).ToLower().Contains("captcha")) return;
    }

    private void HandleVariant(Variant.VarFunc vList)
    {
        var argStr = string.Empty;
        try
        {
            argStr = string.Empty;
            foreach (var arg in vList.Args) argStr += arg + "|";
            argStr = argStr.Remove(argStr.Length - 1);

            // Console.WriteLine($"New VariantFunction [{vList.Name}] called:\n" + argStr +"\n----------------------------------------------------------------" + "\n");

            if (vList.Args[1].ToString().Contains("CT:[SB]")) _bot.SendPacket(2, "action|input\n|text|/radio");
        }
        catch
        {
        }

        _bot.BotLog.Append($"[{vList.Name}] Called:\n {argStr}", BotLog.LogType.VariantList);


        switch (vList.Name)
        {
            case "OnSuperMainStartAcceptLogonHrdxs47254722215a":
            {
                _bot.BotState = "Connected";
                _bot.SendPacket(2, "action|enter_game");
                break;
            }
            case "OnSendToServer":
            {
                var values = argStr.Split('|');

                var doorid = values[0]; //I Don't remember if this is right but yes its in 0 or 5
                var port = int.Parse(values[1]);
                var token = values[2];
                var user = values[3];
                var ip = values[4];
                var unknown = values[5]; //idk yet what this is
                var uuid = values[6];
                var lmode = values[7];

                if (token == "-1")
                    _bot.Connect();
                else
                    OnServerRedirect(ip, (short)port, doorid, user, token, uuid, lmode);

                break;
            }
            case "OnSpawn":
            {
                var onspawn_str = (string)vList.Args[1];

                if (onspawn_str.Contains("type|local"))
                {
                    var netId = 0;
                    var name = "";
                    var x = 0;
                    var y = 0;
                    var lines = onspawn_str.Split('\n');
                    foreach (var line in lines)
                    {
                        var key = line.Split('|')[0];
                        switch (key)
                        {
                            case "netID":
                            {
                                netId = int.Parse(line.Split('|')[1]);
                                break;
                            }
                            case "posXY":
                            {
                                x = int.Parse(line.Split('|')[1]);
                                y = int.Parse(line.Split('|')[2]);
                                break;
                            }
                            case "name":
                            {
                                name = Utils.FixName(line.Split('|')[1]);
                                break;
                            }
                        }
                    }

                    _bot.NetAvatar.NetId = netId;
                    _bot.NetAvatar.Name = name;
                    _bot.NetAvatar.Pos = new Point(x, y);
                    _bot.NetAvatar.Tile = new Point(x / 32, y / 32);
                }

                break;
            }
            case "OnConsoleMessage":
            {
                var OnConsoleMessate_str = (string)vList.Args[1];
                if (OnConsoleMessate_str.Contains("Wrench yourself to accept.") && _bot.Globals.EnableAccess)
                {
                    _bot.SendPacket(2, $"action|wrench\n|netid|{_bot.NetAvatar.NetId}");
                    Thread.Sleep(250);
                    _bot.SendPacket(2,
                        $"action|dialog_return\ndialog_name|popup\nnetID|{_bot.NetAvatar.NetId}|\nbuttonClicked|acceptlock");
                    Thread.Sleep(250);
                    _bot.SendPacket(2,
                        "action|dialog_return\ndialog_name|acceptaccess");
                }

                break;
            }
        }
    }

    private void OnServerRedirect(string host, short port, string doorid, string userid, string token,
        string uuid, string lmode)
    {
        _bot.BotState = "Redirecting";
        _bot.TankInfo.DoorId = doorid;
        _bot.TankInfo.User = userid;
        _bot.TankInfo.Token = token;
        _bot.TankInfo.LMode = lmode;
        _bot.TankInfo.Uuid = uuid;
        _bot.ChangingSubServers = true;
        _bot.SendPacket(3, "action|quit");
        _bot.Peer.DisconnectNow(0);
        _bot.Connect(host, (ushort)port);
    }
}