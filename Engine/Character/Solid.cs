using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TestShader
{
    //TODO: Rework into Character movers

    /// <summary>
    /// Component that moves as a solid object<br/>
    /// Pushes Actors, but not other Solids<br/>
    /// It has a sprite<br/>
    /// </summary>
    public class Solid : Component
    {
        /// <summary>
        /// The position of the solid (Private)<br/>
        /// </summary>
        private Point _positionPrivate;

        /// <summary>
        /// The position of the solid<br/>
        /// On set, it sets the position of the sprite and the collider<br/>
        /// </summary>
        public Point Position
        {
            get
            {
                return _positionPrivate;
            }
            set
            {
                _position = value;
                _positionPrivate = value;
                if (_sprite != null)
                    _sprite._position = value;
                if (_colliderSensor != null)
                    _colliderSensor._position = value;
            }
        }

        /// <summary>
        /// The Sprite corresponding to the solid (Private)<br/>
        /// </summary>
        public SpriteGameComponent _privateSprite = null;
        /// <summary>
        /// The Sprite corresponding to the solid<br/>
        /// Can only be set once<br/>
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
        /// The collider corresponding to the solid<br/>
        /// </summary>
        public ColliderSensor _colliderSensor;

        /// <summary>
        /// The texture of the sprite corresponding to the solid<br/>
        /// </summary>
        public Texture2D _texture;

        /// <summary>
        /// The width of the sprite of the solid<br/>
        /// </summary>
        public int _width = 0;

        /// <summary>
        /// The height of the sprite of the solid<br/>
        /// </summary>
        public int _height = 0;


        /// <summary>
        /// Constructor<br/>
        /// Creates a collider for the solid<br/>
        /// </summary>
        /// <param name="texture"></param>
        public Solid(Texture2D texture) : base()
        {
            _texture = texture;
            _colliderSensor = new ColliderSensor(this);
            _colliderSensor._tags.Add("Solid");
            _colliderSensor._sCC = new ColliderSensor.ShouldCollideCheck(ShouldCollideCheck);

        }

        /// <summary>
        /// LoadContent<br/>
        /// Creates the sprite if it doesn't exist yet<br/>
        /// Sets the width and height from the sprite<br/>
        /// </summary>
        protected override void LoadContent()
        {
            _colliderSensor._position = Position;
            if (_sprite == null) { _sprite = new SpriteGameComponent(_texture); }
            _width = _sprite._width;
            _height = _sprite._height;
            base.LoadContent();
        }


        /// <summary>
        /// Update<br/>
        /// Updates the position of the sprite and collider<br/>
        /// Sets the width and height according to the sprite<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            _sprite._position = Position;
            _colliderSensor._position = Position;
            _width = _sprite._width;
            _height = _sprite._height;
            base.Update(gameTime);
        }

        /// <summary>
        /// Move the Solid by the vector <paramref name="difference"/><br/>
        /// </summary>
        /// <param name="difference"> The difference to move</param>
        public void Move(Vector2 difference)
        {
            Move(difference.X, difference.Y);
        }

        /// <summary>
        /// The function that determines if two solids should collide used to create a delegate<br/>
        /// It returns true if the other solid is an actor<br/>
        /// </summary>
        /// <param name="self"> The collider of the solid</param>
        /// <param name="other"> The other collider in the collision</param>
        /// <returns></returns>
        public bool ShouldCollideCheck(ColliderSensor self, Collider other)
        {
            return other._tags.Contains("Actor");
        }

        /// <summary>
        /// Dispose<br/>
        /// Disposes the sprite and the collider<br/>
        /// </summary>
        public override void Dispose()
        {
            if (!_sprite._isDisposed) _sprite.Dispose();
            if (!_colliderSensor._isDisposed) _colliderSensor.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Move the Solid in direction <paramref name="move"/><br/>
        /// </summary>
        /// <param name="move"> The difference to move</param>
        public void Move(Point move)
        {
            Move(move.X, move.Y);
        }



        /// <summary>
        /// Remainders of floating point movement in the direction X<br/>
        /// </summary>
        float _xRemainder = 0;
        /// <summary>
        /// Remainders of floating point movement in the direction Y<br/>
        /// </summary>
        float _yRemainder = 0;

        //TODO: Omg clean this up

        /// <summary>
        /// Move the Solid in direction <paramref name="x"/> and <paramref name="y"/><br/>
        /// Handles floating point movement with _xremainder and _yRemainder<br/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Move(float x, float y)
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
                    Position = Position + new Point(moveX, 0);

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
                                foreach (Rectangle rect2 in _colliderSensor._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(Position);

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
                                foreach (Rectangle rect2 in _colliderSensor._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(Position);

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
                                actorMover.MoveX(moveX - biggestCarryDistance, null);
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
                                foreach (Rectangle rect2 in _colliderSensor._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(Position);

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
                                foreach (Rectangle rect2 in _colliderSensor._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(Position);

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



                // Repeat for moving up and down
                if (moveY != 0)
                {
                    _yRemainder -= moveY;
                    Position = Position + new Point(0, moveY);

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
                                foreach (Rectangle rect2 in _colliderSensor._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(Position);

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
                                foreach (Rectangle rect2 in _colliderSensor._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(Position);

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
                                actorMover.MoveY(moveY - biggestCarryDistance, null);

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
                                foreach (Rectangle rect2 in _colliderSensor._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(Position);

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
                                foreach (Rectangle rect2 in _colliderSensor._hitboxes)
                                {
                                    Rectangle newRect2 = rect2;
                                    newRect2.Offset(Position);

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
