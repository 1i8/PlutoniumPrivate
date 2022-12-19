using Newtonsoft.Json.Linq;
using Plutonium.Agent.Common;
using Plutonium.Agent.Entities;
using Plutonium.Agent.Lua.Modules;

namespace Plutonium.Agent.BotSystem.Entities;

public class TankInfo
{
    private readonly Random _random = new();
    private string? _hash;
    private string? _hash2;
    private string? _mac;
    private string? _rid;
    private string? _wk;

    public string? TankIdName, TankPass, Meta;
    public string? User = "-1", Token = "-1", Uuid, DoorId, LMode = "0";


    public string MakeLogon()
    {
        var filePath = Path.Combine(Core.BotsDir, TankIdName.ToLower() + ".json");
        if (!File.Exists(filePath))
        {
            _mac = Utils.GenerateMAC();
            _hash = _random.Next(-777777776, 777777776).ToString();
            _hash2 = _random.Next(-777777776, 777777776).ToString();
            _wk = Utils.GenerateUniqueWinKey();
            _rid = Utils.GenerateRID();
            var jobj = new JObject();
            jobj.Add("mac", _mac);
            jobj.Add("hash", _hash);
            jobj.Add("hash2", _hash2);
            jobj.Add("wk", _wk);
            jobj.Add("rid", _rid);
            File.WriteAllText(filePath, jobj.ToString());
        }
        else
        {
            var obj = JObject.Parse(File.ReadAllText(filePath));
            _mac = (string)obj["mac"];
            _hash = (string)obj["hash"];
            _hash2 = (string)obj["hash2"];
            _wk = (string)obj["wk"];
            _rid = (string)obj["rid"];
        }

        //Meta = new Parse(Utils.request_server_data()).Get<string>("meta").Trim();
        Meta = Utils.ParseMeta();

        var parse = new Parse("");
        if (!string.IsNullOrEmpty(TankIdName))
        {
            parse.Append("tankIDName", TankIdName);
            parse.Append("tankIDPass", TankPass);
        }

        parse.Append("requestedName", "plutonium");
        parse.Append("f", "0");
        parse.Append("protocol", Config.GROWTOPIA_PROTOCOL);
        parse.Append("game_version", Config.GROWTOPIA_VERSION);
        parse.Append("fz", "16773672");
        parse.Append("lmode", LMode);
        parse.Append("cbits", "1024");
        parse.Append("player_age", "26");
        parse.Append("GDPR", "1");
        parse.Append("category", "wotd_world");
        parse.Append("totalPlaytime", "0");
        parse.Append("hash2", _hash2);
        parse.Append("meta", Meta);
        parse.Append("fhash", "-716928004");
        parse.Append("rid", _rid);
        parse.Append("platformID", "0,1,1");
        parse.Append("deviceVersion", "0");
        parse.Append("country", Config.COUNTRY_FLAG);
        parse.Append("hash", _hash);
        parse.Append("mac", _mac);
        if (Token != "-1" && Uuid != "-1")
        {
            parse.Append("user", User);
            parse.Append("token", Token);
            parse.Append("UUIDToken", Uuid);
            parse.Append("doorID", DoorId);
        }

        parse.Append("wk", _wk);
        parse.Append("zf", (-19999999 - _random.Next() % 65535).ToString());
        return parse.Serialize();
    }
}