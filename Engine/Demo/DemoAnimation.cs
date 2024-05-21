using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace TestShader
{
    /// <summary>
    /// An example of an animated game component playing music when something collides with it's colliderSensor<br/>
    /// This implementation is outdated. Characters would be better way to implement this. See <seealso cref="DemoSolidCharacter"/><br/>
    /// </summary>
    class DemoAnimation : AnimatedGameComponent
    {
        /// <summary>
        /// The colliderSensor for the class<br/>
        /// </summary>
        public ColliderSensor _demoFish;

        /// <summary>
        /// AudioEmitter for the class<br/>
        /// </summary>
        AudioEmitter _emitter = new AudioEmitter();

        /// <summary>
        /// The music instance the class is playing<br/>
        /// </summary>
        AudioInstance Music;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoBall"/> class.<br/>
        /// It intializes the music<br/>
        /// </summary>
        /// <param name="texture"> The sprite sheet.</param>
        /// <param name="x_count"> The number of frames in the sprite sheet in the x direction</param>
        /// <param name="y_count"> The number of frames in the sprite sheet in the y direction</param>
        /// <param name="shader"> The shader used to draw the sprite</param>
        public DemoAnimation(Texture2D texture, int x_count, int y_count, Effect? shader = null) : base(texture)
        {

            _animations.Add("Default", new Animation(Tools.SplitTileSheet(texture.Width / x_count, texture.Height / y_count, x_count, y_count)) { _animationStepSpeed = 4 });
            ChangeAnimationState("Default");
            _shader = shader;
            _demoFish = new ColliderSensor(this);
            _demoFish.AddHitbox(new Rectangle(new Point((int)_position.X - (int)texture.Width / x_count / 2, (int)_position.Y - (int)texture.Height / y_count / 2), new Point(texture.Width / x_count, texture.Height / y_count)));

            //Initialize the music
            Music = AudioManager.GetAudioInstance("SleepAway");
        }


        /// <summary>
        /// Update,<br/>
        /// Sets the position of the collider and plays the music if the object is colliding<br/>
        /// It stops the music if the object is not colliding<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            _demoFish.setPosition(_position);
            // TODO: Figure out 3d sound
            if (_demoFish._isColliding)
            {
                _position.X -= 1;
                Music.Play();
            }
            else Music.Stop();
    
            base.Update(gameTime);
        }

    }
}
