using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// An example of an actor game component<br/>
    /// It has an animated sprite<br/>
    /// This implementation is outdated. Characters would be better way to implement this. See <seealso cref="DemoSolidCharacter"/><br/>
    /// </summary>
    class PocketBox : Solid
    {

        /// <summary>
        /// The animated sprite corresponding to this actor (Private)<br/>
        /// </summary>
        public AnimatedGameComponent _privateAnimatedComponent = null;

        /// <summary>
        /// The animated sprite corresponding to this actor<br/>
        /// Can only be set once<br/>
        /// </summary>
        public AnimatedGameComponent _animatedComponent
        {
            get => _privateAnimatedComponent;
            set
            {
                if (_privateAnimatedComponent == null)
                {
                    _privateAnimatedComponent = value; _sprite = value; return;
                }
                if (value == _privateAnimatedComponent) return; throw new Exception("Trying to reset _animatedComponent");
            }
        }

        /// <summary>
        /// The number of sprites in the x direction in the sprite sheet<br/>
        /// </summary>
        int _xCount;

        /// <summary>
        /// The number of sprites in the y direction in the sprite sheet<br/>
        /// </summary>
        int _yCount;

        /// <summary>
        /// The shader used to draw the sprite<br/>
        /// </summary>
        Effect _shader;

        /// <summary>
        /// Initializes a new instance of the <see cref="PocketBox"/> class.<br/>
        /// </summary>
        /// <param name="texture"> The sprite sheet.</param>
        /// <param name="x_count"> The number of frames in the sprite sheet in the x direction</param>
        /// <param name="y_count"> The number of frames in the sprite sheet in the y direction</param>
        /// <param name="shader"> The shader used to draw the sprite</param>
        public PocketBox(Texture2D texture, int x_count, int y_count, Effect? shader = null) : base(texture)
        {

            _xCount = x_count;
            _yCount = y_count;
            _shader = shader;

            //Add a hitbox matching the sprite size
            _colliderSensor.AddHitbox(new Rectangle(new Point(-(int)texture.Width / x_count / 2, -(int)texture.Height / y_count / 2), new Point(texture.Width / x_count, texture.Height / y_count)));

        }

        /// <summary>
        /// LoadContent<br/>
        /// Initializes the animated sprite if it is not already initialized<br/>
        /// Creates the animation to be played by this component<br/>
        /// </summary>
        protected override void LoadContent()
        {

            if (_animatedComponent == null) _animatedComponent = new AnimatedGameComponent(_texture);
            _sprite = _animatedComponent;
            _animatedComponent._animations.Add("Default", new Animation(Tools.SplitTileSheet(_texture.Width / _xCount, _texture.Height / _yCount, _xCount, _yCount).GetRange(5 * 32, 1)) { _animationStepSpeed = 4 });
            _animatedComponent.ChangeAnimationState("Default");
            _animatedComponent._shader = _shader;
            base.LoadContent();
        }

        /// <summary>
        /// Update,<br/>
        /// Moves the character to the mouse<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if(InputManager.Mouse.CurrentState.LeftButton == ButtonState.Pressed)
            Move(new Point((int)InputManager.Mouse.Position.X, (int)InputManager.Mouse.Position.Y) - Position);

            base.Update(gameTime);
        }

    }

}

