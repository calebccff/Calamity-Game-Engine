using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestShader
{
    public class AnimatedGameComponent : SpriteGameComponent
    {
        /// <summary>
        /// The currently playing animation<br/>
        /// </summary>
        public Animation _currentAnimation;

        /// <summary>
        /// The current frame of the animation<br/>
        /// </summary>
        public Rectangle _currentFrame;

        /// <summary>
        /// The current animation state name<br/>
        /// </summary>
        public string _currentAnimationState;

        /// <summary>
        /// The next animation state name<br/>
        /// </summary>
        public string _nextAnimationState;

        /// <summary>
        /// The dictionary of animations from names<br/>
        /// </summary>
        public SortedDictionary<string, Animation> _animations =
            new SortedDictionary<string, Animation>();

        /// <summary>
        /// The time since the animation started multiplied by the number of frames a second (60)<br/>
        /// </summary>
        public double _animationTime = 0;


        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedGameComponent"/> class.<br/>
        /// </summary>
        /// <param name="texture">The sprite sheet for the animations</param>
        public AnimatedGameComponent(Texture2D texture) : base(texture)
        {

        }

        /// <summary>
        /// Update,<br/>
        /// Calculates the current animation frame<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            //Calculates the animation time
            _animationTime += Game.I._deltaTime * 60.0;

            // Checks if the animation is over
            if (_animationTime >= _currentAnimation.Length() * _currentAnimation._animationStepSpeed)
            {
                // Resets the animation time
                _animationTime = 0.0;

                // If the next sceduled animation is not the same as the current, change the animation
                if (_currentAnimationState != _nextAnimationState)
                {
                    _currentAnimationState = _nextAnimationState;
                    _currentAnimation = _animations[_nextAnimationState].Clone() as Animation;
                }
            }
            // Sets the current frame
            _currentFrame = _currentAnimation.Frame((int)_animationTime);
            // Sets the current origin
            _origin = _currentAnimation._origin;

            base.Update(gameTime);
        }

        /// <summary>
        /// Draw,<br/>
        /// Sets the sprite frame to the current animation frame<br/>
        /// Calls the base draw<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            _spriteFrame = _currentFrame;
            base.Draw(gameTime);
        }

        /// <summary>
        /// Changes the animation state to the state with the given name<br/>
        /// It interrupts the current animation<br/>
        /// </summary>
        /// <param name="stateName">The name of the state</param>
        /// <exception cref="System.ArgumentException">There is no animation with the given name</exception>
        public void ChangeAnimationState(string stateName)
        {
            //Should be called before Draw (otherwise first frame will be skipped)

            //Checks if the animation exists
            if (!_animations.ContainsKey(stateName))
            {
                throw new System.ArgumentException("There is no animation called", stateName);
            }

            //Changes the animation
            _currentAnimationState = stateName;
            _nextAnimationState = stateName;
            _currentAnimation = _animations[stateName].Clone() as Animation;

            //Resets the animation time
            _animationTime = 0;

            //Sets the width and height
            _width = _currentAnimation.Frame((int)_animationTime).Width;
            _height = _currentAnimation.Frame((int)_animationTime).Height;
        }

        /// <summary>
        /// Changes the animation state to the state with the given name<br/>
        /// It does not interrupt the current animation, the next animation is played after the current one is finished<br/>
        /// </summary>
        /// <param name="stateName"> The name of the state</param>
        /// <exception cref="System.ArgumentException"> There is no animation with the given name</exception>
        public void ChangeNextAnimationState(string stateName)
        {
            //Checks if the animation exists
            if (!_animations.ContainsKey(stateName))
            {
                throw new System.ArgumentException("There is no animation called: ", stateName);
            }
            //Changes the next scheduled animation
            _nextAnimationState = stateName;
        }

    }
}
