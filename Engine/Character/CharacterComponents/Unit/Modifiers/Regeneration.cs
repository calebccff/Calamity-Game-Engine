using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{
    /// <summary>
    /// Example of a stat modifier that regenerates over time<br/>
    /// </summary>
    class RegenerationInt : Modifier<int>
    {
        /// <summary>
        /// The name of the Stat that contains the rate of the regeneration<br/>
        /// </summary>
        public string RegenerationStat;


        /// <summary>
        /// Initializes a new instance of the <see cref="RegenerationInt"/> class<br/>
        /// </summary>
        /// <param name="stat"> The Stat to modify</param>
        public RegenerationInt(Stat<int> stat) : base(stat)
        {

        }



        private float leftOver;
        /// <summary>
        /// Update,<br/>
        /// Increases the value of the stat by the rate of the regeneration<br/>
        /// It corrects for floating point inaccuracies with leftOver<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            float difference = (GetStat<int>(RegenerationStat) * Game.I._deltaTime) + leftOver;
            leftOver = difference - (int)difference;

            stat.Value += (int)difference;
            base.Update(gameTime);
        }



    }

}
