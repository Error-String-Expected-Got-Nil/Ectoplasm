namespace Ectoplasm.Runtime.Tables;

/// <summary>
/// Table implementation for tables that are simple arrays with integer keys greater than zero and less than
/// int.MaxValue, such that no more than half of the array is empty (has <see cref="LuaValueKind.Nil"/> value). This
/// is almost always the case for tables used like arrays in Lua.
/// </summary>
// ReSharper disable once InconsistentNaming
public class TableImpl_Array : TableImpl
{
    private readonly List<LuaValue> _values = [];
    private int _nilCount;
    
    /// <inheritdoc/>
    public override long Length => _values.Count;
    
    /// <inheritdoc/>
    public override LuaValue Get(LuaValue index)
    {
        if (!index.TryCoerceInteger(out var coercedIndex)) return default;
        coercedIndex--;
        if (coercedIndex is < 0 or > int.MaxValue || coercedIndex >= _values.Count) return default;
        return _values[(int)coercedIndex];
    }

    /// <inheritdoc/>
    public override TableImpl Set(LuaValue index, LuaValue value)
    {
        // Index isn't integer, implementation may need upgrade
        if (!index.TryCoerceInteger(out var coercedIndex))
        {
            // Nil assignment to key not contained in this table anyway; we don't have to do anything.
            if (value.Kind == LuaValueKind.Nil) return this;
            
            // TODO: Upgrade to implementation able to handle it
            throw new NotImplementedException();
        }

        coercedIndex--;

        // Index isn't ever containable in this table, may need upgrade
        if (coercedIndex is < 0 or > int.MaxValue)
        {
            if (value.Kind == LuaValueKind.Nil) return this;
            
            // TODO: Upgrade to implementation with Dictionary<long, LuaValue>
            throw new NotImplementedException();
        }

        var intIndex = (int)coercedIndex;
        
        // Setting index may require growing list, may also result in upgrade
        if (intIndex > _values.Count)
        {
            if (value.Kind == LuaValueKind.Nil) return this;
            
            // TODO: Possibly expand list, possibly upgrade to implementation with Dictionary<long, LuaValue>
            throw new NotImplementedException();
        }
        
        // Failing any of that, the index is definitely inside the bounds of the current list.
        if (_values[intIndex].Kind == LuaValueKind.Nil) _nilCount--;
        _values[intIndex] = value;
        if (value.Kind != LuaValueKind.Nil) return this;
        
        // If we are setting the value to nil, we are possibly removing an element, and need to do some more checks.
        _nilCount++;
            
        // If we set the last entry in the list to nil, we should trim any excess nils from the list.
        if (intIndex == _values.Count - 1) 
            TableImplUtil.TrimExcessNils(_values, ref _nilCount);
            
        // Make sure we're not wasting more than half the list's space on nils, if it's big enough to matter.
        if (_values.Count < TableImplUtil.MinCountForMemorySaving || _nilCount <= _values.Count / 2) return this;
            
        // TODO: Upgrade to implementation with Dictionary<long, LuaValue>, tell it to clean up its list
        throw new NotImplementedException();

    }
}