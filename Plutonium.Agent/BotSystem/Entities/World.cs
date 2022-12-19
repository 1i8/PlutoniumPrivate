namespace Plutonium.Agent.BotSystem.Entities;

public class World
{
    private int _readPos;
    private int _tileCount;
    public List<DroppedObject> DroppedObjects = new();
    public int LastDroppedUid;

    public string Name = "";
    public List<NetAvatar> Players = new();
    public Tile[] Tiles = Array.Empty<Tile>();
    public int Width, Height;

    private void Reset()
    {
        DroppedObjects.Clear();
        Players.Clear();
        Tiles = Array.Empty<Tile>();
        Name = "";
        _readPos = 0;
        _tileCount = 0;
        Width = 0;
        Height = 0;
    }

    public World Load(byte[] buffer)
    {
        try
        {
            var extended = Variant.extended_data(buffer);
            Reset();
            short len = 0;
            if (extended.Length < 8192) return this;
            if (extended.Length > 200000) return this;

            _readPos += 6;
            len = BitConverter.ToInt16(extended, _readPos);
            _readPos += 2; //short

            for (var i = 0; i < len; i++)
                Name += (char)extended[_readPos++];

            if (Name != string.Empty)
            {
                Width = BitConverter.ToInt32(extended, _readPos);
                _readPos += 4; //int
                Height = BitConverter.ToInt32(extended, _readPos);
                _readPos += 4; //int
                _tileCount = BitConverter.ToInt32(extended, _readPos);
                _readPos += 4; //int
                Tiles = new Tile[_tileCount]; //Tile count would be always 6000 Because of World size (100,60)
                var ok = 0;
                var ok2 = 0;
                for (var i = 0; i < _tileCount; i++)
                {
                    var header = new Tile();
                    if (ok == 100)
                    {
                        ok = 0;
                        ok2++;
                    }

                    header.X = ok++;
                    header.Y = ok2;

                    header.Foreground = BitConverter.ToUInt16(extended, _readPos);
                    _readPos += 2;
                    header.Background = BitConverter.ToUInt16(extended, _readPos);
                    _readPos += 2;
                    header.TileState = BitConverter.ToInt32(extended, _readPos);
                    _readPos += 4;
                    if ((short)header.TileState > 0) _readPos += 2;

                    if (header.Foreground == 3760)
                    {
                        _readPos += 22;
                        Tiles[i] = header;
                        continue;
                    }

                    if (ItemHandler.RequiresTileExtra(header.Foreground))
                    {
                        ushort len2 = 0;
                        var itemType = extended[_readPos];
                        var itemName = ItemHandler.itemDefs.Find(c => c.id == header.Foreground);
                        _readPos++;
                        header.Type = itemType;
                        switch (itemType)
                        {
                            case 0:
                                break;
                            case 1:
                                // this is door
                                len = BitConverter.ToInt16(extended, _readPos);
                                _readPos += 2;
                                _readPos += len + 1;
                                break;
                            case 2:
                                len = BitConverter.ToInt16(extended, _readPos);
                                _readPos += 2;
                                _readPos += len + 4;
                                break;
                            case 3:
                                _readPos++;
                                var adminCount = extended[_readPos + 4];
                                _readPos += 16 + adminCount * 4;
                                break;
                            case 4: //Trees
                            {
                                var seedTime = BitConverter.ToInt32(extended, _readPos);
                                header.SeedData = new SeedData(itemName.growTime);
                                header.SeedData.Time = seedTime;
                                new Thread(header.SeedData.TimeThread).Start();
                                _readPos += 5;
                            }
                                break;
                            case 0x8:
                                _readPos++;
                                break;
                            case 0x9:
                                _readPos += 4;
                                break;
                            case 0xb:
                                _readPos += 4;
                                len = BitConverter.ToInt16(extended, _readPos);
                                _readPos += 2;
                                _readPos += len;
                                break;
                            case 0xe:
                                len = BitConverter.ToInt16(extended, _readPos);
                                _readPos += 2;
                                _readPos += len;
                                _readPos += 23;
                                break;
                            case 0x0f:
                                _readPos++;
                                break;
                            case 0x10:
                                _readPos++;
                                break;
                            case 0x12:
                                _readPos += 5;
                                break;
                            case 0x13:
                                _readPos += 18;
                                break;
                            case 0x14:
                                len = BitConverter.ToInt16(extended, _readPos);
                                _readPos += 2;
                                _readPos += len;
                                break;
                            case 0x15:
                                len = BitConverter.ToInt16(extended, _readPos);
                                _readPos += 2;
                                _readPos += len;
                                _readPos += 5;
                                break;
                            case 0x17:
                                _readPos += 4;
                                break;
                            case 0x18:
                                _readPos += 8;
                                break;
                            case 0x19:
                                _readPos++;
                                var c = BitConverter.ToInt32(extended, _readPos);
                                _readPos += 4;
                                _readPos += 4 * c;
                                break;
                            case 0x1B:
                                _readPos += 4;
                                break;
                            case 0x1C:
                                _readPos += 6;
                                break;
                            case 0x20:
                                _readPos += 4;
                                break;
                            case 0x21:
                                if (header.Foreground == 3394)
                                {
                                    len = BitConverter.ToInt16(extended, _readPos);
                                    _readPos += 2;
                                    _readPos += len;
                                }

                                break;
                            case 0x23:
                                _readPos += 4;
                                len = BitConverter.ToInt16(extended, _readPos);
                                _readPos += 2;
                                _readPos += len;
                                break;
                            case 0x25:
                                len = BitConverter.ToInt16(extended, _readPos);
                                _readPos += 2;
                                _readPos += 32 + len;
                                break;
                            case 0x27:
                                // lock-bot
                                _readPos += 4;
                                break;
                            case 0x28:
                                // bg weather
                                _readPos += 4;
                                break;
                            case 0x2a:
                                //extended++;
                                break;
                            case 0x2b:
                                _readPos += 16;
                                break;
                            case 0x2c:
                                _readPos++; // skipping owner uid
                                _readPos += 4;
                                var adminCount2 = extended[_readPos];
                                _readPos += 4; // guild shit
                                _readPos += adminCount2 * 4;

                                break;
                            case 0x2f:
                                len = BitConverter.ToInt16(extended, _readPos);
                                _readPos += 2;
                                _readPos += 5 + len;
                                break;
                            case 0x30:
                                len = BitConverter.ToInt16(extended, _readPos);
                                _readPos += 2;
                                _readPos += 26 + len;
                                break;
                            case 0x31:
                                // stuff weather
                                _readPos += 9;
                                break;
                            case 0x32:

                                // activity indicator in there, keep skipping as usual...
                                _readPos += 4;
                                break;
                            case 0x34:
                                // Howler => do not serialize or increase bytes read?
                                break;
                            case 0x36:
                                // storage box xtreme
                                var itemsSize = BitConverter.ToInt16(extended, _readPos);
                                _readPos += 2;
                                _readPos += itemsSize;
                                break;
                            case 0x38:
                                // lucky token
                                len = BitConverter.ToInt16(extended, _readPos);
                                _readPos += 2;
                                _readPos += 4 + len;
                                break;
                            case 0x39:
                                // geiger charger
                                _readPos += 4;
                                // yeah also wondering why 1 byte wasnt enough to determine the existence of a geiger iprogram :)
                                break;
                            case 0x3a:
                                // adventure begins = nothing lol:)
                                break;
                            case 0x3e:
                                _readPos += 14;
                                break;
                            case 0x3f:
                                // cybots
                                var r = BitConverter.ToInt32(extended, _readPos);
                                _readPos += 4;
                                _readPos += r * 15;
                                _readPos += 8;
                                break;
                            case 0x41:
                                // guild item
                                _readPos += 17;
                                break;
                            case 0x42:
                                // growscan 9000
                                _readPos++;
                                break;
                            case 0x49:
                                // temporary platforms
                                _readPos += 4;
                                break;
                            case 0x4a:
                                // safe vault, nothing inside.
                                break;
                            default:
                                len = 0;
                                break; // unknown tile visual type...
                        }
                    }

                    Tiles[i] = header;
                }

                LastDroppedUid = 0;
                int droppedCount;
                droppedCount = BitConverter.ToInt32(extended, _readPos);
                _readPos += 4;
                LastDroppedUid = BitConverter.ToInt32(extended, _readPos);
                _readPos += 4;
                for (var i = 0; i < droppedCount; i++)
                {
                    var item = new DroppedObject();
                    item.Id = BitConverter.ToInt16(extended, _readPos);
                    _readPos += 2;
                    item.X = (int)BitConverter.ToSingle(extended, _readPos);
                    _readPos += 4;
                    item.Y = (int)BitConverter.ToSingle(extended, _readPos);
                    _readPos += 4;
                    item.Amount = extended[_readPos];
                    _readPos += 2;
                    item.Uid = BitConverter.ToInt32(extended, _readPos);
                    _readPos += 4;
                    DroppedObjects.Add(item);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("World Serilization FAILED\n" + e.Message);
            return this;
        }

        return this;
    }
}