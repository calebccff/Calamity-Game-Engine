using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{
    /// <summary>
    /// An AnimatedCharacterComponent that changes the animation for each state of the character<br/>
    /// It may be used as a quickhand for state-based animations for simple characters<br/>
    /// Each state's animation interrupts the previous<br/>
    /// </summary>
    public class StateAnimatedCharacterComponent : AnimatedCharacterComponent
    {
        /// <summary>
        /// Constructor<br/>
        /// </summary>
        /// <param name="texture"> The initial texture of the sprite</param>
        /// <param name="character"> The character to attach the sprite to</param>
        public StateAnimatedCharacterComponent(Texture2D texture, Character character) : base(texture, character)
        {
        }

        /// <summary>
        /// Called when the character's state changes<br/>
        /// Changes the animation to the new state's<br/>
        /// </summary>
        /// <param name="Old"> Name of the old state</param>
        /// <param name="New"> Name of the new state</param>
        public override void OnStateChange(string Old, string New)
        {
            if (_animations.ContainsKey(New))
            {
                _anim.ChangeAnimationState(New);
            }
        }

    }
}
