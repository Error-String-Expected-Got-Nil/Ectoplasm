namespace Ectoplasm.Runtime.Tables;

/// <summary>
/// Implementation for a Lua table. May support only a subset of Lua values, in which case it can be upgraded,
/// causing it to transform into a new implementation which can support the given value.
/// </summary>
internal abstract class TableImpl
{
    /// <summary>
    /// The default length operator for this table implementation. See <see cref="LuaTable"/>
    /// <see cref="LuaTable.Length"/> for more details as to how this should be computed.
    /// </summary>
    public abstract long Length { get; }

    /// <summary>
    /// Get a value at a particular index.
    /// </summary>
    /// <param name="index">The index to check.</param>
    /// <returns>
    /// The value contained at the given index if it existed, or <see cref="LuaValueKind.Nil"/> if the index did not
    /// exist.
    /// </returns>
    public abstract Values.LuaValue Get(Values.LuaValue index);

    /// <summary>
    /// Attempt to set the value at a given index to a given value.
    /// </summary>
    /// <param name="index">Index to set the value at.</param>
    /// <param name="value">Value to assign to the given index.</param>
    /// <returns>
    /// If the set was successful, the same TableImpl. If not, the implementation will be upgraded, the new
    /// implementation will take the assignment, and the new implementation will be returned.
    /// </returns>
    public abstract TableImpl Set(Values.LuaValue index, Values.LuaValue value);
}