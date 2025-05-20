namespace Ectoplasm.Runtime;

public class LuaTable
{
    // TODO

    /// <summary>
    /// <para>
    /// The default length operation for this table, indicated by a unary <c>#</c> operator in Lua.
    /// </para>
    /// <para>
    /// For a table not containing positive integer keys, this will always return 0. Otherwise, it is any positive
    /// integer key in the table which is a "border", meaning it indexes a non-<see cref="LuaValueKind.Nil"/> value and
    /// the next positive integer key after it is either not within the table, or is <see cref="LuaValueKind.Nil"/>. In
    /// particular, this means for a table containing only positive integer keys in a contiguous sequence from 1 to the
    /// maximum key (as you would get for a table constructed with default indices), this property returns the number of
    /// elements in the table.
    /// </para>
    /// </summary>
    /// <remarks>
    /// In Ectoplasm, the highest border is always returned. It is nonetheless not recommended to rely on this fact, as
    /// it is an implementation detail. Other runtimes may not behave this way, and it may not always be true for
    /// Ectoplasm.
    /// </remarks>
    public long Length => throw new NotImplementedException();
    
    public LuaValue this[LuaValue index]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    } 
}