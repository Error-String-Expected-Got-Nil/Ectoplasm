using System.Text;
using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Runtime;

/// <summary>
/// Value type representing a Lua string. Only a wrapper around an array of <see cref="byte"/>, marked as internal to
/// prevent mutation or access by external users. All interaction should occur through <see cref="Values.LuaValue"/>.
/// </summary>
internal readonly struct LuaString(byte[] data) : IEquatable<LuaString>
{
    private readonly byte[] _data = data;

    public ReadOnlySpan<byte> Data => _data;

    public string DataUtf16 => Encoding.UTF8.GetString(_data);

    public string DataUtf16Safe
    {
        get
        {
            try
            {
                return Encoding.UTF8.GetString(_data);
            }
            catch (Exception)
            {
                return "<Unicode invalid string>";
            }
        }
    }

    public long Length => _data.Length;

    public LuaString(LuaString data) : this(data._data) { }

    public LuaString(ReadOnlySpan<byte> data) : this(data.ToArray()) { }

    public LuaString(string data) : this(Encoding.UTF8.GetBytes(data)) { }

    public static LuaString Concat(LuaString a, LuaString b)
    {
        var buffer = new byte[a._data.Length + b._data.Length];
        Array.Copy(a._data, 0, buffer, 0, a._data.Length);
        Array.Copy(b._data, 0, buffer, a._data.Length, b._data.Length);
        return new LuaString(buffer);
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

    public override bool Equals(object? obj) => obj is LuaString luaString && Equals(luaString);
}