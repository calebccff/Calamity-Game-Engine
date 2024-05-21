using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using TestShader;

namespace TestShader
{
    /// <summary>
    /// Component that moves the Character<br/>
    /// A character should only be moved by its active mover<br/>
    /// All other components should move the mover<br/>
    /// This component only handles moving anc collisions<br/>
    /// Speed and friction are handled by FrictionCharacterComponent<br/>
    /// </summary>
    [MoverTag()]
    public class Mover : CharacterComponent
    {

        /// <summary>
        /// Action to be called when a collision occurs<br/>
        /// </summary>
        /// <param name="amount"></param>
        public delegate void Action(float amount);

        /// <summary>
        /// If true, the mover will not check for collisions<br/>
        /// </summary>
        public bool disableCollision = false;

        /// <summary>
        /// Creates a new Mover<br/>
        /// Sets the character's active mover to this<br/>
        /// </summary>
        /// <param name="character"> The character to attach the mover to</param>
        public Mover(Character character) : base(character)
        {
            character.Mover = this;
        }

        /// <summary>
        /// LoadContent,<br/>
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();
        }

        /// <summary>
        /// Checks if a Point is legal for this mover to be in<br/>
        /// Handles disabled collisions<br/>
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Legal(Point point)
        {
            if (disableCollision) return true;
            else
            {
                return _Legal(point);
            }
        }

        /// <summary>
        /// Checks if a Point is legal for this mover to be in<br/>
        /// Override this function to implement custom logic<br/>
        /// Returns true by default<br/>
        /// </summary>
        /// <param name="point"> The point to check</param>
        /// <returns> True if the point is a possible position for the mover</returns>
        public virtual bool _Legal(Point point)
        {
            return true;
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
        /// Dispose<br/>
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// UnsafeDispose,<br/>
        /// Removes the mover from the character's active mover<br/>
        /// </summary>
        public override void UnsafeDispose()
        {
            character.Mover = null;
            base.UnsafeDispose();
        }

        /// <summary>
        /// Move the mover<br/>
        /// Overload for custom logic<br/>
        /// On collision, onCollide should be called<br/>
        /// refers to Move(int,int)<br/>
        /// </summary>
        /// <param name="difference"> The difference to move</param>
        /// <param name="onCollide"> The action to perform when the mover collides</param>
        public virtual void Move(Vector2 difference, Action onCollide = null)
        {
            Move(difference.X, difference.Y, onCollide);
        }

        /// <summary>
        /// Move the mover<br/>
        /// Overload for custom logic<br/>
        /// On collision, onCollide should be called<br/>
        /// refers to Move(int,int)<br/>
        /// </summary>
        /// <param name="move"> The difference to move</param>
        /// <param name="onCollide"> The action to perform when the mover collides</param>
        public virtual void Move(Point move, Action onCollide = null)
        {
            Move(move.X, move.Y, onCollide);
        }

        /// <summary>
        /// Move the mover in direction x with the amount <paramref name="amount"/><br/>
        /// Overload for custom logic<br/>
        /// On collision, onCollide should be called<br/>
        /// </summary>
        /// <param name="amount"> The amount to move </param>
        /// <param name="onCollide"> The action to perform when the mover collides</param>
        public virtual void MoveX(float amount, Action onCollide = null)
        {
            _position = new Point((int)(_position.X+amount), (int)_position.Y);
        }

        /// <summary>
        /// Move the mover in direction y with the amount <paramref name="amount"/><br/>
        /// Overload for custom logic<br/>
        /// On collision, onCollide should be called<br/>
        /// </summary>
        /// <param name="amount"> The amount to move</param>
        /// <param name="onCollide"> The action to perform when the mover collides </param>
        public virtual void MoveY(float amount, Action onCollide = null)
        {
            _position = new Point((int)_position.X, (int)(_position.Y+amount));
        }

        /// <summary>
        /// Moves the mover in direction x and y with the amounts <paramref name="x"/> and <paramref name="y"/><br/>
        /// Overload for custom logic<br/>
        /// On collision, onCollide should be called<br/>
        /// Refers first to MoveX and then MoveY for moving<br/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="onCollide"></param>
        public virtual void Move(float x, float y, Action onCollide = null)
        {
            MoveX(x, onCollide);
            MoveY(y, onCollide);
        }


    }
}
