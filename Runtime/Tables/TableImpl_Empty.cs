using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Runtime.Tables;

/// <summary>
/// An empty table, with no existing keys. Will upgrade to an actual implementation when set with a non-nil value.
/// </summary>
internal class TableImpl_Empty : TableImpl
{
    /// <inheritdoc/>
    // An empty table of course always has length 0.
    public override long Length => 0;
    
    /// <inheritdoc/>
    // And contains only nil values.
    public override LuaValue Get(LuaValue index) => default;

    /// <inheritdoc/>
    // And always needs to be upgraded when an index is assigned a non-nil value.
    public override TableImpl Set(LuaValue index, LuaValue value)
    {
        if (value.Kind == LuaValueKind.Nil) return this;

        if (index.Kind == LuaValueKind.String)
            return new TableImpl_Strings(new Dictionary<LuaString, LuaValue> { { (LuaString)index._ref, value } });
        
        if (index.TryCoerceInteger(out var coercedInteger))
            return coercedInteger switch
            {
                1 => new TableImpl_Array([value], 0),
                2 => new TableImpl_Array([default, value], 1),
                _ => new TableImpl_Integers(new Dictionary<long, LuaValue> { { coercedInteger, value } },
                    [], 0)
            };

        return TableImplUtil.UpgradeToCompleteImpl(index, value);
    }
}