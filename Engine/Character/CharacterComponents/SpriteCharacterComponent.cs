using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TestShader;

namespace TestShader
{
    /// <summary>
    /// A character component that shares functionality with SpriteGameComponent<br/>
    /// It attaches a sprite to a character drawn each frame<br/>
    /// </summary>
    public class SpriteCharacterComponent : CharacterComponent
    {
        /// <summary>
        /// The sprite that is attached to the character. Private<br/>
        /// </summary>
        public SpriteGameComponent _privateSprite = null;
        /// <summary>
        /// The sprite that is attached to the character<br/>
        /// It can only be set once<br/>
        /// </summary>
        public SpriteGameComponent _sprite
        {
            get => _privateSprite;
            set
            {
                if (_privateSprite == null)
                {
                    _privateSprite = value;
                    _privateSprite._position = _position;
                    return;
                }
                if (value == _privateSprite) return; throw new Exception("Trying to reset _sprite");
            }
        }

        /// <summary>
        /// The texture of the sprite when initialized<br/>
        /// </summary>
        public Texture2D initTexture { get; }


        // 
        /// Lifting variables from SpriteGameComponent for easier access
        // 
        #region Accesors
        public ref Texture2D _texture { get => ref _sprite._texture; }
        public ref Color _color { get => ref _sprite._color; }
        public ref Rectangle _spriteFrame { get => ref _sprite._spriteFrame; }
        public ref float _layerDepth { get => ref _sprite._layerDepth; }
        public ref float _rotation { get => ref _sprite._rotation; }
        public ref Vector2 _origin { get => ref _sprite._origin; }
        public ref Vector2 _scale { get => ref _sprite._scale; }
        public ref SpriteEffects _spriteEffect { get => ref _sprite._spriteEffect; }
        public ref SpriteRenderer _spriteRenderer { get => ref _sprite._spriteRenderer; }
        public ref Effect _shader { get => ref _sprite._shader; }
        public ref int _width { get => ref _sprite._width; }
        public ref int _height { get => ref _sprite._height; }
        #endregion Accesors


        /// <summary>
        /// Constructor,<br/>
        /// Sets the initial texture and the corresponding character<br/>
        /// </summary>
        /// <param name="texture"> The initial texture of the sprite</param>
        /// <param name="character"> The character to attach the sprite to</param>
        public SpriteCharacterComponent(Texture2D texture, Character character) : base(character)
        {
            this.initTexture = texture;
        }

        /// <summary>
        /// Called when the corresponding component's position changes<br/>
        /// Updates the position of the sprite<br/>
        /// </summary>
        /// <param name="Old"> Old position of the component</param>
        /// <param name="New"> New Position of the component</param>
        public override void OnPositionChange(Point Old, Point New)
        {
            if (_sprite != null)
                _sprite._position = _position;
        }

        /// <summary>
        /// LoadContent,<br/>
        /// Creates the sprite if it doesn't exist yet<br/>
        /// Loads the content of the sprite if it has not been loaded yet<br/>
        /// </summary>
        public override void LoadContent()
        {
            if (_sprite == null) _sprite = new SpriteGameComponent(initTexture);
            _sprite.TryLoadContent();//It doesn't have a LoadContent rn, but it might later/overload
            base.LoadContent();
        }

        /// <summary>
        /// Update,<br/>
        /// Updates the sprite<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!_sprite._updated) _sprite.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// Draw,<br/>
        /// (The SpriteGameComponent's Draw is handled independently by Game as a DrawableComponent)<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        /// <summary>
        /// Dispose,<br/>
        /// Disposes the sprite as well<br/>
        /// </summary>
        public override void Dispose()
        {
            if (!_sprite._isDisposed) _sprite.Dispose();

            base.Dispose();
        }
    }
}
