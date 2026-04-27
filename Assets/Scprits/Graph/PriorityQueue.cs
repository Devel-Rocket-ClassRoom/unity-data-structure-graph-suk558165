using System.Collections.Generic;
using System;

public class PriorityQueue<TElement, TPriority>
{
    // 부모 (i - 1) / 2
    // 왼쪽 2 * i + 1
    // 오른쪽 2 * i + 2
    private List<(TElement, TPriority)> data = new List<(TElement, TPriority)>();
    private readonly IComparer<TPriority> comparer;
    // IComparer
    public int Count { get => data.Count; }

    private int Left(int i) => 2 * i + 1;
    private int Right(int i) => 2 * i + 2;
    private int Parent(int i) => (i - 1) / 2;
    public PriorityQueue()
    {
        comparer = Comparer<TPriority>.Default;
    }

    public PriorityQueue(IComparer<TPriority> comparer)
    {
        this.comparer = comparer;
    }

    public void Enqueue(TElement element, TPriority priority)
    {
        data.Add((element, priority));

        int index = data.Count - 1;

        while (true)
        {
            if (index == 0)
            {
                break;
            }

            int parent = Parent(index);
            int compare = comparer.Compare(data[index].Item2, data[parent].Item2); 

            if (compare < 0)
            {
                (TElement, TPriority) tmpData = data[index];
                data[index] = data[parent];
                data[parent] = tmpData;
                index = parent;
            }
            else
            {
                break;
            }
        }
    }

    public TElement Dequeue()
    {
        if (data.Count == 0)
        {
            throw new InvalidOperationException("큐가 비어 있습니다.");
        }
        TElement returnData = data[0].Item1;

        data[0] = data[data.Count - 1];
        data.RemoveAt(data.Count - 1);

        int index = 0;

        while (true)
        {
            int left = Left(index);
            int right = Right(index);
            int smallIndex = -1;

            if (left < data.Count && right < data.Count)
            {
                int compare = comparer.Compare(data[left].Item2, data[right].Item2);

                smallIndex = compare <= 0 ? left : right;

                int compare2 = comparer.Compare(data[smallIndex].Item2, data[index].Item2);

                if (compare2 < 0)
                {
                    (TElement, TPriority) tmpData = data[index];
                    data[index] = data[smallIndex];
                    data[smallIndex] = tmpData;
                    index = smallIndex;
                }
                else
                {
                    break;
                }
            }
            else if (left < data.Count)
            {
                int compare = comparer.Compare(data[left].Item2, data[index].Item2);

                if (compare < 0)
                {
                    (TElement, TPriority) tmpData = data[index];
                    data[index] = data[left];
                    data[left] = tmpData;
                    index = left;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return returnData;
    }

    public TElement Peek()
    {
        if (data.Count == 0)
        {
            throw new InvalidOperationException("큐가 비어 있습니다.");
        }
        return data[0].Item1;
    }
    public void Clear()
    {
        data.Clear();
    }

    public string PrintData()
    {
        string result = string.Empty;
        for (int i = 0; i < data.Count; i++)
        {
            result += data[i].Item1.ToString() + ", ";
        }
        return result;
    }
}