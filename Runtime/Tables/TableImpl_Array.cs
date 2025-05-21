namespace Ectoplasm.Runtime.Tables;

/// <summary>
/// Table implementation for tables that are simple arrays with integer keys greater than zero and less than
/// int.MaxValue, such that no more than half of the array is empty (has <see cref="LuaValueKind.Nil"/> value). This
/// is almost always the case for tables used like arrays in Lua.
/// </summary>
// ReSharper disable once InconsistentNaming
internal class TableImpl_Array(List<LuaValue> values, int nilCount) : TableImpl
{
    private int _nilCount = nilCount;
    
    /// <inheritdoc/>
    public override long Length => values.Count;
    
    /// <inheritdoc/>
    public override LuaValue Get(LuaValue index)
    {
        if (!index.TryCoerceInteger(out var coercedIndex)) return default;
        coercedIndex--;
        if (coercedIndex is < 0 or >= int.MaxValue || coercedIndex >= values.Count) return default;
        return values[(int)coercedIndex];
    }

    /// <inheritdoc/>
    public override TableImpl Set(LuaValue index, LuaValue value)
    {
        // Index isn't integer, implementation may need upgrade
        if (!index.TryCoerceInteger(out var coercedIndex))
        {
            // Nil assignment to key not contained in this table anyway; we don't have to do anything.
            if (value.Kind == LuaValueKind.Nil) return this;

            if (index.Kind == LuaValueKind.String)
                return new TableImpl_Multi(new Dictionary<LuaString, LuaValue> { { index._string, value } }, 
                    [], values, _nilCount);
            
            return TableImplUtil.UpgradeToCompleteImpl(index, value, list: values, nilCount: _nilCount);
        }

        // Decrement because Lua tables use 1-based indexing
        coercedIndex--;

        // Index isn't ever containable in this table, may need upgrade
        if (coercedIndex is < 0 or >= int.MaxValue)
        {
            if (value.Kind == LuaValueKind.Nil) return this;

            // coercedIndex is incremented to undo the earlier decrement
            return new TableImpl_Integers(new Dictionary<long, LuaValue> { { coercedIndex + 1, value } }, 
                values, _nilCount);
        }

        var intIndex = (int)coercedIndex;
        
        // Setting index may require growing list, may also result in upgrade
        if (intIndex >= values.Count)
        {
            if (value.Kind == LuaValueKind.Nil) return this;

            // Appending directly onto the end will never increase the nil ratio
            if (intIndex == values.Count)
            {
                values.Add(value);
                return this;
            }

            // Otherwise, we need to append a certain number of nils before the new value to make sure it goes to the
            // correct index. To avoid wasting too much space, we only do this if it won't make more than half of the
            // list nil.
            var extraNils = values.Count - intIndex;
            if (_nilCount + extraNils > (intIndex + 1) / 2)
                // Nil ratio will be over one half. Instead, we'll upgrade to TableImpl_Integers and add the new value
                // into its dictionary portion. Note that coercedIndex is incremented to undo the earlier decrement.
                return new TableImpl_Integers(new Dictionary<long, LuaValue> { { coercedIndex + 1, value } },
                    values, _nilCount);
            
            // Nil ratio won't be over half, we can append normally
            _nilCount += extraNils;
            values.EnsureCapacity(values.Count + extraNils + 1);
            for (var i = 0; i < extraNils; i++) values.Add(default);
            values.Add(value);
            return this;
        }
        
        // Failing any of that, the index is definitely inside the bounds of the current list.
        if (values[intIndex].Kind == LuaValueKind.Nil) _nilCount--;
        values[intIndex] = value;
        if (value.Kind != LuaValueKind.Nil) return this;
        
        // If we are setting the value to nil, we are possibly removing an element, and need to do some more checks.
        _nilCount++;
            
        // If we set the last entry in the list to nil, we should trim any excess nils from the list.
        if (intIndex == values.Count - 1) 
            TableImplUtil.TrimExcessNils(values, ref _nilCount);
            
        // Make sure we're not wasting more than half the list's space on nils, if it's big enough to matter.
        if (values.Count < TableImplUtil.MinCountForMemorySaving || _nilCount <= values.Count / 2) return this;

        var newImpl = new TableImpl_Integers([], values, _nilCount);
        newImpl.CleanListPortion();
        return newImpl;
    }
}