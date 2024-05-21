using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace TestShader
{
    /// <summary>
    /// Represents a particle that can be animated<br/>
    /// </summary>
    public class AnimatedParticle : Particle
    {
        /// <summary>
        /// The animation that this particle uses<br/>
        /// </summary>
        public Animation _anim;

        /// <summary>
        /// The time since the animation started (times fps)<br/>
        /// </summary>
        public double _animationTime = 0;

        /// <summary>
        /// Update the particle animation<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(float gameTime)
        {
            //Calculates the animation time
            _animationTime += Game.I._deltaTime * 60.0;
            
            //Checks if the animation is over
            if (_animationTime >= _anim.Length() * _anim._animationStepSpeed)
            {
                //Resets the animation
                _animationTime = 0.0;

            }

            //Gets the current frame
            _rect = _anim.Frame((int)_animationTime);

            base.Update(gameTime);
        }



    }
}
