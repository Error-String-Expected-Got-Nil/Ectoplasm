namespace Ectoplasm.Runtime.Tables;

public static class TableImplUtil
{
    /// <summary>
    /// A threshold for the number of excess <see cref="LuaValueKind.Nil"/> values that must be present in a list before
    /// memory-saving optimizations, like trimming excess list capacity and splitting part of a list into a dictionary,
    /// are performed. Each <see cref="LuaValue"/> is 16 bytes, so every 64 count here is 1 KiB.
    /// </summary>
    internal const int MinCountForMemorySaving = 128;
    
    /// <summary>
    /// Removes excess <see cref="LuaValueKind.Nil"/> values at the end of a <see cref="List{T}"/> of
    /// <see cref="LuaValue"/>s.
    /// </summary>
    /// <param name="list">List to trim nils off of.</param>
    /// <param name="nilCount">
    /// Reference to int variable tracking the list's count of nils. Amount of nils removed is subtracted from this.
    /// </param>
    public static void TrimExcessNils(List<LuaValue> list, ref int nilCount)
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
}