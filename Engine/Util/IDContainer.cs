using System;
using System.Collections.Generic;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// Represents a container for holding components with unique identifiers.<br/>
    /// </summary>
    public class IDContainer
    {
        /// <summary>
        /// The list of components contained in the IDContainer.<br/>
        /// </summary>
        public List<Component> _list = new List<Component>();

        /// <summary>
        /// Adds a component to the IDContainer.<br/>
        /// </summary>
        /// <param name="comp">The component to add.</param>
        public void Add(Component comp)
        {
            int index = ~_list.BinarySearch(comp);
            if (index < 0)
            {
                throw new System.ArgumentException("Tried inserting " + comp + " with id:" + comp._id + " but it's already contained");
            }
            else if (index == _list.Count)
            {
                _list.Add(comp);
            }
            else
            {
                _list.Insert(index, comp);
            }
        }

        /// <summary>
        /// Removes a component from the IDContainer.<br/>
        /// </summary>
        /// <param name="comp">The component to remove.</param>
        public void Remove(Component comp)
        {
            int index = _list.BinarySearch(comp);
            if (index < 0)
            {
                throw new System.ArgumentException("Tried removing " + comp + " with id:" + comp._id + " but it's not found");
            }
            else
            {
                _list.RemoveAt(index);
            }
        }

        /// <summary>
        /// Gets the number of components in the IDContainer.<br/>
        /// </summary>
        public int Count { get { return _list.Count; } }

        // TODO: Implement disposal of all components (if applicable).
    }
}