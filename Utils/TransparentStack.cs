namespace Ectoplasm.Utils;

/// <summary>
/// Has similar operations to a Stack, except you can inspect elements at any index instead of just the top.
/// </summary>
public class TransparentStack<T>
{
    private readonly List<T> _list = [];

    public int Count => _list.Count;
    
    public TransparentStack() { }

    public TransparentStack(IEnumerable<T> items)
    {
        _list = new List<T>(items);
    }

    public void Push(T item) => _list.Add(item);

    public T Pop()
    {
        var item = _list[^1];
        _list.RemoveAt(_list.Count - 1);
        return item;
    }

    /// <summary>
    /// Pops and discards multiple elements at once.
    /// </summary>
    public void PopMany(int count) => _list.RemoveRange(_list.Count - count, count);

    public T Peek() => _list[^1];

    /// <summary>
    /// Non-destructively enumerate all elements of the stack, starting from the topmost and going down.
    /// </summary>
    public IEnumerable<T> EnumerateTopDown()
    {
        for (var i = _list.Count - 1; i >= 0; i--)
            yield return _list[i];
    }

    public T this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }
}