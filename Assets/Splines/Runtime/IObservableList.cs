using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Splines
{
    public class ItemAddedEventArgs<T> : EventArgs
    {
        public readonly int Index;
        public readonly T Item;

        public ItemAddedEventArgs(int index, T item)
        {
            Index = index;
            Item = item;
        }
    }

    public class ItemRemovedEventArgs<T> : EventArgs
    {
        public readonly int Index;
        public readonly T Item;

        public ItemRemovedEventArgs(int index, T item)
        {
            Index = index;
            Item = item;
        }
    }

    public class ItemMovedEventArgs<T> : EventArgs
    {
        public readonly int OldIndex;
        public readonly int NewIndex;
        public readonly T Item;

        public ItemMovedEventArgs(int oldIndex, int newIndex, T item)
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
            Item = item;
        }
    }

    public class ClearedEventArgs : EventArgs
    {

    }

    public interface IObservableList<T> : IList<T>
    {
        /// <summary>
        /// Fired when an item is added to the list. Fired after a <see cref="ItemRemoved"/> when an item is updated.
        /// </summary>
        event EventHandler<ItemAddedEventArgs<T>> ItemAdded;
        /// <summary>
        /// Fired when an item is removed from the list. Fired before a <see cref="ItemAdded"/> when an item is updated.
        /// </summary>
        event EventHandler<ItemRemovedEventArgs<T>> ItemRemoved;
        /// <summary>
        /// Fired when an item is moved within the list.
        /// </summary>
        event EventHandler<ItemMovedEventArgs<T>> ItemMoved;
        /// <summary>
        /// Fired when the list is cleared.
        /// </summary>
        event EventHandler<ClearedEventArgs> Cleared;
    }
}
