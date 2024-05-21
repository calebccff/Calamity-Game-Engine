using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{
    /// <summary>
    /// The labels for event listeners of _onChange for order of execution<br/>
    /// </summary>
    public enum onStatChangeOrder
    {
        Default
    }



    /// <summary>
    /// The abstract base class for all stats regardless of value type<br/>
    /// </summary>
    public abstract class GenStat
    {
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Disposes of the stat<br/>
        /// </summary>
        public virtual void UnsafeDispose() { }

    }

    /// <summary>
    /// The base class for all stats with a value<br/>
    /// It handles calling _onChange when the value changes<br/>
    /// It has an implicit conversion to its value<br/>
    /// It handles Stat modifiers, and their updates<br/>
    /// </summary>
    /// <typeparam name="T"> The type of the value</typeparam>
    public class Stat<T> : GenStat
    where T : struct
    {
        /// <summary>
        /// The value of the stat<br/>
        /// </summary>
        private T _private = default(T);

        /// <summary>
        /// The StatCharacterComponent corresponding to this stat<br/>
        /// </summary>
        public readonly StatCharacterComponent handler;

        /// <summary>
        /// Constructor for Stat<br/>
        /// </summary>
        /// <param name="handler"> The StatCharacterComponent corresponding to this stat </param>
        public Stat(StatCharacterComponent handler)
        {
            this.handler = handler;
        }

        /// <summary>
        /// Constructor for Stat with value<br/>
        /// </summary>
        /// <param name="handler"> The StatCharacterComponent corresponding to this stat</param>
        /// <param name="startingValue"> The starting value of the stat</param>
        public Stat(StatCharacterComponent handler, T startingValue)
        {
            this.handler = handler;
            _private = startingValue;
        }

        /// <summary>
        /// Implicit conversion overload to the value of the Stat<br/>
        /// </summary>
        /// <param name="stat"> The stat to convert </param>
        public static implicit operator T(Stat<T> stat)
        {
            return stat._private;
        }

        /// <summary>
        /// The value of the stat as a property<br/>
        /// on setting the value it calls _onChange<br/>
        /// </summary>
        public T Value
        {
            get => _private; set
            {
                if (_private.Equals(value)) return;
                T oldVal = _private;
                T newVal = value;
                _private = newVal;
                _onChange.Invoke(oldVal, _private);
            }
        }


        /// <summary>
        /// Update,<br/>
        /// Called by StatCharacterComponent<br/>
        /// Updates all modifiers corresponding to this stat<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {

            foreach (Modifier<T> modifier in modifiers)
            {
                modifier.Update(gameTime);
            }
        }

        /// <summary>
        /// Dispose,<br/>
        /// Called by StatCharacterComponent at UnsafeDispose<br/>
        /// Disposes all modifiers<br/>
        /// </summary>
        public override void UnsafeDispose()
        {

            foreach (Modifier<T> modifier in modifiers)
            {
                modifier.UnsafeDispose();
            }
        }

        /// <summary>
        /// Adds a modifiear to the Stat<br/>
        /// </summary>
        /// <param name="modifier"></param>
        public virtual void AddModifier(Modifier<T> modifier) => modifiers.Add(modifier);

        /// <summary>
        /// List of modifiers corresponding to this Stat<br/>
        /// </summary>
        public List<Modifier<T>> modifiers = new List<Modifier<T>>();

        /// <summary>
        /// The event listener for _onChange<br/>
        /// Called when the value of the Stat changes<br/>
        /// </summary>
        public EventListener<onStatChangeOrder> _onChange = new EventListener<onStatChangeOrder>();
    }
}
