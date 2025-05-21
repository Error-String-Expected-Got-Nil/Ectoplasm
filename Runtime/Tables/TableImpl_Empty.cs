namespace Ectoplasm.Runtime.Tables;

/// <summary>
/// An empty table, with no existing keys. Will upgrade to an actual implementation when set with a non-nil value.
/// </summary>
// ReSharper disable once InconsistentNaming
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
        
        // TODO: Upgrade implementation
        throw new NotImplementedException();
    }
}