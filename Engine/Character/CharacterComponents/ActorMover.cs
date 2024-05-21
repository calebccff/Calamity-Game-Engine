using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TestShader;

namespace TestShader
{
    /// <summary>
    /// Character Component that moves as a dynamic collider with a sprite attached to it<br/>
    /// </summary>
    public class ActorMover : Mover
    {
        /// <summary>
        /// The collider attached to the mover<br/>
        /// </summary>
        public ColliderSensor Hitbox;


        //TODO: think about implementing float value of amount for squish
        /// <summary>
        /// Function called when the actor is squished by a solid<br/>
        /// </summary>
        public Action Squish;

        /// <summary>
        /// The delegate that checks if the actor should collide with another collider<br/>
        /// </summary>
        public ColliderSensor.ShouldCollideCheck _sCC;






        /// <summary>
        /// Constructor<br/>
        /// Sets the Hitbox, adds the Actor tag, and sets the ShouldCollideCheck<br/>
        /// Sets Squish to dispose the character if empty<br/>
        /// Adds ActorMover to Game.I._actorMovers<br/>
        /// </summary>
        /// <param name="character"></param>
        public ActorMover(Character character) : base(character)
        {
            Hitbox = new ColliderSensor(this);
            Hitbox._tags.Add("Actor");
            _sCC = new ColliderSensor.ShouldCollideCheck(onlySolids);
            Hitbox._sCC = _sCC;
            Game.I._actorMovers.Add(this);
            if (Squish == null) { Squish = delegate (float amount) { Dispose(); character.Dispose(); }; }
            Hitbox._position = _position;
        }

        /// <summary>
        /// Called when the position of the component changes<br/>
        /// Updates the position of the Hitbox<br/>
        /// </summary>
        /// <param name="Old"> Old position of the component</param>
        /// <param name="New"> New position of the component</param>
        public override void OnPositionChange(Point Old, Point New)
        {
            Hitbox._position = New;
            base.OnPositionChange(Old, New);
        }




        /// <summary>
        /// Moves the Actor in small increments, to avoid jitter in collision detection<br/>
        /// On collision, calls <paramref name="onCollide"/><br/>
        /// </summary>
        /// <param name="difference"> The difference to move</param>
        /// <param name="onCollide"> The action to perform when the mover collides</param>
        public override void Move(Vector2 difference, Action onCollide = null)
        {
            // Calculate the number of increments to move
            float factor = (float)(Math.Max(difference.X, difference.Y) / 5.0);

            // If the factor is less than 1, move the actor once
            if (factor < 1)
            {
                MoveY(difference.Y, onCollide);
                MoveX(difference.X, onCollide);
                return;
            }

            // Calculate the vector of the increments
            Point smallDifference = new Point((int)(difference.X / factor), (int)(difference.Y / factor));

            // Move the actor in increments
            if (smallDifference.X != 0 || smallDifference.Y != 0)
            {
                for (int index = 0; index < factor; index++)
                {
                    MoveY(smallDifference.Y, onCollide);
                    MoveX(smallDifference.X, onCollide);
                }
            }

            // Calculate the remaining difference
            Vector2 remainingDifference = difference - new Vector2(smallDifference.X * (float)Math.Floor(factor), smallDifference.Y * (float)(Math.Floor(factor)));

            // Move the actor with the remaining difference
            MoveY(remainingDifference.Y, onCollide);
            MoveX(remainingDifference.X, onCollide);
        }



        /// <summary>
        /// Moves the Actor in small increments, to avoid jitter in collision detection<br/>
        /// On collision, calls <paramref name="onCollide"/><br/>
        /// </summary>
        /// <param name="difference"> The difference to move</param>
        /// <param name="onCollide"> The action to perform when the mover collides</param>
        public override void Move(Point difference, Action onCollide = null)
        {
            // Calculate the number of increments to move
            float factor = (float)(Math.Max(difference.X, difference.Y) / 5.0);

            // If the factor is less than 1, move the actor once
            if (factor < 1)
            {
                MoveY(difference.Y, onCollide);
                MoveX(difference.X, onCollide);
                return;
            }

            // Calculate the vector of the increments
            Point smallDifference = new Point((int)(difference.X / factor), (int)(difference.Y / factor));

            // Move the actor in increments
            if (smallDifference.X != 0 || smallDifference.Y != 0)
            {
                for (int index = 0; index < factor; index++)
                {
                    MoveY(smallDifference.Y, onCollide);
                    MoveX(smallDifference.X, onCollide);
                }
            }
            
            // Calculate the remaining difference
            Point remainingDifference = difference - new Point((int)(smallDifference.X * (float)(Math.Floor(factor))), (int)(smallDifference.Y * (float)(Math.Floor(factor))));

            // Move the actor with the remaining difference
            MoveY(remainingDifference.Y, onCollide);
            MoveX(remainingDifference.X, onCollide);
        }



        float _xRemainder = 0;
        /// <summary>
        /// Moves the Actor in direction x, checking for collisions<br/>
        /// On collision, calls <paramref name="onCollide"/><br/>
        /// Handles fractional increments with _xRemainder<br/>
        /// </summary>
        /// <param name="amount"> The amount to move</param>
        /// <param name="onCollide"> The action to perform when the actor collides</param>
        public override void MoveX(float amount, Action onCollide = null)
        {
            // Add the amount to the remainder to move
            _xRemainder += amount;

            // Get the integer part of the remainder
            int move = (int)Math.Floor(_xRemainder);

            if (move != 0)
            {
                // Update the remainder of the move
                _xRemainder -= move;

                // Find the sign of the move
                int sign = Math.Sign(move);
                while (move != 0)
                {
                    // Check if the next move is legal
                    if (Legal(_position + new Point(sign, 0)))
                    {
                        // Move the mover
                        _position = _position + new Point(sign, 0);

                        // Update the remaining move
                        move -= sign;
                    }
                    else
                    {
                        //Hit a solid!
                        move = 0;
                        // If there is an onCollide, call it
                        if (onCollide != null)
                            onCollide(amount);
                        break;
                    }
                }
            }
        }



        float _yRemainder = 0;
        public override void MoveY(float amount, Action onCollide = null)
        {
            // Add the amount to the remainder to move
            _yRemainder += amount;

            // Get the integer part of the remainder
            int move = (int)Math.Floor(_yRemainder);

            if (move != 0)
            {
                // Update the remainder of the move
                _yRemainder -= move;

                // Find the sign of the move
                int sign = Math.Sign(move);

                while (move != 0)
                {
                    // Check if the next move is legal
                    // If we are falling, check for jump throughs as well
                    if (Legal(_position + new Point(0, sign)) && (move>0 || !Hitbox.CollideOutsideAt(_position + new Point(0, sign), onlyJumpThroughs)._isColliding))
                    {
                        // Move the mover
                        _position = _position + new Point(0, sign);

                        // Update the remaining move
                        move -= sign;
                    }
                    else
                    {
                        //Hit a solid!
                        move = 0;
                        // If there is an onCollide, call it
                        if (onCollide != null)
                            onCollide(amount);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the actor is riding a Solid<br/>
        /// </summary>
        /// <param name="solid"> The solid to check</param>
        /// <returns> True if this mover is riding the Solid </returns>
        public virtual bool IsRiding(Solid solid)
        {
            // Check if the solid's collider is the same as the actor's.
            // An Actor cannot be riding itself
            if (solid._colliderSensor._id == Hitbox._id) return false;

            // Check if the solid's collider is active
            if (solid._colliderSensor._active == false) return false;

            bool intersect = false;

            // Check if "solid"'s collider intersects with any of the actor's hitboxes.
            foreach (Rectangle rect in solid._colliderSensor._hitboxes)
            {
                // Offset the hitbox by the "solid"'s position.
                Rectangle newRect1 = rect;
                newRect1.Offset(solid._colliderSensor._position);

                // Check if "newRect1" intersects with any of "solid"'s hitboxes.
                foreach (Rectangle rect2 in Hitbox._hitboxes)
                {
                    // Offset the hitbox by the actor's position.
                    Rectangle newRect2 = rect2;
                    newRect2.Offset(_position);

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
            // Check if the solidmover's collider is the same as the actor's.
            if (solid.Hitbox._id == Hitbox._id) return false;

            // Check if the solidmover's collider is active
            if (solid.Hitbox._active == false) return false;

            bool intersect = false;

            // Check if "solidmover"'s collider intersects with any of the actor's hitboxes.
            foreach (Rectangle rect in solid.Hitbox._hitboxes)
            {
                // Offset the hitbox by the "solidmover"'s position.
                Rectangle newRect1 = rect;
                newRect1.Offset(solid.Hitbox._position);

                // Check if "newRect1" intersects with any of "solidmover"'s hitboxes.
                foreach (Rectangle rect2 in Hitbox._hitboxes)
                {
                    // Offset the hitbox by the actor's position.
                    Rectangle newRect2 = rect2;
                    newRect2.Offset(_position);

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
            // Check if the jumpthrough's collider is the same as the actor's.
            if (jumpt._colliderSensor._id == Hitbox._id) return false;

            // Check if the jumpthrough's collider is active
            if (jumpt._colliderSensor._active == false) return false;

            bool intersect = false;

            // Check if "jumpthrough"'s collider intersects with any of the actor's hitboxes.
            foreach (Rectangle rect in jumpt._colliderSensor._hitboxes)
            {
                // Offset the hitbox by the "jumpthrough"'s position.
                Rectangle newRect1 = rect;
                newRect1.Offset(jumpt._colliderSensor._position);

                 // Check if "newRect1" intersects with any of "jumpthrough"'s hitboxes.
                foreach (Rectangle rect2 in Hitbox._hitboxes)
                {
                    // Offset the hitbox by the actor's position.
                    Rectangle newRect2 = rect2;
                    newRect2.Offset(_position);

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
        /// Checks if the position is possible for the mover to be in<br/>
        /// Checks if the mover is not colliding with solids or jump throughs<br/>
        /// (Disabled collisions are handled in Legal)<br/>
        /// </summary>
        /// <param name="point"> The point to check</param>
        /// <returns> True if the point is a possible position for the mover</returns>
        public override bool _Legal(Point point)
        {
            return !Hitbox.CollideAt(point, onlySolids)._isColliding && !Hitbox.CollideOutsideAt(point, onlyJumpThroughs)._isColliding;
        }

        /// <summary>
        /// Function for filtering for collisions used to create delegate<br/>
        /// returns true if other contrains the "Solid" string tag<br/>
        /// </summary>
        /// <param name="self"> The collider sensor of the component</param>
        /// <param name="other"> The other collider in the collision</param>
        /// <returns> True if the other collider is a Solid</returns>
        public bool onlySolids(ColliderSensor self, Collider other)
        {
            return other._tags.Contains("Solid");
        }

        /// <summary>
        /// Function for filtering for collisions used to create delegate<br/>
        /// returns true if other contrains the "Actor" string tag<br/>
        /// </summary>
        /// <param name="self"> The collider sensor of the component</param>
        /// <param name="other"> The other collider in the collision</param>
        /// <returns> True if the other collider is a JumpThrough</returns>
        public bool onlyJumpThroughs(ColliderSensor self, Collider other)
        {
            return other._tags.Contains("JumpThrough");
        }


        /// <summary>
        /// Dispose,<br/>
        /// Disposes of the Hitbox as well<br/>
        /// </summary>
        public override void Dispose()
        {
            if (!Hitbox._isDisposed) Hitbox.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// UnsafeDispose<br/>
        /// Removes the mover from Game.I._actorMovers<br/>
        /// </summary>
        public override void UnsafeDispose()
        {
            Game.I._actorMovers.Remove(this);
            base.UnsafeDispose();
        }


    }
}
