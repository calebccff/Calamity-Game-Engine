using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{
    /// <summary>
    /// Represents a sorted list that maintains its items in order according to the provided comparer.<br/>
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class OrderedList<T> : IList<T>
    {
        // The comparer used for sorting the list.
        private readonly IComparer<T> comparer;
        // The inner list to hold the items.
        private readonly List<T> innerList = new List<T>();

        /// <summary>
        /// Initializes a new instance of the OrderedList class with the default comparer.<br/>
        /// </summary>
        public OrderedList()
            : this(Comparer<T>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the OrderedList class with the specified comparer.<br/>
        /// </summary>
        /// <param name="comparer">The comparer used for sorting the list.</param>
        public OrderedList(IComparer<T> comparer)
        {
            this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        /// <summary>
        /// Gets or sets the element at the specified index.<br/>
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            get => this.innerList[index];
            set => throw new NotSupportedException("Cannot set an indexed item in a sorted list.");
        }

        /// <summary>
        /// Gets the number of elements contained in the OrderedList.<br/>
        /// </summary>
        public int Count => this.innerList.Count;

        /// <summary>
        /// Defines IsReadOnly as false for the implementation of IList<br/>
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds an item to the OrderedList.<br/>
        /// </summary>
        /// <param name="item">The item to add to the list.</param>
        public void Add(T item)
        {
            int index = innerList.BinarySearch(item, comparer);
            index = (index >= 0) ? index : ~index;
            innerList.Insert(index, item);
        }

        /// <summary>
        /// Removes all items from the OrderedList.<br/>
        /// </summary>
        public void Clear() => this.innerList.Clear();

        /// <summary>
        /// Determines whether the OrderedList contains a specific value.<br/>
        /// </summary>
        /// <param name="item">The object to locate in the OrderedList.</param>
        /// <returns>true if item is found in the OrderedList; otherwise, false.</returns>
        public bool Contains(T item) => this.innerList.Contains(item);

        /// <summary>
        /// Copies the entire OrderedList to a compatible one-dimensional Array, starting at the specified index of the target array.<br/>
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from OrderedList. The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex) => this.innerList.CopyTo(array, arrayIndex);

        /// <summary>
        /// Returns an enumerator that iterates through the OrderedList.<br/>
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the OrderedList.</returns>
        public IEnumerator<T> GetEnumerator() => this.innerList.GetEnumerator();

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire OrderedList.<br/>
        /// </summary>
        /// <param name="item">The object to locate in the OrderedList.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire OrderedList, if found; otherwise, –1.</returns>
        public int IndexOf(T item) => this.innerList.IndexOf(item);

        /// <summary>
        /// Inserts an item to the OrderedList at the specified index.<br/>
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The item to insert into the OrderedList.</param>
        public void Insert(int index, T item) => throw new NotSupportedException("Cannot insert an indexed item in a sorted list.");

        /// <summary>
        /// Removes the first occurrence of a specific object from the OrderedList.<br/>
        /// </summary>
        /// <param name="item">The object to remove from the OrderedList.</param>
        /// <returns>true if item was successfully removed from the OrderedList; otherwise, false. This method also returns false if item is not found in the original OrderedList.</returns>
        public bool Remove(T item) => this.innerList.Remove(item);

        /// <summary>
        /// Removes the element at the specified index of the OrderedList.<br/>
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index) => this.innerList.RemoveAt(index);

        /// <summary>
        /// Returns an enumerator that iterates through the OrderedList.<br/>
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the OrderedList.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}