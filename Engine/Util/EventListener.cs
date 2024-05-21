using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{
    /// <summary>
    /// A class for handling a single event with a list of delegates called where the order of calls doesn't matter<br/>
    /// It has an overload for handling cases where the order of calls matters<br/>
    /// </summary>
    public class EventListener
    {
        /// <summary>
        /// The list of functions that are called when the event is triggered<br/>
        /// </summary>
        protected virtual OrderedList<EventCall> callers { get; set; } = new OrderedList<EventCall>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventListener"/> class.<br/>
        /// </summary>
        public EventListener()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="EventListener"/> class.<br/>
        /// </summary>
        /// <param name="callers"> The list of functions that are called when the event is triggered</param>
        public EventListener(OrderedList<EventCall> callers)
        {
            this.callers = callers;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventListener"/> class from another as a copy<br/>
        /// </summary>
        /// <param name="copy"> The EventListener to copy</param>
        public EventListener(EventListener copy)
        {
            this.callers = copy.callers;
        }




        /// <summary>
        /// Add an action to be called when the event is triggered<br/>
        /// </summary>
        /// <param name="b"> The action with a list of objects as parameter to add</param>
        public virtual void Add(Action<List<object>> b)
        => Add(new EventCall(b));

        /// <summary>
        /// Add an action to be called when the event is triggered<br/>
        /// </summary>
        /// <param name="b"> The action with an array of objects as parameter to add</param>
        public virtual void Add(Action<object[]> b)
        => Add(new EventCall(new Action<List<object>>((param) => b(param.ToArray()))));


        /// <summary>
        /// Add an action to be called when the event is triggered<br/>
        /// The input is a generic delegate, which will be dynamicly invoked with an array of objects<br/>
        /// </summary>
        /// <param name="b"> The generaic delegate to add</param>
        public virtual void Add(Delegate b)
        => Add(new EventCall(new Action<object[]>((param) => b.DynamicInvoke(param))));



        /// <summary>
        /// Add an EventCall to be called when the event is triggered<br/>
        /// </summary>
        public virtual void Add(EventCall b)
        {
            callers.Add(b);
        }




        /// <summary>
        /// Removes a generic delegate from the list of functions to be called<br/>
        /// </summary>
        /// <param name="b"></param>
        public virtual void RemoveFunctions(Delegate b)
        {
            RemoveFunctions((dict) => b.DynamicInvoke(dict.ToArray()));
        }


        /// <summary>
        /// Removes an action with a list of objects as parameter from the list of functions to be called<br/>
        /// </summary>
        /// <param name="b"></param>
        public virtual void RemoveFunctions(Action<List<object>> b)
        {
            foreach (EventCall caller in (callers.Where((caller) => caller.Function == b)))//Ha hiba van, ez az
            {
                callers.Remove(caller);
            }
        }

        /// <summary>
        /// Removes an EventCall from the list of functions to be called<br/>
        /// </summary>
        /// <param name="b"></param>
        public virtual void Remove(EventCall b)
        {
            callers.Remove(b);
        }

        /// <summary>
        /// Invokes all the functions in the list with the given parameters<br/>
        /// </summary>
        /// <param name="param">A list of objects to pass to the functions</param>
        public virtual void Invoke(List<object> param)
        {
            foreach (EventCall eventCall in callers)
            {
                eventCall.Function(param);
            }
        }

        /// <summary>
        /// Invokes all the functions in the list with the given parameters<br/>
        /// </summary>
        /// <param name="param"> An array of objects to pass to the functions</param>
        public virtual void Invoke(params object[] param)
        {
            foreach (EventCall eventCall in callers)
            {
                eventCall.Function.DynamicInvoke(param.ToList());
            }
        }


    }


    /// <summary>
    /// A class for handling a single event with a list of delegates called where the order of calls matters<br/>
    /// </summary>
    /// <typeparam name="U">The enumerator that is used for the ordering of the calls of the functions</typeparam>
    public class EventListener<U> : EventListener
        where U : System.Enum
    {
        //TODO: Test if ordering actually works Ez
        //Not sure, if static operator overrides are applied when comparing EventCall's, I think not
        //Just use a custom comparer in the OrderedList constructor

        /// <summary>
        /// The list of functions that are called when the event is triggered<br/>
        /// It is ordered by the value of the EventCall-s in the enumerator <typeparamref name="U"/><br/>
        /// It should only contain EventCall<\U>-s<br/>
        /// </summary>
        protected override OrderedList<EventCall> callers { get; set; } = new OrderedList<EventCall>();

        /// <summary>
        /// Creates an empty EventListener<br/>
        /// </summary>
        public EventListener() : base()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="EventListener"/> class<br/>
        /// </summary>
        /// <param name="callers"> The list of functions that are called when the event is triggered, should only contain EventCall<\U>-s</param>
        public EventListener(OrderedList<EventCall> callers)
        {
            this.callers = callers;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventListener"/> class<br/>
        /// </summary>
        /// <param name="copy"> The EventListener to copy </param>
        public EventListener(EventListener<U> copy)
        {
            this.callers = copy.callers;
        }

        /// <summary>
        /// Add an action to be called when the event is triggered<br/>
        /// </summary>
        /// <param name="b"> The action with a list of objects as parameter to add</param>
        public override void Add(Action<List<object>> b)
        => Add(new EventCall<U>(b, (U)(object)0));


        /// <summary>
        /// Add an action to be called when the event is triggered<br/>
        /// </summary>
        /// <param name="b"> The action with an array of objects as parameter to add </param>
        public override void Add(Action<object[]> b)
        => Add(new EventCall<U>(new Action<List<object>>((param) => b(param.ToArray())), (U)(object)0));


        /// <summary>
        /// Add an action to be called when the event is triggered<br/>
        /// The input is a generic delegate, which will be dynamicly invoked with an array of objects<br/>
        /// </summary>
        /// <param name="b"> The generaic delegate to add</param>
        public override void Add(Delegate b)
        => Add(new EventCall<U>(new Action<List<object>>((param) => b.DynamicInvoke(param.ToArray())), (U)(object)0));


        /// <summary>
        /// Add an EventCall to be called when the event is triggered<br/>
        /// </summary>
        public override void Add(EventCall b)
        {
            callers.Add(b);
        }




        /// <summary>
        /// Removes an action from the list of functions to be called<br/>
        /// </summary>
        /// <param name="b"> The generaic delegate to remove</param>
        public override void RemoveFunctions(Delegate b)
        {
            RemoveFunctions((dict) => b.DynamicInvoke(dict.ToArray()));
        }

        /// <summary>
        /// Removes a generic delegate from the list of functions to be called<br/>
        /// </summary>
        /// <param name="b"> The action with a list of objects as parameter to remove</param>
        public override void RemoveFunctions(Action<List<object>> b)
        {
            foreach (EventCall<U> caller in (callers.Where((caller) => caller.Function == b)))//Ha hiba van, ez az
            {
                callers.Remove(caller);
            }
        }

        /// <summary>
        /// Removes an EventCall from the list of functions to be called<br/>
        /// </summary>
        /// <param name="b"> The EventCall to remove</param>
        public override void Remove(EventCall b)
        {
            callers.Remove(b);
        }

        /// <summary>
        /// Invokes all the functions in the list with the given parameters<br/>
        /// </summary>
        /// <param name="param"> The list of objects to pass to the functions</param>
        public override void Invoke(List<object> param)
        {
            //The ordered list should already take care of the ordering
            foreach (EventCall<U> eventCall in callers)
            {
                eventCall.Function(param);
            }
        }

        /// <summary>
        /// Invokes all the functions in the list with the given parameters<br/>
        /// </summary>
        /// <param name="param"> The array of objects to pass to the functions</param>
        public override void Invoke(params object[] param)
        {
            //The ordered list should already take care of the ordering
            foreach (EventCall<U> eventCall in callers)
            {
                eventCall.Function.DynamicInvoke(param.ToList());
            }
        }


    }
}

