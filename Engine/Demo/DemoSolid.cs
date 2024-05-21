using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// An example of a solid game component<br/>
    /// It has an animated sprite<br/>
    /// This implementation is outdated. Characters would be better way to implement this. See <seealso cref="DemoSolidCharacter"/><br/>
    /// </summary>
    class DemoSolid : Solid
    {
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
        /// The animated sprite corresponding to this solid (Private)<br/>
        /// </summary>
        public AnimatedGameComponent _privateAnimatedComponent = null;

        /// <summary>
        /// The animated sprite corresponding to this solid<br/>
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
        /// Initializes a new instance of the <see cref="DemoSolid"/> class<br/>
        /// </summary>
        /// <param name="texture">The sprite sheet</param>
        /// <param name="x_count">The number of sprites in the x direction</param>
        /// <param name="y_count"> The number of sprites in the y direction</param>
        /// <param name="shader">The shader used to draw the sprite (if null use none)</param>
        public DemoSolid(Texture2D texture, int x_count, int y_count, Effect? shader = null) : base(texture)
        {
            _shader = shader;
            _xCount = x_count;
            _yCount = y_count;
            _shader = shader;

            //Add the collider matching the sprite size
            _colliderSensor.AddHitbox(new Rectangle(new Point(-texture.Width / x_count / 2 * 4, -texture.Height / y_count / 2 * 4), new Point(texture.Width / x_count * 4, texture.Height / y_count * 4)));
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
            _animatedComponent._animations.Add("Default", new Animation(Tools.SplitTileSheet(_texture.Width / _xCount, _texture.Height / _yCount, _xCount, _yCount)) { _animationStepSpeed = 4 });
            _animatedComponent.ChangeAnimationState("Default");
            _animatedComponent._shader = _shader;
            _animatedComponent._scale = new Vector2(4, 4);
            base.LoadContent();
        }

        /// <summary>
        /// Update,<br/>
        /// Moves the sprite back and forth in the x direction<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Move(Math.Sign(Math.Sin(gameTime.TotalGameTime.TotalSeconds)), 0);
            base.Update(gameTime);
        }
    }
}
