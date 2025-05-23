using Ectoplasm.Runtime.LuaValue;

namespace Ectoplasm.Runtime.Tables;

public static class TableImplUtil
{
    /// <summary>
    /// A threshold for the number of excess <see cref="LuaValueKind.Nil"/> values that must be present in a list before
    /// memory-saving optimizations, like trimming excess list capacity and splitting part of a list into a dictionary,
    /// are performed. Each <see cref="LuaValue"/> is 24 bytes.
    /// </summary>
    internal const int MinCountForMemorySaving = 64;
    
    /// <summary>
    /// Removes excess <see cref="LuaValueKind.Nil"/> values at the end of a <see cref="List{T}"/> of
    /// <see cref="LuaValue"/>s.
    /// </summary>
    /// <param name="list">List to trim nils off of.</param>
    /// <param name="nilCount">
    /// Reference to int variable tracking the list's count of nils. Amount of nils removed is subtracted from this.
    /// </param>
    public static void TrimExcessNils(List<LuaValue.LuaValue> list, ref int nilCount)
    {
        var index = list.Count;
        while (list[index].Kind != LuaValueKind.Nil) index--;
        // Index is now the highest index with a non-nil value. Increment to get the highest nil value index.
        index++;

        // No excess nils, do nothing.
        if (index == list.Count) return;

        nilCount -= list.Count - index;
        list.RemoveRange(index, list.Count - index);
        if (list.Capacity - list.Count >= MinCountForMemorySaving) list.TrimExcess();
    }
    
    // Horribly gangly utility function that produces a TableImpl_Complete given old data and a new index/value pair.
    internal static TableImpl_Complete UpgradeToCompleteImpl(LuaValue.LuaValue index, LuaValue.LuaValue value,
        Dictionary<LuaString, LuaValue.LuaValue>? stringsDict = null, Dictionary<long, LuaValue.LuaValue>? intsDict = null,
        List<LuaValue.LuaValue>? list = null, int nilCount = 0)
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        => index.Kind switch
        {
            LuaValueKind.Boolean => index._boolean
                ? new TableImpl_Complete(value, default, [], [], stringsDict ?? [],
                    intsDict ?? [], list ?? [], nilCount)
                : new TableImpl_Complete(default, value, [], [], stringsDict ?? [],
                    intsDict ?? [], list ?? [], nilCount),
            LuaValueKind.Float => new TableImpl_Complete(default, default,
                new Dictionary<double, LuaValue.LuaValue> { { index._float, value } }, [],
                stringsDict ?? [], intsDict ?? [], list ?? [], nilCount),
            LuaValueKind.String => new TableImpl_Complete(default, default, [],
                [], new Dictionary<LuaString, LuaValue.LuaValue> { { (LuaString)index._ref, value } }, 
                intsDict ?? [], list ?? [], nilCount),
            LuaValueKind.Function or LuaValueKind.Userdata or LuaValueKind.Thread or LuaValueKind.Table 
                => new TableImpl_Complete(default, default, [],
                new Dictionary<object, LuaValue.LuaValue> { { index._ref, value } }, stringsDict ?? [],
                intsDict ?? [], list ?? [], nilCount),
            _ => throw new ArgumentException($"Failed to find suitable upgrade path for index (Kind = {index.Kind})",
                nameof(index))
        };

}