using System;
using System.Collections;
using System.Collections.Generic;

namespace Splines
{
    public class ListItemReplacedEventArgs<T> : EventArgs
    {
        public readonly int Index;
        public readonly T NewItem;
        public readonly T OldItem;

        public ListItemReplacedEventArgs(int index, T newItem, T oldItem)
        {
            Index = index;
            NewItem = newItem;
            OldItem = oldItem;
        }
    }

    public class ListModifiedEventArgs<T> : EventArgs
    {
        public readonly int Index;
        public readonly T Item;

        public ListModifiedEventArgs(int index, T item)
        {
            Index = index;
            Item = item;
        }
    }

    public interface IObservableReadOnlyList<T> : IReadOnlyList<T>
    {
        event EventHandler<ListModifiedEventArgs<T>> ItemAdded;
        event EventHandler<ListModifiedEventArgs<T>> ItemInserted;
        event EventHandler<ListModifiedEventArgs<T>> ItemRemoved;
        event EventHandler<ListItemReplacedEventArgs<T>> ItemReplaced;
        event EventHandler Cleared;

        int IndexOf(T item);
    }

    public interface IObservableList<T> : IObservableReadOnlyList<T>, IList<T>
    {
        new T this[int i] { get; set; }
    }

    public class ObservableList<T> : IObservableList<T>, IObservableReadOnlyList<T>
    {
        private readonly IList<T> list;

        public T this[int i] { 
            get => list[i];
            set {
                T oldValue = list[i];
                list[i] = value;
                ItemReplaced?.Invoke(this, new ListItemReplacedEventArgs<T>(i, value, oldValue));
            } 
        }

        public int Count => list.Count;

        public bool IsReadOnly => list.IsReadOnly;

        public event EventHandler<ListModifiedEventArgs<T>> ItemAdded;
        public event EventHandler<ListModifiedEventArgs<T>> ItemInserted;
        public event EventHandler<ListModifiedEventArgs<T>> ItemRemoved;
        public event EventHandler<ListItemReplacedEventArgs<T>> ItemReplaced;
        public event EventHandler Cleared;

        public void Add(T item)
        {
            list.Add(item);
            ItemAdded?.Invoke(this, new ListModifiedEventArgs<T>(Count - 1, item));
        }

        public bool Contains(T item) => list.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
        public int IndexOf(T item) => list.IndexOf(item);

        public void Insert(int index, T item)
        {
            list.Insert(index, item);
            ItemInserted?.Invoke(this, new ListModifiedEventArgs<T>(index, item));
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index == -1)
                return false;

            list.RemoveAt(index);
            ItemRemoved?.Invoke(this, new ListModifiedEventArgs<T>(index, item));
            return true;
        }

        public void RemoveAt(int index)
        {
            T item = list[index];

            list.RemoveAt(index);
            ItemRemoved?.Invoke(this, new ListModifiedEventArgs<T>(index, item));
        }

        public void Clear()
        {
            list.Clear();
            Cleared?.Invoke(this, new EventArgs());
        }

        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public ObservableList() => list = new List<T>();

        public ObservableList(IEnumerable<T> collection) => list = new List<T>(collection);

        public ObservableList(int capacity) => list = new List<T>(capacity);
    }
}
