using Microsoft.Xna.Framework;
using System.Reflection;

namespace TestShader
{
    /// <summary>
    /// Base class for modifiers<br/>
    /// Used to handle temporary or permanent modifications for a Stat<br/>
    /// Has an Update function, thus can have complex behaviour<br/>
    /// </summary>
    /// <typeparam name="T">Type of the value of the Stat</typeparam>
    public class Modifier<T> where T : struct
    {
        /// <summary>
        /// The Stat to modify<br/>
        /// </summary>
        protected readonly Stat<T> stat;

        /// <summary>
        /// Initializes a new instance of the <see cref="Modifier{T}"/> class, for the specified Stat<br/>
        /// </summary>
        /// <param name="stat">The Stat to modify</param>
        public Modifier(Stat<T> stat)
        {
            this.stat = stat;
        }

        //TODO: Make all dependencie's Stats also Stat's of the original Not sure how to assert this

        /// <summary>
        /// Update<br/>
        /// </summary>
        /// <param name="gameTime"> The game time.</param>
        public virtual void Update(GameTime gameTime)
        {

        }

        /// <summary>
        /// Get the value of another Stat corresponding to the same character by name<br/>
        /// </summary>
        /// <typeparam name="U"> The type of the other Stat (Must be a Struct)</typeparam>
        /// <param name="name">The name of the other Stat</param>
        public Stat<U> GetStat<U>(string name) where U : struct
        {
            return (Stat<U>)stat.handler.stats[name];
        }

        /// <summary>
        /// UnsafeDispose,<br/>
        /// Disposes of unmanaged resources<br/>
        /// </summary>
        public virtual void UnsafeDispose()
        {

        }

    }
}