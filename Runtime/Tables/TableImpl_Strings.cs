using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Runtime.Tables;

/// <summary>
/// Table implementation for tables containing only string keys.
/// </summary>
internal class TableImpl_Strings(Dictionary<LuaString, LuaValue> values) : TableImpl
{
    /// <inheritdoc/>
    // A TableImpl_Strings will never have integer keys and therefore always has length 0.
    public override long Length => 0;
    
    /// <inheritdoc/>
    public override LuaValue Get(LuaValue index)
    {
        if (index.Kind != LuaValueKind.String) return default;
        values.TryGetValue((LuaString)index._ref, out var value);
        return value;
    }

    /// <inheritdoc/>
    public override TableImpl Set(LuaValue index, LuaValue value)
    {
        if (index.Kind != LuaValueKind.String)
        {
            if (value.Kind == LuaValueKind.Nil) return this;

            if (index.TryCoerceInteger(out var coercedInteger))
                return coercedInteger switch
                {
                    1 => new TableImpl_Array([value], 0),
                    2 => new TableImpl_Array([default, value], 1),
                    _ => new TableImpl_Integers(new Dictionary<long, LuaValue> { { coercedInteger, value } },
                        [], 0)
                };

            return TableImplUtil.UpgradeToCompleteImpl(index, value, values);
        }

        if (value.Kind == LuaValueKind.Nil)
        {
            values.Remove((LuaString)index._ref);
            return this;
        }

        values[(LuaString)index._ref] = value;
        return this;
    }
}