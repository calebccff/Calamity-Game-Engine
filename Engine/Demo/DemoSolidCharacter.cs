using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestShader
{

    /// <summary>
    /// An example of a solid game component implemented as a character<br/>
    /// </summary>
    class DemoSolidCharacter : Character
    {
        /// <summary>
        /// The number of frames in the sprite sheet in the x direction<br/>
        /// </summary>
        int _xCount;

        /// <summary>
        /// The number of frames in the sprite sheet in the y direction<br/>
        /// </summary>
        int _yCount;

        /// <summary>
        /// The shader used to draw the sprite<br/>
        /// </summary>
        Effect? _shader;

        /// <summary>
        /// The states that the character can be in (Used in <see cref="StateAnimatedCharacterComponent"/>)<br/>
        /// </summary>
        public override List<string> States { get; } = new List<string>() { "Default" };

        /// <summary>
        /// The animated character component depending only on the state<br/>
        /// </summary>
        public StateAnimatedCharacterComponent _animatedComponent;

        /// <summary>
        /// The solid mover character class handling movement<br/>
        /// </summary>
        public SolidMover _solidComponent;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoSolidCharacter"/> class<br/>
        /// </summary>
        /// <param name="texture"> The sprite sheet</param>
        /// <param name="x_count"> The number of sprites in the x direction</param>
        /// <param name="y_count"> The number of sprites in the y direction</param>
        /// <param name="shader"> The shader used to draw the sprite</param>
        public DemoSolidCharacter(Texture2D texture, int x_count, int y_count, Effect? shader = null) : base()
        {
            _shader = shader;
            _xCount = x_count;
            _yCount = y_count;

            //Initialize the components
            _solidComponent = new SolidMover(this);
            _animatedComponent = new StateAnimatedCharacterComponent(texture, this);

            //Add the hitboxes
            _solidComponent.Hitbox.AddHitbox(new Rectangle(new Point(-texture.Width / x_count / 2 * 4+4, -texture.Height / y_count / 2 * 4+16), new Point(texture.Width / x_count * 4*14/16, texture.Height / y_count * 4*12/16)));
        }

        /// <summary>
        /// LoadContent,<br/>
        /// Creates the animation from sprite sheet and adds it to the <see cref="AnimatedCharacterComponent"/><br/>
        /// </summary>
        protected override void LoadContent()
        {

            base.LoadContent();//Need to load components before such actions
            _animatedComponent._anim._animations.Add("Default", new Animation(Tools.SplitTileSheet(_animatedComponent._anim._texture.Width / _xCount, _animatedComponent._anim._texture.Height / _yCount, _xCount, _yCount).GetRange(15,3)) { _animationStepSpeed = 16 });
            _animatedComponent._anim.ChangeAnimationState("Default");
            _animatedComponent._anim._shader = _shader;
            _animatedComponent._anim._scale = new Vector2(4, 4);
        }

        /// <summary>
        /// Update,<br/>
        /// Moves the character alternatingly in the x direction<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            _solidComponent.Move(Math.Sign(Math.Sin(gameTime.TotalGameTime.TotalSeconds)), 0);
            base.Update(gameTime);
        }
    }
}
