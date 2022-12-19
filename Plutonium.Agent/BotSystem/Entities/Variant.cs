using System.Text;

namespace Plutonium.Agent.BotSystem.Entities;

public class Variant
{
    public static byte[] extended_data(byte[] data)
    {
        return data.Skip(56).ToArray();
    }

    public static byte[]? get_struct_data(byte[] data)
    {
        var dataLength = data.Length;

        if (dataLength < 0x3c) return null;

        var structData = new byte[dataLength - 4];
        Array.Copy(data, 4, structData, 0, dataLength - 4);
        var p2Len = BitConverter.ToInt32(data, 56);

        if ((data[16] & 8) == 0)
            Array.Copy(BitConverter.GetBytes(0), 0, data, 56, 4);

        return structData;
    }

    public static VarFunc GetCall(byte[] buffer)
    {
        var varFunc = new VarFunc();
        var pos = 0;
        int argsTotal = buffer[pos++];

        if (argsTotal > 7) return varFunc;

        varFunc.Args = new object[argsTotal];

        for (var i = 0; i < argsTotal; i++)
        {
            varFunc.Args[i] = 0;
            int index = buffer[pos++];
            int type = buffer[pos++];

            switch (type)
            {
                case 1:
                {
                    var vFloat = BitConverter.ToUInt32(buffer, pos);
                    pos += 4;
                    varFunc.Args[index] = vFloat;
                    break;
                }
                case 2: //string
                {
                    var strLen = BitConverter.ToInt32(buffer, pos);
                    pos += 4;
                    var v = string.Empty;
                    v = Encoding.ASCII.GetString(buffer, pos, strLen);
                    pos += strLen;
                    if (index == 0)
                        varFunc.Name = v;
                    if (index > 0) varFunc.Args[i] = v;
                    break;
                }
                case 5: // uint
                    var vUInt = BitConverter.ToUInt32(buffer, pos);
                    pos += 4;
                    varFunc.Args[index] = vUInt;
                    break;
                case 9
                    :
                    var vInt = BitConverter.ToInt32(buffer, pos);
                    pos += 4;
                    varFunc.Args[index] = vInt;
                    break;
            }
        }

        return varFunc;
    }

    public struct VarFunc
    {
        public string Name;
        public int NetId;
        public uint Delay;
        public object[] Args;
    }
}