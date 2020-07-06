using Splines;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Splines
{
    public class ObservableList<T> : IObservableList<T>
    {
        private readonly IList<T> items = new List<T>();

        public event EventHandler<ItemAddedEventArgs<T>> ItemAdded;
        public event EventHandler<ItemRemovedEventArgs<T>> ItemRemoved;
        public event EventHandler<ItemMovedEventArgs<T>> ItemMoved;
        public event EventHandler<ClearedEventArgs> Cleared;

        public T this[int index]
        {
            get => items[index];
            set
            {
                T item = items[index];
                items[index] = value;
                ItemRemoved?.Invoke(this, new ItemRemovedEventArgs<T>(index, item));
                ItemAdded?.Invoke(this, new ItemAddedEventArgs<T>(index, value));
            }
        }

        public int Count => items.Count;

        public bool IsReadOnly => items.IsReadOnly;

        public void Add(T item)
        {
            items.Add(item);
            ItemAdded?.Invoke(this, new ItemAddedEventArgs<T>(Count - 1, item));
        }

        public void Clear()
        {
            items.Clear();
            Cleared?.Invoke(this, new ClearedEventArgs());
        }

        public bool Contains(T item) => items.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) =>
            items.CopyTo(array, arrayIndex);

        public int IndexOf(T item) => items.IndexOf(item);

        public void Insert(int index, T item)
        {
            items.Insert(index, item);
            ItemAdded?.Invoke(this, new ItemAddedEventArgs<T>(index, item));

            for (int i = index + 1; i < items.Count; i++)
                ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(i - 1, i, items[i]));
        }

        public bool Remove(T item)
        {
            int index = items.IndexOf(item);

            if (index < 0) return false;

            items.RemoveAt(index);
            ItemRemoved?.Invoke(this, new ItemRemovedEventArgs<T>(index, item));

            for (int i = index + 1; i < items.Count; i++)
                ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(i + 1, i, items[i]));
            return true;
        }

        public void RemoveAt(int index)
        {
            T curve = items[index];
            items.RemoveAt(index);
            ItemRemoved?.Invoke(this, new ItemRemovedEventArgs<T>(index, curve));

            for (int i = index + 1; i < items.Count; i++)
                ItemMoved?.Invoke(this, new ItemMovedEventArgs<T>(i + 1, i, items[i]));
        }

        public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }
}
