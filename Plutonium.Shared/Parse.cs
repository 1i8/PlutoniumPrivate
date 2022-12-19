namespace Plutonium.Framework;

public class Parse
{
    private readonly Dictionary<string, object> _dictionary = new();

    public Parse(string str)
    {
        Deserialize(str);
    }

    public void Set(string key, object value)
    {
        _dictionary[key] = value;
    }

    public void Append(string key, object value)
    {
        _dictionary.Add(key, value);
    }

    public void Remove(string key)
    {
        _dictionary.Remove(key);
    }

    public T Get<T>(string key)
    {
        try
        {
            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(_dictionary[key].ToString()!.Trim(), typeof(T));
            return (T)Convert.ChangeType(_dictionary[key], typeof(T));
        }
        catch
        {
            return (T)Convert.ChangeType(0, typeof(T));
        }
    }

    public string Serialize()
    {
        var serialized = string.Empty;
        for (var i = 0; i < _dictionary.Count; i++)
        {
            if (string.IsNullOrEmpty(_dictionary.ElementAt(i).Key)) continue;
            serialized =
                $"{serialized}{_dictionary.ElementAt(i).Key + "|" + _dictionary.ElementAt(i).Value + Environment.NewLine}";
        }

        return serialized;
    }

    public void Deserialize(string str)
    {
        var lines = str.Split(Environment.NewLine);
        foreach (var line in lines)
        {
            if (line.Contains('|'))
                _dictionary.Add(line.Split('|')[0], line.Split('|')[1]);
        }
    }
}