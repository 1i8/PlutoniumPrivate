namespace Plutonium.Agent.BotSystem.Entities;

public class GamePacket
{
    public int count1;
    public int count2;
    public int flags;
    public List<byte> flagsx = new();
    public float float1;
    public float float2;
    public int int_data;
    public int int_x;
    public int int_y;
    public int item;
    public int netid;
    public int objtype;
    public float pos_x;
    public float pos_y;
    public float pos2_x;
    public float pos2_y;
    public int type;
    public int data_size => flagsx.Count;

    public byte[] PackForSendingRaw()
    {
        var b = new byte[57 + data_size];
        Array.Copy(BitConverter.GetBytes(type), b, 4);
        Array.Copy(BitConverter.GetBytes(objtype), 0, b, 1, 4);
        Array.Copy(BitConverter.GetBytes(count1), 0, b, 2, 4);
        Array.Copy(BitConverter.GetBytes(count2), 0, b, 3, 4);
        Array.Copy(BitConverter.GetBytes(netid), 0, b, 4, 4);
        Array.Copy(BitConverter.GetBytes(item), 0, b, 8, 4);
        Array.Copy(BitConverter.GetBytes(flags), 0, b, 12, 4);
        Array.Copy(BitConverter.GetBytes(float1), 0, b, 16, 4);
        Array.Copy(BitConverter.GetBytes(int_data), 0, b, 20, 4);
        Array.Copy(BitConverter.GetBytes(pos_x), 0, b, 24, 4);
        Array.Copy(BitConverter.GetBytes(pos_y), 0, b, 28, 4);
        Array.Copy(BitConverter.GetBytes(pos2_x), 0, b, 32, 4);
        Array.Copy(BitConverter.GetBytes(pos2_y), 0, b, 36, 4);
        Array.Copy(BitConverter.GetBytes(float2), 0, b, 40, 4);
        Array.Copy(BitConverter.GetBytes(int_x), 0, b, 44, 4);
        Array.Copy(BitConverter.GetBytes(int_y), 0, b, 48, 4);
        Array.Copy(BitConverter.GetBytes(data_size), 0, b, 52, 4);
        var dat = flagsx.ToArray();
        var datLength = dat.Length;
        if (datLength > 0) Buffer.BlockCopy(dat, 0, b, 56, datLength);
        return b;
    }

    public static GamePacket UnpackFromPacket(byte[] p)
    {
        var packet = new GamePacket();
        if (p.Length >= 48)
        {
            var s = new byte[p.Length - 4];
            Array.Copy(p, 4, s, 0, s.Length);
            packet = Unpack(s);
        }

        return packet;
    }

    public static GamePacket Unpack(byte[] data)
    {
        var dataStruct = new GamePacket();
        dataStruct.type = BitConverter.ToInt32(data, 0);
        dataStruct.objtype = BitConverter.ToInt32(data, 1);
        dataStruct.count1 = BitConverter.ToInt32(data, 2);
        dataStruct.count2 = BitConverter.ToInt32(data, 3);
        dataStruct.netid = BitConverter.ToInt32(data, 4);
        dataStruct.item = BitConverter.ToInt32(data, 8);
        dataStruct.flags = BitConverter.ToInt32(data, 12);
        dataStruct.float1 = BitConverter.ToInt32(data, 16);
        dataStruct.int_data = BitConverter.ToInt32(data, 20);
        dataStruct.pos_x = BitConverter.ToSingle(data, 24);
        dataStruct.pos_y = BitConverter.ToSingle(data, 28);
        dataStruct.pos2_x = BitConverter.ToSingle(data, 32);
        dataStruct.pos2_y = BitConverter.ToSingle(data, 36);
        dataStruct.float2 = BitConverter.ToInt32(data, 40);
        dataStruct.int_x = BitConverter.ToInt32(data, 44);
        dataStruct.int_y = BitConverter.ToInt32(data, 48);
        return dataStruct;
    }
}