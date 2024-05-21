using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{
    /// <summary>
    /// Base class for all modifiers that calculate the stat from other stats<br/>
    /// </summary>
    /// <typeparam name="T"> The type of the value of the Stat </typeparam>
    class Calculation<T> : Modifier<T> where T : struct
    {
        /// <summary>
        /// The function delegate that calculates the value of the stat from other Statistics of the same character<br/>
        /// </summary>
        Func<StatCharacterComponent, T> calculation;

        /// <summary>
        /// Initializes a new instance of the <see cref="Calculation{T}"/> class<br/>
        /// </summary>
        /// <param name="stat"> The Stat to modify</param>
        public Calculation(Stat<T> stat) : base(stat)
        {

        }

        /// <summary>
        /// Update,<br/>
        /// calculates the value of the stat from other Statistics of the same character<br/>
        /// </summary>
        /// <param name="gameTime"> The game time.</param>
        public override void Update(GameTime gameTime)
        {
            if (calculation != null)
            {
                stat.Value = calculation(stat.handler);
            }
            base.Update(gameTime);
        }
    }
}
