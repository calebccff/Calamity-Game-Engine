using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TestShader
{
    /// <summary>
    /// Represents a collection of EventListeners<br/>
    /// It allows for events to be called by [name] in any subclass<br/>
    /// Events should be prefixed with _on<br/>
    /// </summary>
    public class EventCollection
    {


        // TODO: Handle Dictionaries too  (_on)DictionaryNameMemberName
        // Unused
        // public Dictionary<string,EventListener> collection;

        /// <summary>
        /// Gets an EventListener by name<br/>
        /// If the name is not prefixed with _on it will be prefixed<br/>
        /// </summary>
        /// <param name="name"> The name of the event</param>
        /// <returns> The EventListener</returns>
        /// <exception cref="System.ArgumentException"> Tried accessing non-existent event </exception>
        public virtual EventListener this[string name]
        {
            get
            {
                if (!name.StartsWith("_on")) name = "_on" + name;
                List<PropertyInfo> theProperty = (List<PropertyInfo>)GetType().GetProperties().Where((property) => property.Name == name && property.PropertyType.IsSubclassOf(typeof(EventListener)));
                if (theProperty.Count != 0)
                {
                    return theProperty.First().GetValue(this) as EventListener;
                }
                else
                {
                    throw new System.ArgumentException(name + " is not a event of " + GetType().Name);
                }

            }
        }


    }
}