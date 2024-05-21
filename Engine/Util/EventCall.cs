using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{




    /// <summary>
    /// Class for handling a single delegate subscribed to an event<br/>
    /// </summary>
    public class EventCall : IComparable<EventCall>

    {
        /// <summary>
        /// The maximal ID of any EventCall<br/>
        /// </summary>
        static int IDcount = 0;

        /// <summary>
        /// The ID of the EventCall<br/>
        /// </summary>
        public int ID = 0;

        /// <summary>
        /// Creates a new EventCall with the action with a list of objects as parameter<br/>
        /// </summary>
        /// <param name="function"></param>
        public EventCall(Action<List<object>> function)
        {
            Function = function;
            ID = IDcount++;
        }

        /// <summary>
        /// Creates a new EventCall with the action with an array of objects as parameter<br/>
        /// </summary>
        /// <param name="function"></param>
        public EventCall(Action<object[]> function)
        {
            Function = new Action<List<object>>((dict) => function(dict.ToArray()));
            ID = IDcount++;
        }


        /// <summary>
        /// Creates a new EventCall with the action<br/>
        /// </summary>
        /// <param name="function"></param>
        public EventCall(Action function)
        {

            Function = new Action<List<object>>((dict) => function());
            ID = IDcount++;
        }


        /// <summary>
        /// The action delegate to be called when the event is triggered<br/>
        /// </summary>
        public Action<List<object>> Function { get; }

        /// <summary>
        /// Defines the comparability of two EventCalls based on their ID<br/>
        /// </summary>
        public int CompareTo(EventCall other)
        {
            return ID.CompareTo(other.ID);
        }

        /// <summary>
        /// Defines the comparability of any object to this EventCall based on their ID<br/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is EventCall)
                return ID.Equals((obj as EventCall).ID);
            return false;
        }

        /// <summary>
        /// Gets the hashcode of the EventCall's ID<br/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        /// <summary>
        /// Operator overload for == using CompareTo<br/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(EventCall left, EventCall right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Operator overload for != using the overload for ==<br/>
        /// </summary>
        public static bool operator !=(EventCall left, EventCall right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Class for handling a single delegate subscribed to an event<br/>
    /// Used for eventListeners where the order of calls matters<br/>
    /// </summary>
    /// <typeparam name="U"> The enumerator that is used for the ordering of the calls of the EventCalls by the EventListener</typeparam>
    public class EventCall<U> : EventCall, IComparable<EventCall<U>>
        where U : System.Enum
    {

        /// <summary>
        /// Creates a new EventCall with the action with a list of objects as parameter and the given value from the enum to define when it should be called<br/>
        /// </summary>
        public EventCall(Action<List<object>> function, U depth) : base(function)
        {

            Depth = depth;
        }

        /// <summary>
        /// The value from the enum <typeparamref name="U"/> to define when it should be called<br/>
        /// </summary>
        public U Depth { get; }

        /// <summary>
        /// Defines the comparability of two EventCalls<\U> based on their depth<br/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(EventCall<U> obj)
        {
            return Depth.CompareTo(obj.Depth);
        }

        /// <summary>
        /// Defines the comparability of any object to this EventCall<\U> based on their depth if it's an EventCall<\U><br/>
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is EventCall<U>)
                return ID.Equals((obj as EventCall<U>).ID);
            return false;
        }

        /// <summary>
        /// Gets the hashcode of the EventCall<\U>'s ID<br/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        /// <summary>
        /// Operator overload for == using CompareTo<br/>
        /// </summary>
        public static bool operator ==(EventCall<U> left, EventCall<U> right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Operator overload for != using the overload for ==<br/>
        /// </summary>
        public static bool operator !=(EventCall<U> left, EventCall<U> right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Operator overload for <\ using the overload for CompareTo<br/>
        /// </summary>
        public static bool operator <(EventCall<U> left, EventCall<U> right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Operator overload for <=\ using the overload for CompareTo<br/>
        /// </summary>
        public static bool operator <=(EventCall<U> left, EventCall<U> right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Operator overload for >\ using the overload for CompareTo<br/>
        /// </summary>
        public static bool operator >(EventCall<U> left, EventCall<U> right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Operator overload for >=\ using the overload for CompareTo<br/>
        /// </summary>
        public static bool operator >=(EventCall<U> left, EventCall<U> right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }
    }
}
