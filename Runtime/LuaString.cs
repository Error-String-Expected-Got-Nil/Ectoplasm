using System.Text;

namespace Ectoplasm.Runtime;

/// <summary>
/// Value type representing a Lua string. Only a wrapper around an array of <see cref="byte"/>, marked as internal to
/// prevent mutation or access by external users. All interaction should occur through <see cref="LuaValue"/>.
/// </summary>
internal readonly struct LuaString : IEquatable<LuaString>
{
    private readonly byte[] _data;

    public ReadOnlySpan<byte> Data => _data;

    public string DataUtf16 => Encoding.UTF8.GetString(_data);

    public long Length => _data.Length;

    internal LuaString(byte[] data)
    {
        _data = data;
    }

    public LuaString(LuaString data)
    {
        _data = data._data;
    }

    public LuaString(ReadOnlySpan<byte> data)
    {
        _data = data.ToArray();
    }

    public LuaString(string data)
    {
        _data = Encoding.UTF8.GetBytes(data);
    }

    public bool Equals(LuaString other)
    {
        if (_data == other._data) return true;
        if (_data.Length != other._data.Length) return false;
        
        // Returns false if any indices in data arrays have non-matching values, true otherwise.
        return !_data.Where((value, index) => value != other._data[index]).Any();
    }

    // Basic djb2 hash, for lack of any better ideas.
    private const int HashSeed = 5381;
    public override int GetHashCode() 
        => _data.Aggregate(HashSeed, (hash, b) => (hash << 5) + hash + b);
}