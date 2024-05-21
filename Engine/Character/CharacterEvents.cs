using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{
    /// <summary>
    /// Calling order for eventlisteners of StateChange<br/>
    /// </summary>
    public enum onStateChangeOrder { Default }


    /// <summary>
    /// Calling order for eventlisteners of PositionChange<br/>
    /// </summary>
    public enum onPositionChangeOrder { Default }

    /// <summary>
    /// Events corresponding to a character<br/>
    /// Implements _onPositionChange and _onStateChange for every character<br/>
    /// Any event may be accesed as CharacterEvents[name]<br/>
    /// Should be overloaded for more events<br/>
    /// </summary>
    public class CharacterEvents : EventCollection
    {
        public EventListener<onPositionChangeOrder> _onPositionChange { get; set; } = new EventListener<onPositionChangeOrder>();

        public EventListener<onStateChangeOrder> _onStateChange { get; set; } = new EventListener<onStateChangeOrder>();
    }
}
