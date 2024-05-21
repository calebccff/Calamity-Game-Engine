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
    /// Old example class creating an animatedComponent with a hitbox<br/>
    /// </summary>
    class DemoObstacle : AnimatedGameComponent
    {
        /// <summary>
        /// The hitbox for the component<br/>
        /// </summary>
        public Collider _demoFish;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoObstacle"/> class<br/>
        /// </summary>
        /// <param name="texture"> The sprite sheet</param>
        /// <param name="x_count"> The number of sprites in the x direction</param>
        /// <param name="y_count"> The number of sprites in the y direction</param>
        /// <param name="shader"> The shader used to draw the sprite</param>
        public DemoObstacle(Texture2D texture, int x_count, int y_count, Effect? shader = null) : base(texture)
        {

            //Create the animations from the sprite sheet
            _animations.Add("Default", new Animation(Tools.SplitTileSheet(texture.Width / x_count, texture.Height / y_count, x_count, y_count)) { _animationStepSpeed = 4 });
            
            //Set the animation to play
            ChangeAnimationState("Default");

            _shader = shader;
            //Create Collider for component and set it's hitbox
            _demoFish = new Collider(this);
            _demoFish.AddHitbox(new Rectangle(new Point((int)_position.X - (int)texture.Width / x_count / 2, (int)_position.Y - (int)texture.Height / y_count / 2), new Point(texture.Width / x_count, texture.Height / y_count)));

        }

        /// <summary>
        /// Update,<br/>
        /// Updates the position of the collider<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            _demoFish.setPosition(_position);

            base.Update(gameTime);
        }

    }
}
