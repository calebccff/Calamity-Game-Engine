using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// A class containing the bounding boxes for each frame of an animation<br/>
    /// Along with the animation speed<br/>
    /// </summary>
    public class Animation : ICloneable
    {
        /// <summary>
        /// The animation frames on a spriteSheet<br/>
        /// </summary>
        public List<Rectangle> _sourceRectangles;
        
        //TODO: Animation dynamic origin
        /// <summary>
        /// The origin for each frame of the animation<br/>
        /// </summary>
        public Vector2 _origin = new Vector2(0, 0);
        
        /// <summary>
        /// The speed of the animation to be played at<br/>
        /// The animation changes frame every time _animationStepSpeed ticks pass<br/>
        /// </summary>
        public int _animationStepSpeed = 1;
        
        /// <summary>
        /// Creates a new animation from a list of rectangles<br/>
        /// </summary>
        public Animation(List<Rectangle> sourceRectangles)
        {
            _sourceRectangles = sourceRectangles;
            _origin = new Vector2(sourceRectangles[0].Width / 2, sourceRectangles[0].Height / 2);
        }

        /// <summary>
        /// Returns the number of frames in the animation<br/>
        /// </summary>
        public int Length()
        {
            return _sourceRectangles.Count;
        }

        /// <summary>
        /// Clones the animation<br/>
        /// </summary>
        public Object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Gets the frame of the animation<br/>
        /// </summary>
        /// <param name="_animationTime">The time since the animation started as an integer</param>
        /// <returns> The frame of the animation as a rectangle on the sprite sheet </returns>
        public Rectangle Frame(int _animationTime)
        {
            return _sourceRectangles[_animationTime / _animationStepSpeed];
        }
    }
}
