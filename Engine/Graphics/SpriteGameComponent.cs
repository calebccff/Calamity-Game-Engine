using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace TestShader
{
    /// <summary>
    /// A component that draws a sprite on the screen<br/>
    /// </summary>
    public class SpriteGameComponent : DrawableComponent
    {
        /// <summary>
        /// The texture of the sprite<br/>
        /// </summary>
        public Texture2D _texture;

        /// <summary>
        /// The default color to draw the sprite with<br/>
        /// </summary>
        public Color _color = Color.White;

        /// <summary>
        /// The frame of the sprite in the texture<br/>
        /// </summary>
        public Rectangle _spriteFrame;

        /// <summary>
        /// The depth to draw the sprite at<br/>
        /// </summary>
        public float _layerDepth = 0.3f;

        /// <summary>
        /// The rotation to draw the sprite with<br/>
        /// </summary>
        public float _rotation = 0f;

        /// <summary>
        /// The origin of rotations applied to the sprite<br/>
        /// </summary>
        public Vector2 _origin;

        /// <summary>
        /// The scale to draw the sprite with<br/>
        /// </summary>
        public Vector2 _scale = new Vector2(1, 1);

        /// <summary>
        /// Whether or not the sprite should be flipped or not<br/>
        /// </summary>
        public SpriteEffects _spriteEffect = SpriteEffects.None;

        /// <summary>
        /// The corresponding sprite renderer<br/>
        /// </summary>
        public SpriteRenderer _spriteRenderer;

        /// <summary>
        /// The shader to apply when drawing the sprite<br/>
        /// </summary>
        public Effect _shader = null;

        /// <summary>
        /// The width of the sprite<br/>
        /// </summary>
        public int _width = -1;

        /// <summary>
        /// The height of the sprite<br/>
        /// </summary>
        public int _height = -1;


        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteGameComponent"/> class<br/>
        /// Use the <paramref name="texture"/> as the texture for the sprite<br/>
        /// Sets the texture, sprite renderer, width, height, spriteframe and origin of the sprite<br/>
        /// </summary>
        /// <param name="texture"></param>
        public SpriteGameComponent(Texture2D texture) : base()
        {
            _texture = texture;
            _spriteFrame = new Rectangle
            {
                Height = _texture.Height,
                Width = _texture.Width,
                Location = new Point(0, 0)
            };

            _origin.X = _spriteFrame.Width / 2;
            _origin.Y = _spriteFrame.Height / 2;


            _spriteRenderer = Game.I._spriteRenderer;
            _width = _spriteFrame.Width;
            _height = _spriteFrame.Height;
        }

        /// <summary>
        /// LoadContent,<br/>
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
        }

        /// <summary>
        /// Update<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// Draw<br/>
        /// Draws the sprite to the screen when visible<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            // Test if the sprite should be drawn
            if (_visible)
            {
                // Calculate the position of the sprite from if it is flipped and scaled
                Vector2 scaledPosition = new Vector2(_position.X, _position.Y);
                Vector2 origin = _origin;
                if (_spriteEffect == SpriteEffects.FlipVertically) { origin.Y = _spriteFrame.Height - origin.Y; }
                if (_spriteEffect == SpriteEffects.FlipHorizontally) { origin.X = _spriteFrame.Width - origin.X; }

                // Draw the sprite
                _spriteRenderer.Draw(_texture, scaledPosition, _spriteFrame, _color, _rotation, origin, new Vector2(_scale.X, _scale.Y), _spriteEffect, _layerDepth, _shader);
            }
            base.Draw(gameTime);
        }

        /// <summary>
        /// The X coordinate of the position of the sprite<br/>
        /// </summary>
        public int X
        {
            get
            {
                return _position.X;
            }
        }

        /// <summary>
        /// The Y coordinate of the position of the sprite<br/>
        /// </summary>
        public int Y
        {
            get
            {
                return _position.Y;
            }
        }
    }
}
