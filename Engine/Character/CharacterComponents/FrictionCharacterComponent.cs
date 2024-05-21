using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TestShader;

namespace TestShader
{
    /// <summary>
    /// CharacterComponent that manages speed friction and other rudimentary physics<br/>
    /// Actual movement and hitboxes are handled by Mover-s<br/>
    /// </summary>
    public class FrictionCharacterComponent : CharacterComponent
    {
        //TODO: Implement property for speed and onSpeedChange eventlistener

        /// <summary>
        /// The current speed of the character<br/>
        /// </summary>
        public Vector2 _speed;
        
        /// <summary>
        /// The current acceleration of the character in the x direction<br/>
        /// </summary>
        public float _actualXAccel;
        /// <summary>
        ///  The current acceleration of the character in the y direction<br/>
        /// </summary>
        public float _actualYAccel;
        
        //TODO: Change from readonly
        /// <summary>
        /// The effect of gravity on frictionCharacterComponents<br/>
        /// </summary>
        public static readonly float Gravity = 1000f;
        
        /// <summary>
        /// The current target speed of the character in the x direction<br/>
        /// This should be handled by the character's logic<br/>
        /// </summary>
        public float _targetXSpeed;
        /// <summary>
        /// The current target speed of the character in the y direction<br/>
        /// This should be handled by the character's logic<br/>
        /// </summary>
        public float _targetYSpeed;


        //TODO: Why Mover.Action?
        /// <summary>
        /// The action to perform when the character collides in the horizontal direction<br/>
        /// </summary>
        public Mover.Action OnCollideH;
        /// <summary>
        /// The action to perform when the character collides in the vertical direction<br/>
        /// </summary>
        public Mover.Action OnCollideV;


        //TODO: Is this necessary?
        /// <summary>
        /// The action to perform when the character collides with a surface<br/>
        /// </summary>
        public Dictionary<Point, Action> OnSurfaceReset = new Dictionary<Point, Action>{
            {Direction.Ground   ,null},
            {Direction.RightWall,null},
            {Direction.LeftWall ,null},
            {Direction.Ceiling ,null},
        };

        /// <summary>
        /// The bools for if the character is on a surface in each direction<br/>
        /// </summary>
        public Dictionary<Point, bool> _onSurface = new Dictionary<Point, bool>{
            {Direction.Ground   ,false},
            {Direction.RightWall,false},
            {Direction.LeftWall ,false},
            {Direction.Ceiling ,false },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FrictionCharacterComponent"/> class.<br/>
        /// </summary>
        /// <param name="character"> The character this component is attached to</param>
        public FrictionCharacterComponent(ICharacter character) : base(character)
        {
        }

        /// <summary>
        /// LoadContent<br/>
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();
        }

        /// <summary>
        /// This is called when the position of the component changes<br/>
        /// </summary>
        /// <param name="Old"> Old position of the component</param>
        /// <param name="New"> New position of the component</param>
        public override void OnPositionChange(Point Old, Point New)
        {
            base.OnPositionChange(Old, New);
        }

        /// <summary>
        /// Update<br/>
        /// Calculates new speeds with friction<br/>
        /// Moves the character<br/>
        /// Updates the surface variables<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            //Calculate new speeds with friction
            _speed.Y = CalculateSpeedY();
            _speed.X = CalculateSpeedX();

            //Move the character in both directions
            character.Mover.MoveX(this._speed.X * Game.I._deltaTime, OnCollideH);
            character.Mover.MoveY(this._speed.Y * Game.I._deltaTime, OnCollideV);

            //Update the variables for each surface direction
            foreach (Point direction in Direction.Directions)
                UpdateSurface(direction);

            base.Update(gameTime);
        }

        /// <summary>
        /// Updates the surface variables for a given direction<br/>
        /// </summary>
        /// <param name="direction"> The direction to update</param>
        private void UpdateSurface(Point direction)
        {
            //Note, Is is on Ground (Check before move!!!!) We don't want to fall with edge correction

            //Check if the character just got to the surface
            if (!_onSurface[direction] && CheckOnSurface(direction))
            {
                // Call the surface action
                if (OnSurfaceReset[direction] != null) OnSurfaceReset[direction]();
            }

            //Update the surface variable
            _onSurface[direction] = CheckOnSurface(direction);
        }


        /// <summary>
        /// Checks if the character is on a surface<br/>
        /// If Character has a speed away from the surface, it is not on the surface<br/>
        /// </summary>
        /// <param name="offset"> The offset to check with (This is the same as the direction of the check)</param>
        /// <returns></returns>
        private bool CheckOnSurface(Point offset)
        {
            // Check if the character is on a surface
            // If Character has a speed away from the surface, it is not on the surface
            return (_speed.X * offset.X + _speed.Y * offset.Y >= 0) && character.Mover.Legal(_position) && !(character.Mover.Legal(_position + offset));

        }

        /// <summary>
        /// Calculates new speed with friction in the X direction<br/>
        /// </summary>
        /// <returns> The new speed in the X direction </returns>
        private float CalculateSpeedX()
        {
            //Approach the target speed with the actual acceleration value
            float newSpeedX = Calc.Approach(_speed.X, _targetXSpeed, _actualXAccel * Game.I._deltaTime);  //Reduce back from beyond the max speed

            // If the character is on a wall, then set the speed to 0
            if (_onRightWall)
            {
                newSpeedX = Math.Min(newSpeedX, 0);
            }
            if (_onLeftWall)
            {
                newSpeedX = Math.Max(newSpeedX, 0);
            }
            return newSpeedX;
        }

        /// <summary>
        /// Calculates new speed with friction in the Y direction<br/>
        /// </summary>
        /// <returns> The new speed in the Y direction</returns>
        private float CalculateSpeedY()
        {
            //Approach the target speed with the actual acceleration value
            float newSpeedY = Calc.Approach(_speed.Y, _targetYSpeed, _actualYAccel * Game.I._deltaTime);

            // If the character is on the ground or ceiling, then set the speed to 0
            if (_onGround)
            {
                newSpeedY = Math.Min(0, newSpeedY);
            }
            if (_onCeiling)
            {
                newSpeedY = Math.Max(0, newSpeedY);
            }
            return newSpeedY;
        }

        /// <summary>
        /// Checks if the character is falling<br/>
        /// A character is falling if it is adjacent to the ground and is not moving upwards<br/>
        /// </summary>
        /// <returns></returns>
        private bool isFalling()
        {
            return !_onGround && _speed.Y > 0;
        }

        /// <summary>
        /// Checks if the character is rising<br/>
        /// A character is rising if it is not adjacent to the ground and is moving upwards<br/>
        /// </summary>
        /// <returns></returns>
        private bool isRising()
        {
            return !_onGround && _speed.Y < 0;
        }

        //TODO: Remove superflous speed checks


        // Defining names for each surface variable explicitly
        #region accesors
        public bool _onGround { get => _onSurface[Direction.Ground]; set { _onSurface[Direction.Ground] = value; } }
        public bool _onRightWall { get => _onSurface[Direction.RightWall]; set { _onSurface[Direction.RightWall] = value; } }
        public bool _onLeftWall { get => _onSurface[Direction.LeftWall]; set { _onSurface[Direction.LeftWall] = value; } }
        public bool _onCeiling { get => _onSurface[Direction.Ceiling]; set { _onSurface[Direction.Ceiling] = value; } }


        public Action OnGroundReset { get => OnSurfaceReset[Direction.Ground]; set { OnSurfaceReset[Direction.Ground] = value; } }
        public Action OnRightWallReset { get => OnSurfaceReset[Direction.RightWall]; set { OnSurfaceReset[Direction.RightWall] = value; } }
        public Action OnLeftWallReset { get => OnSurfaceReset[Direction.LeftWall]; set { OnSurfaceReset[Direction.LeftWall] = value; } }
        public Action OnCeilingReset { get => OnSurfaceReset[Direction.Ceiling]; set { OnSurfaceReset[Direction.Ceiling] = value; } }

        #endregion accesors



        #region Default options (EdgeCorrection,Squish)

        /// <summary>
        /// Amount of edge correction when colliding with a wall<br/>
        /// </summary>
        int _sidewayCornerCorrection = 16;
        /// <summary>
        /// Amount of edge correction when colliding with a ceiling<br/>
        /// </summary>
        int _upwardCornerCorrection = 16;
        /// <summary>
        /// Example for onCollideH<br/>
        /// Tries to wiggle the character if it hits a wall, to see if it can move past that way<br/>
        /// This creates the illusion of smoother movement<br/>
        /// </summary>
        /// <param name="amount"></param>
        private void EdgeCorrectionH(float amount)
        {
            //Implement edge correction according to the direction of the X and Y speed
            if (_speed.X < 0)
            {
                
                if (_speed.Y <= 0)
                {
                    //Try to wigle the character up to _sidewayCornerCorrection times to see if it can move past that way
                    //Repeat for each other direction
                    for (int i = 1; i <= _sidewayCornerCorrection; i++)
                    {
                        if (character.Mover.Legal(new Point(-1, -i)))
                        {
                            _position += new Point(-1, -i);
                            return;
                        }
                    }
                }

                if (_speed.Y >= 0)
                {
                    for (int i = 1; i <= _sidewayCornerCorrection; i++)
                    {
                        if (character.Mover.Legal(new Point(-1, i)))
                        {
                            _position += new Point(-1, i);
                            return;
                        }
                    }
                }
                
            }

            if (_speed.X > 0)
            {
                if (_speed.Y <= 0)
                {
                    for (int i = 1; i <= _sidewayCornerCorrection; i++)
                    {
                        if (character.Mover.Legal(new Point(1, -i)))
                        {
                            _position += new Point(1, -i);
                            return;
                        }
                    }
                }

                if (_speed.Y >= 0)
                {
                    for (int i = 1; i <= _sidewayCornerCorrection; i++)
                    {
                        if (character.Mover.Legal(new Point(1, i)))
                        {
                            _position += new Point(1, i);
                            return;
                        }
                    }
                }
            }

            //Otherwise, set the speed to 0
            _speed.X = 0;
        }

        /// <summary>
        /// Example for onCollideV<br/>
        /// Tries to wiggle the character if it hits a wall, to see if it can move past that way<br/>
        /// This creates the illusion of smoother movement<br/>
        /// We only implement upward correction, as we don't want to make it easier to fall off of platforms<br/>
        /// </summary>
        /// <param name="amount"></param>
        private void EdgeCorrectionV(float amount)
        {
            //Implement edge correction according to the direction of the X and Y speed
            if (_speed.Y < 0)
            {
                if (_speed.X <= 0)
                {
                    //Try to wigle the character up to _upwardCornerCorrection times to see if it can move past that way
                    //Repeat for each other direction
                    for (int i = 1; i <= _upwardCornerCorrection; i++)
                    {
                        if (character.Mover.Legal(new Point(-i, -1)))
                        {
                            _position += new Point(-i, -1);
                            return;
                        }
                    }
                }

                if (_speed.X >= 0)
                {
                    for (int i = 1; i <= _upwardCornerCorrection; i++)
                    {
                        if (character.Mover.Legal(new Point(i, -1)))
                        {
                            _position += new Point(i, -1);
                            return;
                        }
                    }
                }
                
            }
            //Otherwise, set the speed to 0
            _speed.Y = 0;
        }

        /// <summary>
        /// Amount of pixels in direction X to wiggle when trying to avoid squishing<br/>
        /// </summary>
        int _squishWiggleX = 6;
        /// <summary>
        /// Amount of pixels in direction Y to wiggle when trying to avoid squishing<br/>
        /// </summary>
        int _squishWiggleY = 6;

        /// <summary>
        /// Example for Squish<br/>
        /// Tries to wiggle the character in a spiral pattern to avoid squishing<br/>
        /// </summary>
        /// <param name="amount"></param>
        public void SpiralCorrect(float amount)
        {
            //Create a spiral pattern
            List<Point> spiral = Tools.spiralOrder(_squishWiggleX * 2 + 1, _squishWiggleY * 2 + 1);
            
            //Try to wiggle the character in the spiral to find a legal position
            for (int i = 0; i < spiral.Count; i++)
            {
                if (character.Mover.Legal(spiral[i] - spiral[0]))
                {
                    _position += spiral[i] - spiral[0];
                    return;
                }
            }
            //If no legal position is found, Dispose the character
            character.Dispose();
        }


        #endregion

    }


    /// <summary>
    /// A struct for directions<br/>
    /// </summary>
    public struct Direction
    {
        public static readonly Point Up = new Point(0, -1);
        public static readonly Point Ceiling = new Point(0, -1);
        public static readonly Point Left = new Point(-1, 0);
        public static readonly Point LeftWall = new Point(-1, 0);
        public static readonly Point Down = new Point(0, 1);
        public static readonly Point Ground = new Point(0, 1);
        public static readonly Point Right = new Point(1, 0);
        public static readonly Point RightWall = new Point(1, 0);
        public static Point[] Directions = new Point[4]
        {
            new Point(0, -1),
            new Point(0, 1),
            new Point(-1, 0),
            new Point(1, 0),
        };
    }
}

