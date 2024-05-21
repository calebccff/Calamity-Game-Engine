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
    /// Component that moves the Character as a Solid<br/>
    /// </summary>
    public class SolidMover : Mover
    {
        /// <summary>
        /// The Hitbox of the Solid<br/>
        /// </summary>
        public ColliderSensor Hitbox;

        //TODO: Rework to use tag system instead of strings

        /// <summary>
        /// Constructor<br/>
        /// Sets the Hitbox, adds the Solid tag, and sets the ShouldCollideCheck<br/>
        /// </summary>
        /// <param name="character"></param>
        public SolidMover(Character character) : base(character)
        {
            Hitbox = new ColliderSensor(this);
            Hitbox._tags.Add("Solid");
            Hitbox._sCC = new ColliderSensor.ShouldCollideCheck(ShouldCollideCheck);
        }

        /// <summary>
        /// Called when the position changes<br/>
        /// Updates the position of the Hitbox<br/>
        /// </summary>
        /// <param name="Old"> Old position of the component</param>
        /// <param name="New"> New position of the component</param>
        public override void OnPositionChange(Point Old, Point New)
        {
            Hitbox._position = New;
        }

        /// <summary>
        /// Function for filtering for collisions used to create delegate<br/>
        /// returns true if other contrains the "Actor" string tag<br/>
        /// </summary>
        /// <param name="self"> The collider sensor of the component</param>
        /// <param name="other"> The other collider in the collision</param>
        /// <returns></returns>
        public bool ShouldCollideCheck(ColliderSensor self, Collider other)
        {
            return other._tags.Contains("Actor");
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



        //TODO: Omg clean this up

        float _xRemainder = 0;
        float _yRemainder = 0;
        /// <summary>
        /// Move the Solid in direction <paramref name="x"/> and <paramref name="y"/><br/>
        /// Handles floating point movement with _xremainder and _yRemainder<br/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="onCollide"></param>
        public override void Move(float x, float y, Action onCollide = null)
        {
            //Calculates values to move this round
            //Later we reduce the remainders as we move
            _xRemainder += x;
            _yRemainder += y;
            int moveX = (int)Math.Round(_xRemainder);
            int moveY = (int)Math.Round(_yRemainder);

            // Check if there is any movement
            if (moveX != 0 || moveY != 0)
            {
                //Find all Actor-s riding this solid

                //Loop through every Actor in the Level, add it to 
                //a list if actor.IsRiding(this) is true 
                List<Actor> riding = new List<Actor>();
                foreach (Actor actor in Game.I._actors)
                {
                    if (actor.IsRiding(this))
                    {
                        riding.Add(actor as Actor);
                    }

                }
                //Loop through every ActorMover in the Level, add it to 
                //a list if actor.IsRiding(this) is true 
                List<ActorMover> ridingMover = new List<ActorMover>();
                foreach (ActorMover actorMover in Game.I._actorMovers)
                {
                    if (actorMover.IsRiding(this))
                    {
                        ridingMover.Add(actorMover as ActorMover);
                    }

                }

                //Check if there is any movement in the X direction
                if (moveX != 0)
                {
                    //If there is, reduce the remainder accordingly
                    _xRemainder -= moveX;
                    //Move the solid in the X direction
                    //Pay attention that this already triggers onCollide
                    _position = _position + new Point(moveX, 0);

                    //Handle movement left and right separately
                    if (moveX > 0)
                    {

                        //Find if there is any actor or actor mover in the way
                        //(ones that weren't riding before, but are in the way of the solid)
                        foreach (Actor actor in Game.I._actors)
                        {
                            //For each Actor it the way, calculate the distance the solid pushes it to the right
                            int biggestCarryDistance = 0;

                            bool intersect = false;
                            foreach (Rectangle rect in actor._colliderSensor._hitboxes)
                            {
                                //Check if the actor intersects any of the solid's hitboxes
                                Rectangle newRect1 = rect;
                                newRect1.Offset(actor._colliderSensor._position);
                                foreach (Rectangle rect2 in Hitbox._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(_position);

                                    if (newRect1.Intersects(newRect2))
                                    {
                                        //Calculate the largest distance any of the hitboxes pushes the actor to the right
                                        intersect = true;
                                        biggestCarryDistance = Math.Max(biggestCarryDistance, newRect2.Right - newRect1.Left);

                                    }
                                }
                            }

                            if (intersect)
                            {
                                //Push the actor to the right (if can't because of something in the way, it squishes the actor)
                                actor.MoveX(biggestCarryDistance, actor.Squish);
                                /**/
                                //TODO: Think about implementing an eventlistener for pushing
                                /**/
                            }
                            if (riding.Contains(actor))
                            {
                                //If the actor is riding the solid, push it to the right by the same amount it moves instead
                                //At this remaining distance, the actor will just stop moving, if it can't move further
                                actor.MoveX(moveX- biggestCarryDistance, null);
                            }
                        }

                        //Do the same for actor movers
                        foreach (ActorMover actorMover in Game.I._actorMovers)
                        {
                            int biggestCarryDistance = 0;
                            bool intersect = false;
                            foreach (Rectangle rect in actorMover.Hitbox._hitboxes)
                            {
                                Rectangle newRect1 = rect;
                                newRect1.Offset(actorMover.Hitbox._position);
                                foreach (Rectangle rect2 in Hitbox._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(_position);

                                    if (newRect1.Intersects(newRect2))
                                    {
                                        intersect = true;
                                        biggestCarryDistance = Math.Max(biggestCarryDistance, newRect2.Right - newRect1.Left);

                                    }
                                }
                            }

                            if (intersect)
                            {
                                //Push right 
                                actorMover.MoveX(biggestCarryDistance, actorMover.Squish);
                                /**/

                                /**/
                            }
                            if (ridingMover.Contains(actorMover))
                            {
                                //Carry right 
                                actorMover.MoveX(moveX- biggestCarryDistance, null);
                            }
                        }
                    }
                    else
                    {
                        //Do the same for moving left
                        foreach (Actor actor in Game.I._actors)
                        {
                            int biggestCarryDistance = 0;
                            bool intersect = false;
                            foreach (Rectangle rect in actor._colliderSensor._hitboxes)
                            {
                                Rectangle newRect1 = rect;
                                newRect1.Offset(actor._colliderSensor._position);
                                foreach (Rectangle rect2 in Hitbox._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(_position);

                                    if (newRect1.Intersects(newRect2))
                                    {
                                        intersect = true;
                                        biggestCarryDistance = Math.Min(biggestCarryDistance, newRect2.Left - newRect1.Right);

                                    }
                                }
                            }

                            if (intersect)
                            {
                                //Push left 
                                actor.MoveX(biggestCarryDistance, actor.Squish);

                            }
                            if (riding.Contains(actor))
                            {
                                //Carry left 
                                actor.MoveX(moveX - biggestCarryDistance, null);
                            }
                        }

                        foreach (ActorMover actorMover in Game.I._actorMovers)
                        {
                            int biggestCarryDistance = 0;
                            bool intersect = false;
                            foreach (Rectangle rect in actorMover.Hitbox._hitboxes)
                            {
                                Rectangle newRect1 = rect;
                                newRect1.Offset(actorMover.Hitbox._position);
                                foreach (Rectangle rect2 in Hitbox._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(_position);

                                    if (newRect1.Intersects(newRect2))
                                    {
                                        intersect = true;
                                        biggestCarryDistance = Math.Min(biggestCarryDistance, newRect2.Left - newRect1.Right);

                                    }
                                }
                            }

                            if (intersect)
                            {
                                //Push left 
                                actorMover.MoveX(biggestCarryDistance, actorMover.Squish);

                            }
                            if (ridingMover.Contains(actorMover))
                            {
                                //Carry left 
                                actorMover.MoveX(moveX - biggestCarryDistance, null);
                            }
                        }
                    }
                }



                //Do the same for moving up and down
                if (moveY != 0)
                {
                    _yRemainder -= moveY;
                    _position = _position + new Point(0, moveY);

                    if (moveY > 0)
                    {
                        foreach (Actor actor in Game.I._actors)
                        {
                            int biggestCarryDistance = 0;
                            bool intersect = false;
                            foreach (Rectangle rect in actor._colliderSensor._hitboxes)
                            {
                                Rectangle newRect1 = rect;
                                newRect1.Offset(actor._colliderSensor._position);
                                foreach (Rectangle rect2 in Hitbox._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(_position);

                                    if (newRect1.Intersects(newRect2))
                                    {
                                        intersect = true;
                                        biggestCarryDistance = Math.Max(biggestCarryDistance, newRect2.Bottom - newRect1.Top);

                                    }
                                }
                            }

                            if (intersect)
                            {
                                //Push down 
                                actor.MoveY(biggestCarryDistance, actor.Squish);
                            }
                            if (riding.Contains(actor))
                            {

                                //Carry down 
                                actor.MoveY(moveY- biggestCarryDistance, null);

                            }
                        }


                        foreach (ActorMover actorMover in Game.I._actorMovers)
                        {
                            int biggestCarryDistance = 0;
                            bool intersect = false;
                            foreach (Rectangle rect in actorMover.Hitbox._hitboxes)
                            {
                                Rectangle newRect1 = rect;
                                newRect1.Offset(actorMover.Hitbox._position);
                                foreach (Rectangle rect2 in Hitbox._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(_position);

                                    if (newRect1.Intersects(newRect2))
                                    {
                                        intersect = true;
                                        biggestCarryDistance = Math.Max(biggestCarryDistance, newRect2.Bottom - newRect1.Top);

                                    }
                                }
                            }

                            if (intersect)
                            {
                                //Push down 
                                actorMover.MoveY(biggestCarryDistance, actorMover.Squish);
                            }
                            if (ridingMover.Contains(actorMover))
                            {

                                //Carry down 
                                actorMover.MoveY(moveY- biggestCarryDistance, null);

                            }
                        }
                    }
                    else
                    {
                        foreach (Actor actor in Game.I._actors)
                        {
                            int biggestCarryDistance = 0;
                            bool intersect = false;
                            foreach (Rectangle rect in actor._colliderSensor._hitboxes)
                            {
                                Rectangle newRect1 = rect;
                                newRect1.Offset(actor._colliderSensor._position);
                                foreach (Rectangle rect2 in Hitbox._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(_position);

                                    if (newRect1.Intersects(newRect2))
                                    {
                                        intersect = true;
                                        biggestCarryDistance = Math.Min(biggestCarryDistance, newRect2.Top - newRect1.Bottom);

                                    }
                                }
                            }

                            if (intersect)
                            {
                                //Push up 
                                actor.MoveY(biggestCarryDistance, actor.Squish);
                            }
                            if (riding.Contains(actor))
                            {
                                //Carry up 
                                actor.MoveY(moveY - biggestCarryDistance, null);
                            }
                        }
                        foreach (ActorMover actorMover in Game.I._actorMovers)
                        {
                            int biggestCarryDistance = 0;
                            bool intersect = false;
                            foreach (Rectangle rect in actorMover.Hitbox._hitboxes)
                            {
                                Rectangle newRect1 = rect;
                                newRect1.Offset(actorMover.Hitbox._position);
                                foreach (Rectangle rect2 in Hitbox._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(_position);

                                    if (newRect1.Intersects(newRect2))
                                    {
                                        intersect = true;
                                        biggestCarryDistance = Math.Min(biggestCarryDistance, newRect2.Top - newRect1.Bottom);

                                    }
                                }
                            }

                            if (intersect)
                            {
                                //Push up 
                                actorMover.MoveY(biggestCarryDistance, actorMover.Squish);
                            }
                            if (ridingMover.Contains(actorMover))
                            {
                                //Carry up 
                                actorMover.MoveY(moveY - biggestCarryDistance, null);
                            }
                        }
                    }
                }


            }
        }


    }
}
