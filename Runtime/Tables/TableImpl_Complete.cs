namespace Ectoplasm.Runtime.Tables;

/// <summary>
/// Table implementation able to handle any valid Lua value.
/// </summary>
// ReSharper disable once InconsistentNaming
internal class TableImpl_Complete : TableImpl
{
    private LuaValue _trueValue;
    private LuaValue _falseValue;
    private readonly Dictionary<double, LuaValue> _floatsDictPortion;
    private readonly Dictionary<object, LuaValue> _refsDictPortion;
    private readonly Dictionary<LuaString, LuaValue> _stringsDictPortion;
    private readonly Dictionary<long, LuaValue> _dictPortion;
    private readonly List<LuaValue> _listPortion;
    private int _listNilCount;

    /// <inheritdoc/>
    public override long Length => _listPortion.Count;
    
    // Large portions copied from TableImpl_Multi
    
    /// <inheritdoc/>
    public override LuaValue Get(LuaValue index)
    {
        if (!index.TryCoerceInteger(out var coercedIndex))
            return index.Kind switch
            {
                LuaValueKind.Boolean => index._boolean ? _trueValue : _falseValue,
                LuaValueKind.Float => _floatsDictPortion.GetValueOrDefault(index._float),
                LuaValueKind.String => _stringsDictPortion.GetValueOrDefault(index._string),
                LuaValueKind.Function => _refsDictPortion.GetValueOrDefault(index._function),
                LuaValueKind.Userdata => _refsDictPortion.GetValueOrDefault(index._userdata),
                LuaValueKind.Thread => _refsDictPortion.GetValueOrDefault(index._thread),
                LuaValueKind.Table => _refsDictPortion.GetValueOrDefault(index._table),
                _ => default
            };
        
        coercedIndex--;
        if (coercedIndex is > 0 and < int.MaxValue && coercedIndex < _listPortion.Count)
            return _listPortion[(int)coercedIndex];
        
        _dictPortion.TryGetValue(coercedIndex + 1, out var dictValue);
        return dictValue;
    }

    /// <inheritdoc/>
    public override TableImpl Set(LuaValue index, LuaValue value)
    {
        if (!index.TryCoerceInteger(out var coercedIndex))
            return index.Kind switch
            {
                LuaValueKind.Boolean => SetBoolean(index._boolean, value),
                LuaValueKind.Float => SetFloat(index._float, value),
                LuaValueKind.String => SetString(index._string, value),
                LuaValueKind.Function => SetRef(index._function, value),
                LuaValueKind.Userdata => SetRef(index._userdata, value),
                LuaValueKind.Thread => SetRef(index._thread, value),
                LuaValueKind.Table => SetRef(index._table, value),
                _ => this
            };
        
        return SetInteger(coercedIndex, value);
    }

    private TableImpl_Complete SetBoolean(bool index, LuaValue value)
    {
        if (index) _trueValue = value;
        else _falseValue = value;

        return this;
    }

    private TableImpl_Complete SetFloat(double index, LuaValue value)
    {
        if (double.IsNaN(index)) throw new LuaRuntimeException("Table index is NaN");
        
        if (value.Kind == LuaValueKind.Nil)
        {
            _floatsDictPortion.Remove(index);
            return this;
        }

        _floatsDictPortion[index] = value;
        return this;
    }

    private TableImpl_Complete SetRef(object index, LuaValue value)
    {
        if (value.Kind == LuaValueKind.Nil)
        {
            _refsDictPortion.Remove(index);
            return this;
        }

        _refsDictPortion[index] = value;
        return this;
    }
    
    private TableImpl_Complete SetString(LuaString index, LuaValue value)
    {
        if (value.Kind == LuaValueKind.Nil)
        {
            _stringsDictPortion.Remove(index);
            return this;
        }

        _stringsDictPortion[index] = value;
        return this;
    }

    private TableImpl_Complete SetInteger(long index, LuaValue value)
    {
        // Decrement because Lua tables use 1-based indexing
        index--;

        // Index isn't ever containable in list portion, add it to the dictionary portion instead
        if (index is < 0 or >= int.MaxValue)
        {
            if (value.Kind == LuaValueKind.Nil)
            {
                _dictPortion.Remove(index + 1);
                return this;
            }

            _dictPortion[index + 1] = value;
        }

        var intIndex = (int)index;
        
        // Setting index may require growing list
        if (intIndex >= _listPortion.Count)
        {
            // Value is outside list and is nil, remove it from dictionary portion if and return
            if (value.Kind == LuaValueKind.Nil)
            {
                _dictPortion.Remove(index + 1);
                return this;
            }
            
            // Appending directly onto the end will never increase the nil ratio
            if (intIndex == _listPortion.Count)
            {
                _listPortion.Add(value);
                ConcatPortions();
                return this;
            }

            // Otherwise, we need to append a certain number of nils before the new value to make sure it goes to the
            // correct index. To avoid wasting too much space, we only do this if it won't make more than half of the
            // list nil.
            var extraNils = _listPortion.Count - intIndex;
            if (_listNilCount + extraNils > (intIndex + 1) / 2)
            {
                // List portion would have nil ratio over half, add it to dictionary portion instead
                // Increment coercedIndex to undo earlier decrement
                _dictPortion[index + 1] = value;
                return this;
            }
            
            // Nil ratio won't be over half, we can append normally
            _listPortion.EnsureCapacity(_listPortion.Count + extraNils + 1);
            var initialDictIndex = _listPortion.Count + 1;
            for (var i = 0; i < extraNils; i++)
            {
                // While adding filler values, we should move anything in the dictionary we can to the list
                _dictPortion.TryGetValue(initialDictIndex + i, out var foundValue);
                if (foundValue.Kind == LuaValueKind.Nil) _listNilCount++;
                else _dictPortion.Remove(initialDictIndex + i);
                _listPortion.Add(foundValue);
            }
            _listPortion.Add(value);

            ConcatPortions();
            
            return this;
        }
        
        // Failing any of that, the index is definitely inside the bounds of the current list.
        if (_listPortion[intIndex].Kind == LuaValueKind.Nil) _listNilCount--;
        _listPortion[intIndex] = value;
        if (value.Kind != LuaValueKind.Nil) return this;
        
        // If we are setting the value to nil, we are possibly removing an element, and need to do some more checks.
        _listNilCount++;
            
        // If we set the last entry in the list to nil, we should trim any excess nils from the list.
        if (intIndex == _listPortion.Count - 1) 
            TableImplUtil.TrimExcessNils(_listPortion, ref _listNilCount);
            
        // Make sure we're not wasting more than half the list's space on nils, if it's big enough to matter.
        if (_listPortion.Count >= TableImplUtil.MinCountForMemorySaving && _listNilCount > _listPortion.Count / 2) 
            CleanListPortion();
        
        return this;
    }
    
    // Starting from the end of the list portion, moving non-nil values to the dictionary portion and replacing them
    // with nils, until we reach an index such that removing it and all greater indices would make the nil ratio less
    // than half, at which point we trim excess nils.
    private void CleanListPortion()
    {
        var cleanIndex = _listPortion.Count - 1;
        while (cleanIndex >= 0)
        {
            if (_listPortion[cleanIndex].Kind != LuaValueKind.Nil)
            {
                _dictPortion[cleanIndex + 1] = _listPortion[cleanIndex];
                _listPortion[cleanIndex] = default;
                _listNilCount++;
            }

            // cleanIndex = list size after trimming that index and above
            // count of nils after trimming = nil count - (listPortion.Count - cleanIndex)
            // nil ratio not above half when: nil count - listPortion.Count + cleanIndex <= cleanIndex / 2
            if (_listNilCount - _listPortion.Count + cleanIndex <= cleanIndex / 2) break;

            cleanIndex--;
        }
        
        TableImplUtil.TrimExcessNils(_listPortion, ref _listNilCount);
    }

    // Checks if there's a sequence of indices immediately after the end of the list portion in the dictionary portion,
    // and moves them to the list portion if there are.
    private void ConcatPortions()
    {
        var checkIndex = (long)_listPortion.Count + 1;
        
        // checkIndex - 1 < int.MaxValue
        while (checkIndex <= int.MaxValue)
        {
            _dictPortion.TryGetValue(checkIndex, out var value);
            // We stop if we hit a nil since that ends the contiguous sequence
            if (value.Kind == LuaValueKind.Nil) break;
            
            // Otherwise, move the value from the dictionary portion to the list portion
            _dictPortion.Remove(checkIndex);
            _listPortion.Add(value); // checkIndex will always be such that it is the next new index in the list portion

            checkIndex++;
        }
    }
}