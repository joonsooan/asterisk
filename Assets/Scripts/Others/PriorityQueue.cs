using System.Collections.Generic;
using System.Linq;

public class PriorityQueue<T>
{
    private readonly SortedDictionary<float, Queue<T>> _dictionary = new SortedDictionary<float, Queue<T>>();
    public int Count { get; private set; }

    public void Enqueue(T item, float priority)
    {
        if (!_dictionary.ContainsKey(priority)) {
            _dictionary[priority] = new Queue<T>();
        }
        _dictionary[priority].Enqueue(item);
        Count++;
    }

    public T Dequeue()
    {
        KeyValuePair<float, Queue<T>> pair = _dictionary.First();
        T item = pair.Value.Dequeue();

        if (pair.Value.Count == 0) {
            _dictionary.Remove(pair.Key);
        }
        Count--;
        return item;
    }

    public bool IsEmpty()
    {
        return Count == 0;
    }
}
