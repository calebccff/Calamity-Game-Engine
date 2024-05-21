using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{
    /// <summary>
    /// Unused prototype for an enum that can be dynamically accessed and updated<br/>
    /// </summary>
    public class States : DynamicObject
    {

        /// <summary>
        /// The value of the enum<br/>
        /// </summary>
        public string value = "Default";


        /// <summary>
        /// The list of possible values<br/>
        /// </summary>
        static List<string> states
            = new List<string>() { "Default" };

        /// <summary>
        /// The number of possible values<br/>
        /// </summary>
        public static int Count
        {
            get
            {
                return states.Count;
            }
        }

        // If you try to get a value of a property
        // not defined in the class, this method is called.
        /// <summary>
        /// Accesses enum dynamically<br/>
        /// </summary>
        /// <param name="binder"> The name of the property to access </param>
        /// <param name="result"> The value of the property </param>
        /// <returns> True if the property was found </returns>
        /// <exception cref="ArgumentException"> If the property is not found </exception>
        public new static bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            if (states.Contains(binder.Name))
            {
                result = binder.Name;
                return true;
            }
            else throw new ArgumentException(binder.Name + " is not a member of enum.");
        }

        /// <summary>
        /// If you try to set a value of a property that is<br/>
        /// not defined in the class, this method is called.<br/>
        /// We can't add new members to enum<br/>
        /// </summary>
        /// <param name="binder"> The name of the property to set </param>
        /// <param name="value"> The value to set</param>
        /// <returns> Did the operation succeed</returns>
        public new static bool TrySetMember(
            SetMemberBinder binder, object value)
        {
            return false;
        }
    }

}

