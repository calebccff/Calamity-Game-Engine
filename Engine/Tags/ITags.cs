using System.Collections.Generic;

namespace TestShader
{
    /// <summary>
    /// Interface for classes that have a list of tags<br/>
    /// </summary>
    public interface ITags
    {
        /// <summary>
        /// List of tags<br/>
        /// </summary>
        List<Tag> TagList { get; set; }

        /// <summary>
        /// Get list of tags<br/>
        /// </summary>
        /// <returns>List of tags</returns>
        List<Tag> GetTags();

        /// <summary>
        /// Get if a tag of the given type or  any subtype is attached to this object<br/>
        /// </summary>
        /// <typeparam name="T"> Type of Tag</typeparam>
        /// <returns> True if the object has the tag</returns>
        bool Tag<T>() where T : Tag;

        /// <summary>
        /// Get list of tags attached to this object for a given type (or any subtype)<br/>
        /// </summary>
        /// <typeparam name="T"> Type of Tag</typeparam>
        /// <returns> List of tags of the given type</returns>
        List<Tag> Tags<T>() where T : Tag;
    }
}