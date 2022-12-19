using System.Net;
using System.Text;
using Plutonium.Agent.BotSystem;
using Plutonium.Agent.Entities;
using Plutonium.Agent.Framework.AStar;

namespace Plutonium.Agent.Common;

public static class Utils
{
    public static string Between(string STR, string FirstString, string LastString)
    {
        string FinalString;
        var Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
        var Pos2 = STR.IndexOf(LastString);
        FinalString = STR.Substring(Pos1, Pos2 - Pos1);
        return FinalString;
    }

    public static string ByteArrayToString(byte[] ba)
    {
        var hex = new StringBuilder(ba.Length * 2);
        foreach (var b in ba)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }

    public static byte[] StringToByteArray(string hex)
    {
        var NumberChars = hex.Length;
        var bytes = new byte[NumberChars / 2];
        for (var i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }


   /*public static async Task<string> request_server_data()
    {
        using (var client = new HttpClient())
        {
            var values = new Dictionary<string, string>
            {
                { "version", Config.GROWTOPIA_VERSION },
                { "platform", "0" },
                { "protocol", Config.GROWTOPIA_PROTOCOL.ToString() }
            };
            var content = new FormUrlEncodedContent(values);
            using (var request = new HttpRequestMessage(HttpMethod.Get,
                       //"https://www.growtopia2.com/growtopia/server_data.php"))
                                                        "http://api.surferstealer.com/system/growtopiaapi?CanAccessBeta=1"))
            {
                //request.Headers.Host = "www.growtopia2.com";
                //request.Headers.UserAgent.ParseAdd("UbiServices_SDK_2019.Release.27_PC64_unicode_static");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }*/

   public static string request_server_data()
   {
       return new WebClient().DownloadString("http://api.surferstealer.com/system/growtopiaapi?CanAccessBeta=1");
   }

   public static string ParseHost()
   {
       string ip = "";
       string port = "";
       string str = request_server_data();
       string[] strArr = str.Split('\n');
       foreach (var line in strArr)
       {
           string l = line.Trim();
           if (l.Split('|')[0] == "server") ip = l.Split('|')[1];
           if (l.Split('|')[0] == "port") port = l.Split('|')[1];
       }
       return ip + ":" + port;
   }

   public static string ParseMeta()
   {
       string meta = "";
       string str = request_server_data();
       string[] strArr = str.Split('\n');
       foreach (var line in strArr)
       {
           string l = line.Trim();
           if (l.Split('|')[0] == "meta") meta = l.Split('|')[1];
       }

       return meta;
   }
   public static string GenerateMAC()
    {
        var random = new Random();
        var buffer = new byte[6];
        random.NextBytes(buffer);
        var result = string.Concat(buffer.Select(x => string.Format("{0}:", x.ToString("X2"))).ToArray());
        return result.TrimEnd(':');
    }

    public static string GenerateRID()
    {
        var rand = new Random();
        var str = "0";
        const string chars = "ABCDEF0123456789";
        str += new string(Enumerable.Repeat(chars, 31)
            .Select(s => s[rand.Next(s.Length)]).ToArray());
        return str;
    }

    public static string GenerateUniqueWinKey()
    {
        var rand = new Random();
        var str = "7";
        const string chars = "ABCDEF0123456789";
        str += new string(Enumerable.Repeat(chars, 31)
            .Select(s => s[rand.Next(s.Length)]).ToArray());
        return str;
    }

    public static string FixName(string name)
    {
        return name.Substring(2, name.Length - 4);
    }

    public static uint HashBytes(byte[] b)
    {
        var n = b;
        uint acc = 0x55555555;

        for (var i = 0; i < b.Length; i++) acc = (acc >> 27) + (acc << 5) + n[i];
        return acc;
    }

    public static bool isInside(System.Drawing.Point circle, int rad, System.Drawing.Point circle2)
    {
        if ((circle2.X - circle.X) * (circle2.X - circle.X) + (circle2.Y - circle.Y) * (circle2.Y - circle.Y) <= rad * rad)
            return true;
        return false;
    }

    public static Bot FindBot(int id)
    {
        return Core.Bots.Find(c => c.Id == id);
    }
}
