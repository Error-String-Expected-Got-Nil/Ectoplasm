namespace Ectoplasm.Runtime.Tables;

/// <summary>
/// Table implementation able to handle any valid Lua value.
/// </summary>
internal class TableImpl_Complete(LuaValue.LuaValue trueValue, LuaValue.LuaValue falseValue, 
    Dictionary<double, LuaValue.LuaValue> floatsDictPortion, Dictionary<object, LuaValue.LuaValue> refsDictPortion, 
    Dictionary<LuaString, LuaValue.LuaValue> stringsDictPortion, Dictionary<long, LuaValue.LuaValue> dictPortion,
    List<LuaValue.LuaValue> listPortion, int listNilCount) : TableImpl
{
    private LuaValue.LuaValue _trueValue = trueValue;
    private LuaValue.LuaValue _falseValue = falseValue;
    private int _listNilCount = listNilCount;

    /// <inheritdoc/>
    public override long Length => listPortion.Count;
    
    // Large portions copied from TableImpl_Multi
    
    /// <inheritdoc/>
    public override LuaValue.LuaValue Get(LuaValue.LuaValue index)
    {
        if (!index.TryCoerceInteger(out var coercedIndex))
            return index.Kind switch
            {
                LuaValueKind.Boolean => index._boolean ? _trueValue : _falseValue,
                LuaValueKind.Float => floatsDictPortion.GetValueOrDefault(index._float),
                LuaValueKind.String => stringsDictPortion.GetValueOrDefault((LuaString)index._ref),
                LuaValueKind.Function or LuaValueKind.Userdata or LuaValueKind.Thread or LuaValueKind.Table 
                    => refsDictPortion.GetValueOrDefault(index._ref),
                _ => default
            };
        
        coercedIndex--;
        if (coercedIndex is > 0 and < int.MaxValue && coercedIndex < listPortion.Count)
            return listPortion[(int)coercedIndex];
        
        dictPortion.TryGetValue(coercedIndex + 1, out var dictValue);
        return dictValue;
    }

    /// <inheritdoc/>
    public override TableImpl Set(LuaValue.LuaValue index, LuaValue.LuaValue value)
    {
        if (!index.TryCoerceInteger(out var coercedIndex))
            return index.Kind switch
            {
                LuaValueKind.Boolean => SetBoolean(index._boolean, value),
                LuaValueKind.Float => SetFloat(index._float, value),
                LuaValueKind.String => SetString((LuaString)index._ref, value),
                LuaValueKind.Function or LuaValueKind.Userdata or LuaValueKind.Thread or LuaValueKind.Table 
                    => SetRef(index._ref, value),
                _ => this
            };
        
        return SetInteger(coercedIndex, value);
    }

    private TableImpl_Complete SetBoolean(bool index, LuaValue.LuaValue value)
    {
        if (index) _trueValue = value;
        else _falseValue = value;

        return this;
    }

    private TableImpl_Complete SetFloat(double index, LuaValue.LuaValue value)
    {
        if (double.IsNaN(index)) throw new LuaRuntimeException("Table index is NaN");
        
        if (value.Kind == LuaValueKind.Nil)
        {
            floatsDictPortion.Remove(index);
            return this;
        }

        floatsDictPortion[index] = value;
        return this;
    }

    private TableImpl_Complete SetRef(object index, LuaValue.LuaValue value)
    {
        if (value.Kind == LuaValueKind.Nil)
        {
            refsDictPortion.Remove(index);
            return this;
        }

        refsDictPortion[index] = value;
        return this;
    }
    
    private TableImpl_Complete SetString(LuaString index, LuaValue.LuaValue value)
    {
        if (value.Kind == LuaValueKind.Nil)
        {
            stringsDictPortion.Remove(index);
            return this;
        }

        stringsDictPortion[index] = value;
        return this;
    }

    private TableImpl_Complete SetInteger(long index, LuaValue.LuaValue value)
    {
        // Decrement because Lua tables use 1-based indexing
        index--;

        // Index isn't ever containable in list portion, add it to the dictionary portion instead
        if (index is < 0 or >= int.MaxValue)
        {
            if (value.Kind == LuaValueKind.Nil)
            {
                dictPortion.Remove(index + 1);
                return this;
            }

            dictPortion[index + 1] = value;
        }

        var intIndex = (int)index;
        
        // Setting index may require growing list
        if (intIndex >= listPortion.Count)
        {
            // Value is outside list and is nil, remove it from dictionary portion if and return
            if (value.Kind == LuaValueKind.Nil)
            {
                dictPortion.Remove(index + 1);
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
                dictPortion[index + 1] = value;
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
    private void CleanListPortion()
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