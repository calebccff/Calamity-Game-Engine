using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{
    /// <summary>
    /// A Stat modifier that resets the value of another Stat when a specified event is triggered<br/>
    /// </summary>
    /// <typeparam name="T"> The type of the value of the Stat</typeparam>
    class ResetListener<T> : Modifier<T> where T : struct
    {
        /// <summary>
        /// The value to reset the Stat to (containing the "default" value of the stat)<br/>
        /// </summary>
        public string ResetStat;

        /// <summary>
        /// The event that triggers the reset<br/>
        /// </summary>
        private EventListener _privateListener;

        /// <summary>
        /// The event that triggers the reset<br/>
        /// On setting, subscribes the reset function to the EventListener and unsubscribes the old one<br/>
        /// </summary>
        public EventListener Listener
        {
            get { return _privateListener; }
            set { ChangeSubscribe(_privateListener, value); _privateListener = value; }
        }

        /// <summary>
        /// Subscribes the reset function to a new EventListener and unsubscribes it from the old one<br/>
        /// </summary>
        /// <param name="oldListener"> The old EventListener</param>
        /// <param name="newListener"> The new EventListener</param>
        private void ChangeSubscribe(EventListener oldListener, EventListener newListener)
        {
            if (oldListener != null) oldListener.RemoveFunctions(OnReset);
            if (newListener != null) newListener.Add(OnReset);
        }

        /// <summary>
        /// An event that is triggered when the stat is reset<br/>
        /// </summary>
        public EventListener _onReset = new EventListener();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetListener{T}"/> class<br/>
        /// </summary>
        /// <param name="stat"> The Stat to modify</param>
        public ResetListener(Stat<T> stat) : base(stat)
        {

        }

        /// <summary>
        /// The function resetting the stat<br/>
        /// Called by the eventlistener in the modifier<br/>
        /// </summary>
        public void OnReset(params object[] objects)
        {
            T oldValue = stat.Value;
            stat.Value = GetStat<T>(ResetStat);
            _onReset.Invoke(oldValue, stat.Value);
        }

        /// <summary>
        /// UnsafeDispose,<br/>
        /// Unsubscribes the reset function from the EventListener<br/>
        /// </summary>
        public override void UnsafeDispose()
        {

            if (Listener != null)
                Listener.RemoveFunctions(OnReset);

        }

    }
}
