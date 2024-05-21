using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// A UI Component that draws animations<br/>
    /// </summary>
    class AnimatedUIComponent : UIComponent
    {
        /// <summary>
        /// The current animation<br/>
        /// </summary>
        public Animation _currentAnimation;
        /// <summary>
        /// The source rectangle of the current frame<br/>
        /// </summary>
        public Rectangle _currentFrame;
        /// <summary>
        /// The name of the current animation<br/>
        /// </summary>
        public string _currentAnimationState;

        /// <summary>
        /// The name of the next animation<br/>
        /// </summary>
        public string _nextAnimationState;
        /// <summary>
        /// The dictionary of animations from names<br/>
        /// </summary>
        public SortedDictionary<string, Animation> _animations =
        new SortedDictionary<string, Animation>();

        /// <summary>
        /// The time since the start of the animation (times fps)<br/>
        /// </summary>
        public double _animationTime = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedUIComponent"/> class.<br/>
        /// </summary>
        public AnimatedUIComponent() : base()
        {
        }

        //TODO: Make global variable for fps

        /// <summary>
        /// Updates which frame to draw<br/>
        /// Calls base.Update<br/>
        /// </summary>
        /// <param name="basePos"></param>
        public override void Update(Point basePos)
        {
            // Calculate the animation time
            _animationTime += Game.I._deltaTime * 60.0;

            //Check if the animation is over
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
            // Calculates the current frame
            _currentFrame = _currentAnimation.Frame((int)_animationTime);
            
            // Calls base.Update
            base.Update(basePos);
        }



        /// <summary>
        /// Draws the AnimatedUIComponent<br/>
        /// </summary>
        /// <param name="basePos"></param>
        public override void Draw(Point basePos)
        {
            //Sets the source rect to the current frame
            _sourceRect = _currentFrame;
            base.Draw(basePos);
        }

        /// <summary>
        /// Changes the animation playing<br/>
        /// Interrupts the current animation<br/>
        /// </summary>
        /// <param name="stateName"> The name of the animation to play</param>
        /// <exception cref="System.ArgumentException"> There is no animation with the given name</exception>
        public void ChangeAnimationState(string stateName)
        {
            // Check if the animation exists
            if (!_animations.ContainsKey(stateName))
            {
                throw new System.ArgumentException("There is no animation called", stateName);
            }
            // Sets the current animation
            _currentAnimationState = stateName;
            _nextAnimationState = stateName;
            _currentAnimation = _animations[stateName].Clone() as Animation;
            // Resets the animation time
            _animationTime = 0;
        }

        /// <summary>
        /// Changes the animation playing<br/>
        /// Doesn't interrupt the current animation,<br/>
        /// schedules the animation to play after the current finishes<br/>
        /// </summary>
        /// <param name="stateName"> The name of the animation to play</param>
        /// <exception cref="System.ArgumentException"> There is no animation with the given name</exception>
        public void ChangeNextAnimationState(string stateName)
        {
            // Check if the animation exists
            if (!_animations.ContainsKey(stateName))
            {
                throw new System.ArgumentException("There is no animation called: ", stateName);
            }
            // Sets the next animation
            _nextAnimationState = stateName;
        }
    }
}
