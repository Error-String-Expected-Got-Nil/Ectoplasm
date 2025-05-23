namespace Ectoplasm.Runtime.Tables;

/// <summary>
/// Table implementation for tables with only integer keys. Permits negative integer keys and 0, unlike TableImpl_Array.
/// Also, when the list part of the table grows large enough and is at least half empty, it will break the list apart
/// to reduce the wasted space. 
/// </summary>
internal class TableImpl_Integers(Dictionary<long, Values.LuaValue> dictPortion, List<Values.LuaValue> listPortion, int listNilCount)
    : TableImpl
{
    private int _listNilCount = listNilCount;

    /// <inheritdoc/>
    public override long Length => listPortion.Count;

    /// <inheritdoc/>
    public override Values.LuaValue Get(Values.LuaValue index)
    {
        if (!index.TryCoerceInteger(out var coercedIndex)) return default;
        
        coercedIndex--;
        if (coercedIndex is > 0 and < int.MaxValue && coercedIndex < listPortion.Count)
            // Index is within bounds of list portion, it will be there if the key exists
            return listPortion[(int)coercedIndex];

        // Otherwise, valid integer index, but can't be in the list portion, so if it exists it must be in the
        // dictionary portion. TryGetValue returns default if the key isn't found, which is what we want if it wasn't
        // found anyway (default == nil), so we can just return it immediately.
        dictPortion.TryGetValue(coercedIndex + 1, out var dictValue);
        return dictValue;
    }

    /// <inheritdoc/>
    public override TableImpl Set(Values.LuaValue index, Values.LuaValue value)
    {
        // Copied from TableImpl_Array.Set(), except wherever it would upgrade to this implementation, we instead insert
        // into the dictionary portion of this implementation.
        
        // Index isn't integer, implementation may need upgrade
        if (!index.TryCoerceInteger(out var coercedIndex))
        {
            // Nil assignment to key not contained in this table anyway; we don't have to do anything.
            if (value.Kind == LuaValueKind.Nil) return this;
            
            if (index.Kind == LuaValueKind.String)
                return new TableImpl_Multi(new Dictionary<LuaString, Values.LuaValue> { { (LuaString)index._ref, value } }, 
                    dictPortion, listPortion, _listNilCount);
            
            return TableImplUtil.UpgradeToCompleteImpl(index, value, intsDict: dictPortion, list: listPortion, 
                nilCount: _listNilCount);
        }

        // Decrement because Lua tables use 1-based indexing
        coercedIndex--;

        // Index isn't ever containable in list portion, add it to the dictionary portion instead
        if (coercedIndex is < 0 or >= int.MaxValue)
        {
            if (value.Kind == LuaValueKind.Nil)
            {
                dictPortion.Remove(coercedIndex + 1);
                return this;
            }

            dictPortion[coercedIndex + 1] = value;
        }

        var intIndex = (int)coercedIndex;
        
        // Setting index may require growing list
        if (intIndex >= listPortion.Count)
        {
            // Value is outside list and is nil, remove it from dictionary portion if and return
            if (value.Kind == LuaValueKind.Nil)
            {
                dictPortion.Remove(coercedIndex + 1);
                return this;
            }
            
            // Appending directly onto the end will never increase the nil ratio
            if (intIndex == listPortion.Count)
            {
                listPortion.Add(value);
                ConcatPortions();
                return this;
            }

            // Otherwise, we need to append a certain number of nils before the new value to make sure it goes to the
            // correct index. To avoid wasting too much space, we only do this if it won't make more than half of the
            // list nil.
            var extraNils = listPortion.Count - intIndex;
            if (_listNilCount + extraNils > (intIndex + 1) / 2)
            {
                // List portion would have nil ratio over half, add it to dictionary portion instead
                // Increment coercedIndex to undo earlier decrement
                dictPortion[coercedIndex + 1] = value;
                return this;
            }
            
            // Nil ratio won't be over half, we can append normally
            listPortion.EnsureCapacity(listPortion.Count + extraNils + 1);
            var initialDictIndex = listPortion.Count + 1;
            for (var i = 0; i < extraNils; i++)
            {
                // While adding filler values, we should move anything in the dictionary we can to the list
                dictPortion.TryGetValue(initialDictIndex + i, out var foundValue);
                if (foundValue.Kind == LuaValueKind.Nil) _listNilCount++;
                else dictPortion.Remove(initialDictIndex + i);
                listPortion.Add(foundValue);
            }
            listPortion.Add(value);

            ConcatPortions();
            
            return this;
        }
        
        // Failing any of that, the index is definitely inside the bounds of the current list.
        if (listPortion[intIndex].Kind == LuaValueKind.Nil) _listNilCount--;
        listPortion[intIndex] = value;
        if (value.Kind != LuaValueKind.Nil) return this;
        
        // If we are setting the value to nil, we are possibly removing an element, and need to do some more checks.
        _listNilCount++;
            
        // If we set the last entry in the list to nil, we should trim any excess nils from the list.
        if (intIndex == listPortion.Count - 1) 
            TableImplUtil.TrimExcessNils(listPortion, ref _listNilCount);
            
        // Make sure we're not wasting more than half the list's space on nils, if it's big enough to matter.
        if (listPortion.Count >= TableImplUtil.MinCountForMemorySaving && _listNilCount > listPortion.Count / 2) 
            CleanListPortion();
        
        return this;
    }

    // Starting from the end of the list portion, moving non-nil values to the dictionary portion and replacing them
    // with nils, until we reach an index such that removing it and all greater indices would make the nil ratio less
    // than half, at which point we trim excess nils.
    internal void CleanListPortion()
    {
        var cleanIndex = listPortion.Count - 1;
        while (cleanIndex >= 0)
        {
            if (listPortion[cleanIndex].Kind != LuaValueKind.Nil)
            {
                dictPortion[cleanIndex + 1] = listPortion[cleanIndex];
                listPortion[cleanIndex] = default;
                _listNilCount++;
            }

            // cleanIndex = list size after trimming that index and above
            // count of nils after trimming = nil count - (listPortion.Count - cleanIndex)
            // nil ratio not above half when: nil count - listPortion.Count + cleanIndex <= cleanIndex / 2
            if (_listNilCount - listPortion.Count + cleanIndex <= cleanIndex / 2) break;

            cleanIndex--;
        }
        
        TableImplUtil.TrimExcessNils(listPortion, ref _listNilCount);
    }

    // Checks if there's a sequence of indices immediately after the end of the list portion in the dictionary portion,
    // and moves them to the list portion if there are.
    private void ConcatPortions()
    {
        var checkIndex = (long)listPortion.Count + 1;
        
        // checkIndex - 1 < int.MaxValue
        while (checkIndex <= int.MaxValue)
        {
            dictPortion.TryGetValue(checkIndex, out var value);
            // We stop if we hit a nil since that ends the contiguous sequence
            if (value.Kind == LuaValueKind.Nil) break;
            
            // Otherwise, move the value from the dictionary portion to the list portion
            dictPortion.Remove(checkIndex);
            listPortion.Add(value); // checkIndex will always be such that it is the next new index in the list portion

            checkIndex++;
        }
    }
}