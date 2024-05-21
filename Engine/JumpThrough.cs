using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TestShader
{
    public class JumpThrough : Component
    {

        public SpriteGameComponent _privateSprite = null;
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
        public ColliderSensor _colliderSensor;
        public Texture2D _texture;
        public int _width = 0;
        public int _height = 0;

        public JumpThrough(Texture2D texture) : base()
        {
            _texture = texture;
            _colliderSensor = new ColliderSensor(this);
            _colliderSensor._tags.Add("JumpThrough");
            _colliderSensor._sCC = new ColliderSensor.ShouldCollideCheck(ShouldCollideCheck);

        }

        protected override void LoadContent()
        {
            _colliderSensor._position = _position;
            if (_sprite == null) { _sprite = new SpriteGameComponent(_texture); }
            _width = _sprite._width;
            _height = _sprite._height;
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            _sprite._position = _position;
            _colliderSensor._position = _position;
            _width = _sprite._width;
            _height = _sprite._height;
            base.Update(gameTime);
        }


        public bool ShouldCollideCheck(ColliderSensor self, Collider other)
        {
            return other._tags.Contains("Actor");
        }

        public override void Dispose()
        {
            if (!_sprite._isDisposed) _sprite.Dispose();
            if (!_colliderSensor._isDisposed) _colliderSensor.Dispose();
            base.Dispose();
        }
        float _xRemainder = 0;
        float _yRemainder = 0;

        public void Move(Point move)
        {
            for (int x = 0; Math.Abs(x) < Math.Abs(move.X); x += Math.Sign(move.X)) Move1(Math.Sign(move.X), 0);

            for (int y = 0; Math.Abs(y) < Math.Abs(move.Y); y += Math.Sign(move.Y)) Move1(0, Math.Sign(move.Y));
        }


        public void Move(float x0, float y0)
        {
            for (int x = 0; Math.Abs(x) < Math.Abs(x0); x += Math.Sign(x0)) Move1(Math.Sign(x0), 0);

            for (int y = 0; Math.Abs(y) < Math.Abs(y0); y += Math.Sign(y0)) Move1(0, Math.Sign(y0));
        }

        //TODO: Jump throughs
        //TODO: dash corrections





        public void Move1(float x, float y)
        {

            //bool beforePlayerIntersection = TestGame.game._player._colliderSensor.Colliding(_colliderSensor);

            _xRemainder += x;
            _yRemainder += y;
            int moveX = (int)Math.Round(_xRemainder);
            int moveY = (int)Math.Round(_yRemainder);
            if (moveX != 0 || moveY != 0)
            {


                //Make this Solid non-collidable for Actors, 
                //so that Actors moved by it do not get stuck on it 
                _colliderSensor._active = false;
                if (moveX != 0)
                {

                    _xRemainder -= moveX;
                    if (moveX > 0)
                    {
                        for (int i = 0; i < moveX; i++)
                        {
                            //Re-enable collisions for this Solid 
                            _colliderSensor._active = true;

                            //Loop through every Actor in the Level, add it to 
                            //a list if actor.IsRiding(this) is true 
                            List<Actor> riding = new List<Actor>();
                            List<ActorMover> ridingMover = new List<ActorMover>();
                            foreach (Actor actor in Game.I._actors)
                            {
                                if (actor.IsRiding(this))
                                {
                                    Debug.WriteLine("Ride");
                                    riding.Add(actor as Actor);
                                }

                            }
                            foreach (ActorMover actorMover in Game.I._actorMovers)
                            {
                                if (actorMover.IsRiding(this))
                                {
                                    Debug.WriteLine("Ride");
                                    ridingMover.Add(actorMover as ActorMover);
                                }

                            }
                            //Make this Solid non-collidable for Actors, 
                            //so that Actors moved by it do not get stuck on it 
                            _colliderSensor._active = false;


                            _position.X += 1;

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
                                        newRect2.Offset(_position);

                                        if (newRect1.Intersects(newRect2))
                                        {
                                            intersect = true;
                                            biggestCarryDistance = Math.Max(biggestCarryDistance, newRect2.Right - newRect1.Left);

                                        }
                                    }
                                }

                                if (intersect && riding.Contains(actor) && biggestCarryDistance >= 1)
                                {

                                    //Push right 
                                    //actor.MoveX(1, actor.Squish);
                                    actor.MoveX(1, null);
                                    /**/

                                    /**/
                                }
                                else if (riding.Contains(actor))
                                {
                                    //Carry right 
                                    actor.MoveX(1, null);
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
                                        newRect2.Offset(_position);

                                        if (newRect1.Intersects(newRect2))
                                        {
                                            intersect = true;
                                            biggestCarryDistance = Math.Max(biggestCarryDistance, newRect2.Right - newRect1.Left);

                                        }
                                    }
                                }

                                if (intersect && ridingMover.Contains(actorMover) && biggestCarryDistance >= 1)
                                {

                                    //Push right 
                                    //actor.MoveX(1, actor.Squish);
                                    actorMover.MoveX(1, null);
                                    /**/

                                    /**/
                                }
                                else if (ridingMover.Contains(actorMover))
                                {
                                    //Carry right 
                                    actorMover.MoveX(1, null);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < -moveX; i++)
                        {



                            //Re-enable collisions for this Solid 
                            _colliderSensor._active = true;

                            //Loop through every Actor in the Level, add it to 
                            //a list if actor.IsRiding(this) is true 
                            List<Actor> riding = new List<Actor>();
                            foreach (Actor actor in Game.I._actors)
                            {
                                if (actor.IsRiding(this))
                                {
                                    Debug.WriteLine("Ride");
                                    riding.Add(actor as Actor);
                                }

                            }
                            List<ActorMover> ridingMover = new List<ActorMover>();
                            foreach (ActorMover actorMover in Game.I._actorMovers)
                            {
                                if (actorMover.IsRiding(this))
                                {
                                    Debug.WriteLine("Ride");
                                    ridingMover.Add(actorMover as ActorMover);
                                }

                            }
                            //Make this Solid non-collidable for Actors, 
                            //so that Actors moved by it do not get stuck on it 
                            _colliderSensor._active = false;
                            _position.X -= 1;

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
                                        newRect2.Offset(_position);

                                        if (newRect1.Intersects(newRect2))
                                        {
                                            intersect = true;
                                            biggestCarryDistance = Math.Min(biggestCarryDistance, newRect2.Left - newRect1.Right);

                                        }
                                    }
                                }

                                if (intersect && riding.Contains(actor) && biggestCarryDistance >= 1)
                                {

                                    //Push right 
                                    //actor.MoveX(1, actor.Squish);
                                    actor.MoveX(-1, null);
                                    /**/

                                    /**/
                                }
                                else if (riding.Contains(actor))
                                {
                                    //Carry right 
                                    actor.MoveX(-1, null);
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
                                        newRect2.Offset(_position);

                                        if (newRect1.Intersects(newRect2))
                                        {
                                            intersect = true;
                                            biggestCarryDistance = Math.Min(biggestCarryDistance, newRect2.Left - newRect1.Right);

                                        }
                                    }
                                }

                                if (intersect && ridingMover.Contains(actorMover) && biggestCarryDistance >= 1)
                                {

                                    //Push right 
                                    //actor.MoveX(1, actor.Squish);
                                    actorMover.MoveX(-1, null);
                                    /**/

                                    /**/
                                }
                                else if (ridingMover.Contains(actorMover))
                                {
                                    //Carry right 
                                    actorMover.MoveX(-1, null);
                                }
                            }
                        }
                    }
                }




                if (moveY != 0)
                {






                    _yRemainder -= moveY;
                    if (moveY > 0)
                    {

                        for (int i = 0; i < moveY; i++)
                        {



                            //Re-enable collisions for this Solid 
                            _colliderSensor._active = true;

                            //Loop through every Actor in the Level, add it to 
                            //a list if actor.IsRiding(this) is true 
                            List<Actor> riding = new List<Actor>();
                            foreach (Actor actor in Game.I._actors)
                            {
                                if (actor.IsRiding(this))
                                {
                                    Debug.WriteLine("Ride");
                                    riding.Add(actor as Actor);
                                }

                            }
                            //Loop through every ActorMover in the Level, add it to 
                            //a list if actorMover.IsRiding(this) is true 
                            List<ActorMover> ridingMover = new List<ActorMover>();
                            foreach (ActorMover actorMover in Game.I._actorMovers)
                            {
                                if (actorMover.IsRiding(this))
                                {
                                    Debug.WriteLine("Ride");
                                    ridingMover.Add(actorMover as ActorMover);
                                }

                            }

                            //Make this Solid non-collidable for Actors, 
                            //so that Actors moved by it do not get stuck on it 
                            _colliderSensor._active = false;
                            _position.Y += 1;

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
                                        newRect2.Offset(_position);

                                        if (newRect1.Intersects(newRect2))
                                        {

                                            intersect = true;
                                            biggestCarryDistance = Math.Max(biggestCarryDistance, newRect2.Bottom - newRect1.Top);

                                        }
                                    }
                                }

                                if (intersect && riding.Contains(actor) && biggestCarryDistance >= 1)
                                {

                                    //Push right 
                                    //actor.MoveX(1, actor.Squish);
                                    actor.MoveY(1, null);
                                    /**/

                                    /**/
                                }
                                else if (riding.Contains(actor))
                                {
                                    //Carry right 
                                    actor.MoveY(1, null);
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
                                        newRect2.Offset(_position);

                                        if (newRect1.Intersects(newRect2))
                                        {

                                            intersect = true;
                                            biggestCarryDistance = Math.Max(biggestCarryDistance, newRect2.Bottom - newRect1.Top);

                                        }
                                    }
                                }

                                if (intersect && ridingMover.Contains(actorMover) && biggestCarryDistance >= 1)
                                {

                                    //Push right 
                                    //actor.MoveX(1, actor.Squish);
                                    actorMover.MoveY(1, null);
                                    /**/

                                    /**/
                                }
                                else if (ridingMover.Contains(actorMover))
                                {
                                    //Carry right 
                                    actorMover.MoveY(1, null);
                                }
                            }
                        }



                    }
                    else
                    {

                        for (int i = 0; i < -moveY; i++)
                        {

                            //Re-enable collisions for this Solid 
                            _colliderSensor._active = true;

                            //Loop through every Actor in the Level, add it to 
                            //a list if actor.IsRiding(this) is true 
                            List<Actor> riding = new List<Actor>();
                            foreach (Actor actor in Game.I._actors)
                            {
                                if (actor.IsRiding(this))
                                {
                                    Debug.WriteLine("Ride");
                                    riding.Add(actor as Actor);
                                }

                            }
                            //Loop through every Actor in the Level, add it to 
                            //a list if actor.IsRiding(this) is true 
                            List<ActorMover> ridingMover = new List<ActorMover>();
                            foreach (ActorMover actorMover in Game.I._actorMovers)
                            {
                                if (actorMover.IsRiding(this))
                                {
                                    Debug.WriteLine("Ride");
                                    ridingMover.Add(actorMover as ActorMover);
                                }

                            }
                            //Make this Solid non-collidable for Actors, 
                            //so that Actors moved by it do not get stuck on it 
                            _colliderSensor._active = false;
                            _position.Y -= 1;

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
                                        newRect2.Offset(_position);

                                        if (newRect1.Intersects(newRect2))
                                        {
                                            //TODO: intersection with jump-throughs
                                            intersect = true;
                                            biggestCarryDistance = Math.Min(biggestCarryDistance, newRect2.Top - newRect1.Bottom);

                                        }
                                    }
                                }

                                if (intersect && riding.Contains(actor) && biggestCarryDistance >= 1)
                                {

                                    //Push right 
                                    //actor.MoveX(1, actor.Squish);
                                    actor.MoveY(-1, null);
                                    /**/

                                    /**/
                                }
                                else if (riding.Contains(actor))
                                {
                                    //Carry right 
                                    actor.MoveY(-1, null);
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
                                        newRect2.Offset(_position);

                                        if (newRect1.Intersects(newRect2))
                                        {
                                            //TODO: intersection with jump-throughs
                                            intersect = true;
                                            biggestCarryDistance = Math.Min(biggestCarryDistance, newRect2.Top - newRect1.Bottom);

                                        }
                                    }
                                }

                                if (intersect && ridingMover.Contains(actorMover) && biggestCarryDistance >= 1)
                                {

                                    //Push right 
                                    //actor.MoveX(1, actor.Squish);
                                    actorMover.MoveY(-1, null);
                                    /**/

                                    /**/
                                }
                                else if (ridingMover.Contains(actorMover))
                                {
                                    //Carry right 
                                    actorMover.MoveY(-1, null);
                                }
                            }
                        }

                    }
                }




                //Re-enable collisions for this Solid 
                _colliderSensor._active = true;
            }

            //bool afterPlayerIntersection = TestGame.game._player._colliderSensor.Colliding(_colliderSensor);

            //if (!beforePlayerIntersection && afterPlayerIntersection)
            //{
            //    Debug.WriteLine("It is jumpthrough");
            //}
        }


    }
}


