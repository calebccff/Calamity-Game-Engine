using System.Collections.Generic;
using System.Linq;

namespace TestShader
{

    //TODO: Merge with ITaggable as lost separate functionality

    /// <summary>
    /// Interface for classes that have a list of tags<br/>
    /// </summary>
    public interface ITaggable
    {
        /// <summary>
        /// Returns the tags of the component<br/>
        /// </summary>
        public List<Tag> GetTags();

        /// <summary>
        /// Returns if the component has the tag (or a subclass of it)<br/>
        /// </summary>
        /// <typeparam name="T"> The tag to check for</typeparam>
        /// <returns> If the component has the tag</returns>
        public bool Tag<T>() where T : Tag;


        /// <summary>
        /// Returns the tags of the component matching a given tag (or a subclass of it)<br/>
        /// </summary>
        /// <typeparam name="T"> The tag to check for</typeparam>
        /// <returns> The list of tags</returns>
        public List<Tag> Tags<T>() where T : Tag;

    }
}