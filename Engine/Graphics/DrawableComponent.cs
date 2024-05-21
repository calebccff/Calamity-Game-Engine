using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// Represents a component that can be drawn<br/>
    /// Adds a Draw method called each frame by the game<br/>
    /// </summary>
    public class DrawableComponent : Component
    {
        /// <summary>
        /// Whether the component should be drawn<br/>
        /// </summary>
        public bool _visible = true;

        //TODO: Implement this with Event listeners instead

        /// <summary>
        /// An action that is called when the component is drawn<br/>
        /// </summary>
        Action<DrawableComponent, GameTime> _onDraw = Actions.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawableComponent"/> class.<br/>
        /// </summary>
        public DrawableComponent() : base()
        {

        }

        /// <summary>
        /// Draw,<br/>
        /// A function called each frame by the game for drawing<br/>
        /// Calls the action _onDraw<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime gameTime)
        {
            if (_onDraw != null) _onDraw(this, gameTime);
        }


        //TODO: Rework even listeners somehow

        /// <summary>
        /// Adds an action to be called on draw<br/>
        /// </summary>
        /// <param name="action"></param>
        public void AddActionOnDraw(Action action) => _onDraw+=(new Action<Component, GameTime>((a, b) => action()));

        /// <summary>
        /// Adds an action to be called on draw<br/>
        /// </summary>
        /// <param name="action"></param>
        public void AddActionOnDraw(Action<Component> action) => _onDraw += (new Action<Component, GameTime>((a, b) => action(a)));

        /// <summary>
        /// Adds an action to be called on draw<br/>
        /// </summary>
        /// <param name="action"></param>
        public void AddActionOnDraw(Action<GameTime> action) => _onDraw += (new Action<Component, GameTime>((a, b) => action(b)));

        /// <summary>
        /// Adds an action to be called on draw<br/>
        /// </summary>
        /// <param name="action"></param>
        public void AddActionOnDraw(Action<Component, GameTime> action) => _onDraw+=(action);
    }
}
