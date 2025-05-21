namespace Ectoplasm.Runtime.Tables;

/// <summary>
/// Table implementation for tables containing only string keys.
/// </summary>
// ReSharper disable once InconsistentNaming
internal class TableImpl_Strings : TableImpl
{
    private Dictionary<LuaString, LuaValue> _values;
    
    /// <inheritdoc/>
    // A TableImpl_Strings will never have integer keys and therefore always has length 0.
    public override long Length => 0;
    
    /// <inheritdoc/>
    public override LuaValue Get(LuaValue index)
    {
        if (index.Kind != LuaValueKind.String) return default;
        _values.TryGetValue(index._string, out var value);
        return value;
    }

    /// <inheritdoc/>
    public override TableImpl Set(LuaValue index, LuaValue value)
    {
        if (index.Kind != LuaValueKind.String)
        {
            if (value.Kind == LuaValueKind.Nil) return this;
            
            // TODO: Upgrade implementation
            throw new NotImplementedException();
        }

        if (value.Kind == LuaValueKind.Nil)
        {
            _values.Remove(index._string);
            return this;
        }

        _values[index._string] = value;
        return this;
    }
}