namespace Ectoplasm.Runtime.Tables;

/// <summary>
/// Implementation for a Lua table. May support only a subset of Lua values, in which case it can be upgraded,
/// causing it to transform into a new implementation which can support the given value.
/// </summary>
public abstract class TableImpl
{
    /// <summary>
    /// The default length operator for this table implementation. See <see cref="LuaTable.Length"/> for more details
    /// as to how this should be computed.
    /// </summary>
    public abstract long Length { get; }

    /// <summary>
    /// Get a value at a particular index.
    /// </summary>
    /// <param name="index">The index to check.</param>
    /// <returns>
    /// The value contained at the given index if it existed, or <see cref="LuaValueKind.Nil"/> if the index did not
    /// exist, or was not supported by this implementation.
    /// </returns>
    public abstract LuaValue Get(LuaValue index);

    /// <summary>
    /// Attempt to set the value at a given index to a given value.
    /// </summary>
    /// <param name="index">Index to set the value at.</param>
    /// <param name="value">Value to assign to the given index.</param>
    /// <returns>
    /// True if assignment was successful, false if not. Upgrading the implementation should allow assignment, if false
    /// was returned.
    /// </returns>
    public abstract bool Set(LuaValue index, LuaValue value);

    /// <summary>
    /// Upgrade this table implementation so that it is able to assign the given value to the given index.
    /// </summary>
    /// <param name="index">Index to set the value at.</param>
    /// <param name="value">Value to assign to the given index.</param>
    /// <returns>The new implementation, which can handle the assignment, with the assignment performed.</returns>
    public abstract TableImpl Upgrade(LuaValue index, LuaValue value);
}