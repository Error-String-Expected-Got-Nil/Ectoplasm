using Ectoplasm.Runtime.Tables;

namespace Ectoplasm.Runtime;

/// <summary>
/// <para>
/// Type representing a Lua table. Can be indexed using any <see cref="LuaValue"/> other than
/// <see cref="LuaValueKind.Nil"/> or <see cref="double.NaN"/>, returning any <see cref="LuaValue"/>. A return of
/// <see cref="LuaValueKind.Nil"/> on an index get means that index has no value; furthermore, setting an index to
/// <see cref="LuaValueKind.Nil"/> means removing that index.
/// </para>
/// <para>
/// Creating a table from a collection of only values, without keys, will automatically assign the elements sequential
/// integer keys starting with 1, effectively a 1-base indexed array.
/// </para>
/// <para>
/// When indexing, <see cref="LuaValueKind.Float"/> values which can be coerced to <see cref="LuaValueKind.Integer"/>
/// values without any loss will index the same values as the equivalent integer. Ex.: <c>table[1] == table[1.0]</c>.
/// </para>
/// </summary>
public class LuaTable
{
    private TableImpl _implementation = new TableImpl_Empty();

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
    
    /// <summary>
    /// Creates a new, empty Lua table.
    /// </summary>
    public LuaTable() { }

    /// <summary>
    /// Creates a new Lua table with sequential integer key values starting at 1. 
    /// </summary>
    /// <param name="values">Enumeration of values to create the table out of.</param>
    /// <remarks>
    /// Ensure the number of items in the enumeration is less than <see cref="int"/> <see cref="int.MaxValue"/>.
    /// </remarks>
    public LuaTable(params IEnumerable<LuaValue> values)
    {
        var list = values.ToList();
        _implementation = new TableImpl_Array(list, list.Count(val => val.Kind == LuaValueKind.Nil));
    }
    
    // TODO: Constructor from IEnumerable<KeyValuePair<LuaValue, LuaValue>> amd from LuaState stack
    
    public LuaValue this[LuaValue index]
    {
        get => _implementation.Get(index);
        set
        {
            if (index.Kind == LuaValueKind.Nil)
                throw new LuaRuntimeException("Table index is nil");
            _implementation = _implementation.Set(index, value);
        }
    } 
}