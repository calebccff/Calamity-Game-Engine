using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TestShader;

namespace TestShader
{
    /// <summary>
    /// A CharacterComponent sharing functionality with AnimatedGameComponent<br/>
    /// It attaches an animated sprite to a character drawn each frame<br/>
    /// </summary>
    public class AnimatedCharacterComponent : SpriteCharacterComponent
    {
        // Lifting variables from AnimatedGameComponent for easier access,
        #region Accesors
        public ref Animation _currentAnimation { get => ref _anim._currentAnimation; }
        public ref Rectangle _currentFrame { get => ref _anim._currentFrame; }
        public ref string _currentAnimationState { get => ref _anim._currentAnimationState; }
        public ref string _nextAnimationState { get => ref _anim._nextAnimationState; }
        public ref SortedDictionary<string, Animation> _animations { get => ref _anim._animations; }
        public ref double _animationTime { get => ref _anim._animationTime; }

        #endregion Accesors


        //TODO: Why do I allow setting this multiple times, while not in SpriteCharacterComponent?

        /// <summary>
        /// The animated sprite<br/>
        /// Accesses the corresponding sprite as an AnimatedGameComponent<br/>
        /// </summary>
        public AnimatedGameComponent _anim
        {
            get
            {
                return (_sprite as AnimatedGameComponent);
            }
            set { _sprite = value; }
        }


        /// <summary>
        /// Constructor,<br/>
        /// Sets the initial texture and the corresponding character<br/>
        /// </summary>
        /// <param name="texture"> The initial texture of the sprite</param>
        /// <param name="character"> The character to attach the sprite to</param>
        public AnimatedCharacterComponent(Texture2D texture, Character character) : base(texture, character)
        {


        }

        /// <summary>
        /// LoadContent,<br/>
        /// Creates the sprite if it doesn't exist yet<br/>
        /// Loads the content of the sprite if it has not been loaded yet<br/>
        /// </summary>
        public override void LoadContent()
        {
            if (_sprite == null) _sprite = new AnimatedGameComponent(initTexture);
            base.LoadContent();
        }

        /// <summary>
        /// Update,<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// Draw,<br/>
        /// (AnimatedGameComponent's Draw is handled independently by Game as a DrawableComponent)<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        /// <summary>
        /// Dispose,<br/>
        /// </summary>
        public override void Dispose()
        {
        
            base.Dispose();
        }
    }
}
