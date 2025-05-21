namespace Ectoplasm.Runtime;

public class LuaTable
{
    // TODO

    /// <summary>
    /// The default length operation for this table, indicated by a unary <c>#</c> operator in Lua.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For array-like tables, this is the number of elements in the table. "Array-like" in this case means having
    /// contiguous positive integer keys from 1 to some maximum value, all of which are
    /// non-<see cref="LuaValueKind.Nil"/>. This will be the case for tables constructed with unspecified keys, like:
    /// <c>{1, 2, 3, 4, 5}</c>.
    /// </para>
    /// <para>
    /// In any other case, however, this property (and thereby the length operator) should be considered unreliable.
    /// For tables which are exceptionally large, or which contain a very large gap of <see cref="LuaValueKind.Nil"/>
    /// values between two keys, a dramatically different number will be returned, and no guarantees can be made about
    /// it. For more details as to why this is, see the Lua reference manual (version 5.4) section 3.4.7 - The Length
    /// Operator, as it goes more in-depth as to the behavior of the length operator, and Ectoplasm obeys these
    /// constraints under normal circumstances.
    /// </para>
    /// <para>
    /// In Ectoplasm, the largest value that can be returned by this is the <see cref="int"/>
    /// <see cref="int.MaxValue"/>. This is because the contiguous integer key portion of tables is implemented as a
    /// <see cref="List{T}"/> of <see cref="LuaValue"/>s, and this property is retrieved by getting the
    /// <see cref="List{T}.Count"/> property of this list. If you somehow find yourself in a situation where you should
    /// be getting a return more than this value, you should reconsider if Lua via Ectoplasm is the correct choice of
    /// programming language for what you're trying to do.
    /// </para>
    /// </remarks>
    public long Length => throw new NotImplementedException();
    
    public LuaValue this[LuaValue index]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    } 
}