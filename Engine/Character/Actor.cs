using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace TestShader
{
    // TODO: EXTRACT ENTITY FROM ACTOR AND GET PLAYER INHERIT UNIT INHERIT ENTITY
    // TODO: This should be reworked as a "Mover" class and _sprite removed from it. It shouldn't inherit Squish should be an Action delegate

    /// <summary>
    /// Component that moves as a dynamic collider with a sprite attached to it<br/>
    /// </summary>
    public class Actor : Component
    {
        /// <summary>
        /// The offset of the sprite compared to the center of the actor
        /// </summary>
        public Point spriteOffset = new Point(0, 0);

        private Point _positionPrivate;
        /// <summary>
        /// The position of the actor, setting this will also move the sprite and collider<br/>
        /// </summary>
        public Point Position
        {
            get
            {
                return _positionPrivate;
            }
            set
            {
                _positionPrivate = value;
                _position = value;
                if (_sprite != null)
                    _sprite._position = value+spriteOffset;
                if (_colliderSensor != null)
                    _colliderSensor._position = value;
            }
        }

        public SpriteGameComponent _privateSprite = null;
        /// <summary>
        /// The sprite of the actor, can be only set once<br/>
        /// </summary>
        public SpriteGameComponent _sprite
        {
            get => _privateSprite;
            set
            {
                if (_privateSprite == null)
                {
                    _privateSprite = value; return;
                }
                if (value == _privateSprite) return; throw new Exception("Trying to reset _sprite");
            }
        }

        /// <summary>
        /// The collider of the actor<br/>
        /// </summary>
        public ColliderSensor _colliderSensor;
        /// <summary>
        /// The action to perform when the actor collides<br/>
        /// </summary>
        /// <param name="amount"></param>
        public delegate void Action(float amount);

        /// <summary>
        /// The delegate that checks if the actor should collide with another collider<br/>
        /// </summary>
        public ColliderSensor.ShouldCollideCheck _sCC;
        /// <summary>
        /// The texture of the actor<br/>
        /// </summary>
        public Texture2D _texture;
        /// <summary>
        /// The width of the actor<br/>
        /// </summary>
        public int _width = 0;
        /// <summary>
        /// The height of the actor<br/>
        /// </summary>
        public int _height = 0;



        /// <summary>
        /// Creates a new actor<br/>
        /// </summary>
        /// <param name="texture"> The texture of the actor</param>
        public Actor(Texture2D texture) : base()
        {
            // Set the texture
            _texture = texture;

            // Create a new collider and add the tag "Actor" to the collider
            _colliderSensor = new ColliderSensor(this);
            _colliderSensor._tags.Add("Actor");

            // Create delagate for shouldCollideCheck that only checks for solids
            _sCC = new ColliderSensor.ShouldCollideCheck(onlySolids);
            _colliderSensor._sCC = _sCC;

            // Add the actor to the list of actors
            Game.I._actors.Add(this);
        }

        /// <summary>
        /// LoadsContent,<br/>
        /// sets the position of the sprite and the collider<br/>
        /// and sets the width and height<br/>
        /// If _sprite is null, it creates a new sprite (Not expected to happen)<br/>
        /// </summary>
        protected override void LoadContent()
        {
            // Set the position of the the collider
            _colliderSensor._position = Position;

            // If _sprite is null, it creates a new sprite (Not expected to happen)
            if (_sprite == null) { _sprite = new SpriteGameComponent(_texture); Debug.WriteLine("error"); }

            // Set the dimensions of the sprite
            _width = _sprite._width;
            _height = _sprite._height;

            base.LoadContent();
        }

        /// <summary>
        /// Update,<br/>
        /// sets the position, width and height of the sprite and the collider<br/>
        /// </summary>
        /// <param name="gameTime"> The game time</param>
        public override void Update(GameTime gameTime)
        {
            _sprite._position = Position+spriteOffset;
            _width = _sprite._width;
            _height = _sprite._height;
            _colliderSensor._position = Position;

            base.Update(gameTime);
        }


        /// <summary>
        /// Moves the Actor in small increments, to avoid jitter in collision detection<br/>
        /// </summary>
        /// <param name="difference"></param>
        public void Move(Point difference)
        {
            // Calculate the number of increments to move
            float factor = (float)(Math.Max(difference.X, difference.Y) / 5.0);

            // If the factor is less than 1, move the actor once
            if (factor < 1)
            {
                MoveY(difference.Y);
                MoveX(difference.X);
                return;
            }

            // Calculate the vector of the increments
            Point smallDifference = new Point((int)(difference.X / factor), (int)(difference.Y / factor));

            // Move the actor in increments
            if (smallDifference.X != 0 || smallDifference.Y != 0)
            {
                for (int index = 0; index < factor; index++)
                {
                    MoveY(smallDifference.Y);
                    MoveX(smallDifference.X);
                }
            }

            // Calculate the remaining difference
            Point remainingDifference = difference - new Point((int)(smallDifference.X * (float)(Math.Floor(factor))), (int)(smallDifference.Y * (float)(Math.Floor(factor))));

            // Move the actor with the remaining difference
            MoveY(remainingDifference.Y);
            MoveX(remainingDifference.X);
        }


        /// <summary>
        /// Tracks the remainder of the x movement<br/>
        /// </summary>
        float _xRemainder = 0;
        /// <summary>
        /// Moves the Actor in direction x, checking for collisions<br/>
        /// </summary>
        /// <param name="amount"> The amount to move</param>
        /// <param name="onCollide"> The action to perform when the actor collides</param>
        public void MoveX(float amount, Action onCollide = null)
        {

            // Add the amount to the remainder to move
            _xRemainder += amount;

            // Get the integer part of the remainder
            int move = (int)Math.Floor(_xRemainder);

            if (move != 0)
            {
                // Calculate the remainder
                _xRemainder -= move;

                // Find the sign of the move
                int sign = Math.Sign(move);

                while (move != 0)
                {
                    // Check if there is no Solid immediately front of the actor in the direction of the move
                    if (!_colliderSensor.CollideAt(Position + new Point(sign, 0), onlySolids)._isColliding)
                    {
                        // Move the actor 1 pixel in the direction of the move
                        Position = Position + new Point(sign, 0);

                        // Reduce the remaining move
                        move -= sign;
                    }
                    else
                    {
                        //If there is a solid blocking the actor, stop moving
                        move = 0;

                        //Trigger the onCollide action
                        if (onCollide != null)
                            onCollide(amount);

                        break;
                    }
                }
            }
            // Move the collider to the new position
            _colliderSensor._position = Position;
        }


        /// <summary>
        /// Tracks the remainder of the y movement<br/>
        /// </summary>
        float _yRemainder = 0;
        /// <summary>
        /// Moves the Actor in direction y, checking for collisions<br/>
        /// </summary>
        /// <param name="amount"> The amount to move</param>
        /// <param name="onCollide"> The action to perform when the actor collides</param>
        public void MoveY(float amount, Action onCollide = null)
        {
            // Add the amount to the remainder to move
            _yRemainder += amount;

            // Get the integer part of the remainder
            int move = (int)Math.Floor(_yRemainder);


            if (move != 0)
            {
                // Calculate the new remainder
                _yRemainder -= move;

                // Find the sign of the move
                int sign = Math.Sign(move);

                while (move != 0)
                {
                    //There is no Solid immediately beside us 
                    //If we are falling, check if there is a jumpthrough
                    if (!_colliderSensor.CollideAt(Position + new Point(0, sign), onlySolids)._isColliding && (move<0 || !_colliderSensor.CollideAt(Position + new Point(0, sign), onlyJumpThroughs)._isColliding))
                    {
                        
                        // Move the actor
                        Position = Position + new Point(0, sign);

                        
                        // Reduce the remaining move
                        move -= sign;
                    }
                    else
                    {
                        // If there is a solid blocking the actor, stop moving
                        move = 0;

                        // Trigger the onCollide action
                        if (onCollide != null)
                            onCollide(amount);
                        break;
                    }
                }
            }
            // Move the collider to the new position
            _colliderSensor._position = Position;
        }


        /// <summary>
        /// Checks if the actor is riding a solid<br/>
        /// </summary>
        /// <param name="solid"> The solid to check</param>
        /// <returns> True if the actor is riding the solid</returns>
        public virtual bool IsRiding(Solid solid)
        {
            // Check if the solid's collider is the same as the actor's.
            // An Actor cannot be riding itself
            if (solid._colliderSensor._id == _colliderSensor._id) return false;

            // Check if the solid's collider is active
            if (solid._colliderSensor._active == false) return false;

            bool intersect = false;

            // Check if any of the actor's hitboxes intersect with any of "solid"'s hitboxes.
            foreach (Rectangle rect in solid._colliderSensor._hitboxes)
            {
                // Offset the hitbox by the "solid"'s position
                Rectangle newRect1 = rect;
                newRect1.Offset(solid._colliderSensor._position);

                // Check if "newRect1" intersects with any of "solid"'s hitboxes.
                foreach (Rectangle rect2 in _colliderSensor._hitboxes)
                {
                    // Offset the hitbox by the actor's position.
                    Rectangle newRect2 = rect2;
                    newRect2.Offset(Position);

                    // Check if the two hitboxes intersect.
                    bool original = newRect1.Intersects(newRect2);

                    // Try again with an offset of one unit in the y-direction.
                    newRect2.Offset(new Point(0, 1));

                    if (!original && newRect1.Intersects(newRect2))
                    {
                        // If the two boxes only intersect after the y-offset, there is an intersection.
                        intersect = true;
                        break;
                    }
                }
                if (intersect) break;
            }

            return intersect;
        }

        /// <summary>
        /// Checks if the actor is riding a SolidMover<br/>
        /// </summary>
        /// <param name="solid"> The SolidMover to check</param>
        /// <returns> True if the actor is riding the SolidMover</returns>
        public virtual bool IsRiding(SolidMover solid)
        {
            // Check if the solid's collider is the same as the actor's.
            // An Actor cannot be riding itself
            if (solid.Hitbox._id == _colliderSensor._id) return false;

            // Check if the solid's collider is active
            if (solid.Hitbox._active == false) return false;

            bool intersect = false;

            // Check if any of the actor's hitboxes intersect with any of "solid"'s hitboxes.
            foreach (Rectangle rect in solid.Hitbox._hitboxes)
            {
                // Offset the hitbox by the "solid"'s position
                Rectangle newRect1 = rect;
                newRect1.Offset(solid.Hitbox._position);

                // Check if "newRect1" intersects with any of "solid"'s hitboxes.
                foreach (Rectangle rect2 in _colliderSensor._hitboxes)
                {
                    // Offset the hitbox by the actor's position.
                    Rectangle newRect2 = rect2;
                    newRect2.Offset(Position);

                    // Check if the two hitboxes intersect.
                    bool original = newRect1.Intersects(newRect2);

                    // Try again with an offset of one unit in the y-direction.
                    newRect2.Offset(new Point(0, 1));

                    if (!original && newRect1.Intersects(newRect2))
                    {
                        // If the two boxes only intersect after the y-offset, there is an intersection.
                        intersect = true;
                        break;
                    }
                }
                if (intersect) break;
            }

            return intersect;
        }

        /// <summary>
        /// Checks if the actor is riding a JumpThrough<br/>
        /// </summary>
        /// <param name="jumpt"> The JumpThrough to check</param>
        /// <returns> True if the actor is riding the JumpThrough</returns>
        public virtual bool IsRiding(JumpThrough jumpt)
        {
            // Check if the jumpthrough's collider sensor is the same as the actor's.
            // An Actor cannot be riding itself.
            if (jumpt._colliderSensor._id == _colliderSensor._id) return false;

            // Check if the jumpthrough's collider sensor is active.
            if (jumpt._colliderSensor._active == false) return false;


            bool intersect = false;

            // Check if any of the actor's hitboxes intersect with any of "jumpt"'s hitboxes.
            foreach (Rectangle rect in jumpt._colliderSensor._hitboxes)
            {

                // Offset the hitbox by "jumpt"'s position.
                Rectangle newRect1 = rect;
                newRect1.Offset(jumpt._colliderSensor._position);

                // Check if "newRect1" intersects with any of "jumpt"'s hitboxes.
                foreach (Rectangle rect2 in _colliderSensor._hitboxes)
                {
                    // Offset the hitbox by the actor's position.
                    Rectangle newRect2 = rect2;
                    newRect2.Offset(Position);

                    // Check if the two hitboxes intersect.
                    bool original = newRect1.Intersects(newRect2);

                    // Try again with an offset of one unit in the y-direction.
                    newRect2.Offset(new Point(0, 1));

                    if (!original && newRect1.Intersects(newRect2))
                    {
                        // If the two boxes only intersect after the y-offset, there is an intersection.
                        intersect = true;
                        break;
                    }
                }

                // If an intersection is found, break out of the loop.
                if (intersect) break;
            }

            // Return the result of the check.
            return intersect;
        }

        /// <summary>
        /// Function called when the actor is squished<br/>
        /// Default implementation destroys the actor<br/>
        /// </summary>
        /// <param name="amount"> The amount of squish</param>
        public virtual void Squish(float amount)
        {
            Dispose();
        }

        /// <summary>
        /// Function to create delagate for shouldCollideCheck that only checks for solids<br/>
        /// </summary>
        public bool onlySolids(ColliderSensor self, Collider other)
        {
            return other._tags.Contains("Solid");
        }

        /// <summary>
        /// Function to create delagate for shouldCollideCheck that only checks for jumpThroughs<br/>
        /// </summary>
        public bool onlyJumpThroughs(ColliderSensor self, Collider other)
        {
            return other._tags.Contains("JumpThrough");
        }

        /// <summary>
        /// Dispose,<br/>
        /// Disposes the sprite and the collider<br/>
        /// </summary>
        public override void Dispose()
        {
            if (!_sprite._isDisposed) _sprite.Dispose();
            if (!_colliderSensor._isDisposed) _colliderSensor.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// UnsafeDispose,<br/>
        /// Removes the actor from the list of actors<br/>
        /// </summary>
        public override void UnsafeDispose()
        {
            Game.I._actors.Remove(this);
            base.UnsafeDispose();
        }

    }
}
