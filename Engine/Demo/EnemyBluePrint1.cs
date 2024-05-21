using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// An abandoned try to create an enemy blueprint class to streamline enemy creation<br/>
    /// Will be replaced by Characters<br/>
    /// It is based on the player class<br/>
    /// </summary>
    class EnemyBluePrint1 : Actor
    {
        /// <summary>
        /// The animated component corresponding to this class (private)<br/>
        /// </summary>
        public AnimatedGameComponent _privateAnimatedComponent = null;

        /// <summary>
        /// The animated component corresponding to this class<br/>
        /// Can only be set once<br/>
        /// </summary>
        public AnimatedGameComponent _animatedComponent
        {
            get => _privateAnimatedComponent;
            set
            {
                if (_privateAnimatedComponent == null)
                {
                    _privateAnimatedComponent = value; _sprite = value; return;
                }
                if (value == _privateAnimatedComponent) return; throw new Exception("Trying to reset _animatedComponent");
            }
        }

        /// <summary>
        /// Which direction the enemy is facing<br/>
        /// </summary>
        private int _facing;

        /// <summary>
        /// The standing hitbox of the enemy<br/>
        /// </summary>
        private readonly Rectangle StandRect = new Rectangle(new Point(-14, -16), new Point(28, 38));

        /// <summary>
        /// The speed of the enemy<br/>
        /// </summary>
        public Vector2 _speed;

        /// <summary>
        /// The intention of the enemy to move in the X axis<br/>
        /// This will be controlled by the enemy AI<br/>
        /// </summary>
        private float _moveX;

        /// <summary>
        /// The maximum fall speed of the enemy<br/>
        /// </summary>
        private float _maxFall;

        /// <summary>
        /// Whether or not the enemy is on the ground<br/>
        /// </summary>
        public bool _onGround = false;


        /// <summary>
        /// The modifier fot the speed of acceleration and deceleration in the air<br/>
        /// </summary>
        public const float AirMult = 0.65f;
        
        /// <summary>
        /// The maximum horizontal speed of the enemy<br/>
        /// </summary>
        public const float MaxRun = 90f;

        /// <summary>
        /// The decceleration of the enemy when stopping while running<br/>
        /// </summary>
        private const float RunReduce = 400f;
        
        /// <summary>
        /// The acceleration of the enemy when running<br/>
        /// </summary>
        private const float RunAccel = 1000f;
        
        /// <summary>
        /// The horizontal boost of the enemy when jumping<br/>
        /// </summary>
        private const float JumpHBoost = 50f;
        
        /// <summary>
        /// The vertical speed of the enemy when jumping<br/>
        /// </summary>
        private const float JumpSpeed = 200f;
        
        /// <summary>
        /// How strong does Gravity effects the enemy<br/>
        /// </summary>
        private const float Gravity = 1000f;

        /// <summary>
        /// What is the treshold for the enemy to have only half the gravity applying to it (makes jumps more responsive)<br/>
        /// </summary>
        private const float HalfGravThreshold = 40f;
        
        /// <summary>
        /// Maximum passive fall speed of the enemy<br/>
        /// </summary>
        private const float MaxFall = 160f;
        
        /// <summary>
        /// Maximum active fall speed of the enemy (Equivalent to pressing the down button while falling with the player)<br/>
        /// </summary>
        private const float FastMaxFall = 240f;
        
        /// <summary>
        /// The acceleration of the enemy when falling<br/>
        /// </summary>
        private const float FastMaxAccel = 300f;
        
        /// <summary>
        /// The friction of the enemy when ducking<br/>
        /// </summary>
        private const float DuckFriction = 500f;

        /// <summary>
        /// The possible states of the enemy<br/>
        /// </summary>
        enum States
        {
            Default,
            Crouch,
            Run,
            Jump,
            Soar,
            AirSpin,
            Fall,
            Slide
        };

        /// <summary>
        /// The current state of the enemy<br/>
        /// </summary>
        States state;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnemyBluePrint1"/> class.<br/>
        /// Adds the stand hitbox to the collider<br/>
        /// </summary>
        /// <param name="texture"> The sprite sheet.</param>
        public EnemyBluePrint1(Texture2D texture) : base(texture)
        {
            _colliderSensor.AddHitbox(StandRect);

        }

        /// <summary>
        /// LoadContent,<br/>
        /// Initializes the animatedComponent if it is not already initialized<br/>
        /// Sets up the animation<br/>
        /// </summary>
        protected override void LoadContent()
        {
            if (_animatedComponent == null) _animatedComponent = new AnimatedGameComponent(_texture);
            _sprite = _animatedComponent;

            SetUpAnimations();

            base.LoadContent();
        }

        /// <summary>
        /// Update<br/>
        /// Selects the animation based on the current state<br/>
        /// Checks if the enemy is on the ground<br/>
        /// Handles the facing of the enemy<br/>
        /// Calculates the speed of the enemy<br/>
        /// Moves the enemy<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {//Debug.WriteLine("speed: "+_speed);

            SelectAnimation();

            _onGround = CheckOnGround();

            _facing = Math.Sign(_moveX);
            if (_facing == 1)
            {
                _animatedComponent._spriteEffect = SpriteEffects.None;
            }
            else if (_facing == -1)
            {
                _animatedComponent._spriteEffect = SpriteEffects.FlipHorizontally;
            }

            _speed.Y = CalculateSpeedY();
            //Running and Friction
            _speed.X = CalculateSpeedX();

            MoveX(this._speed.X * Game.I._deltaTime, new Actor.Action(OnCollideH));
            MoveY(this._speed.Y * Game.I._deltaTime, new Actor.Action(OnCollideV));


            base.Update(gameTime);
        }

        /// <summary>
        /// Calculates the speed of the enemy in the X direction<br/>
        /// </summary>
        /// <returns></returns>
        private float CalculateSpeedX()
        {

            //Apply the multiplier if we are in the air
            float mult = _onGround ? 1 : AirMult;


            //Is it running
            if (Math.Abs(_speed.X) > MaxRun && Math.Sign(_speed.X) == _moveX)
                return Calc.Approach(_speed.X, MaxRun * _moveX, RunReduce * mult * Game.I._deltaTime);  //Reduce back from beyond the max speed
            else
                return Calc.Approach(_speed.X, MaxRun * _moveX, RunAccel * mult * Game.I._deltaTime);

        }

        /// <summary>
        /// Calculates the speed of the enemy in the Y direction<br/>
        /// </summary>
        /// <returns></returns>
        private float CalculateSpeedY()
        {


            //Get current max fall speed
            float mf = MaxFall;
            float fmf = FastMaxFall;
            _maxFall = Calc.Approach(_maxFall, mf, FastMaxAccel * Game.I._deltaTime);

            //Calculate the speed
            float returnValue = Calc.Approach(_speed.Y, _maxFall, Gravity * Game.I._deltaTime);

            return returnValue;
        }


        /// <summary>
        /// Sets up the animation<br/>
        /// This works for the player character sprite sheet<br/>
        /// </summary>
        private void SetUpAnimations()
        {
            //_animatedComponent._shader = TestGame.game.Content.Load<Effect>("Glow");
            List<Rectangle> frameRect = Tools.SplitTileSheet(_texture.Width / 7, _texture.Height / 16, 7, 16);
            _animatedComponent._animations.Add("Default", new Animation(frameRect.GetRange(0, 4)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("Crouch", new Animation(frameRect.GetRange(4, 4)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("Run", new Animation(frameRect.GetRange(8, 6)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("Jump", new Animation(frameRect.GetRange(14, 4)) { _animationStepSpeed = 4 });
            _animatedComponent._animations.Add("Soar", new Animation(frameRect.GetRange(17, 1)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("AirSpin", new Animation(frameRect.GetRange(18, 4)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("Fall", new Animation(frameRect.GetRange(22, 2)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("Slide", new Animation(frameRect.GetRange(24, 2)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("SlideEnd", new Animation(frameRect.GetRange(26, 3)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("Hang", new Animation(frameRect.GetRange(29, 4)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("HangPull", new Animation(frameRect.GetRange(33, 5)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("SwordDefault", new Animation(frameRect.GetRange(38, 4)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("SwordBigHit1", new Animation(frameRect.GetRange(42, 6)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("SwordBigHitIdle1", new Animation(frameRect.GetRange(48, 2)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("SwordBigHit2", new Animation(frameRect.GetRange(50, 3)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("SwordBigHit3", new Animation(frameRect.GetRange(53, 6)) { _animationStepSpeed = 8 });
            //_animatedComponent._animations.Add("DemoSwordCombo", new Animation(frameRect.GetRange(42, 6+6+5)) { _animationStepSpeed = 8 });


            _animatedComponent.ChangeAnimationState("SwordDefault");
        }

        /// <summary>
        /// Checks if the enemy is on the ground<br/>
        /// </summary>
        /// <returns></returns>
        private bool CheckOnGround()
        {
            return (_speed.Y >= 0) && (_colliderSensor.CollideAt(Position + new Point(0, 1), onlySolids)._isColliding || _colliderSensor.CollideOutsideAt(Position + new Point(0, 1), onlyJumpThroughs)._isColliding);
        }

        /// <summary>
        /// Selects the appropriate State and animation based on the other defined functions<br/>
        /// </summary>
        private void SelectAnimation()
        {
            if (isCrouching())
            {
                if (_animatedComponent._currentAnimationState != "Crouch")
                {
                    state = States.Crouch;
                    _animatedComponent.ChangeAnimationState("Crouch");
                }
            }
            else if (isRunning())
            {
                if (_animatedComponent._currentAnimationState != "Run")
                {
                    state = States.Run;
                    _animatedComponent.ChangeAnimationState("Run");
                }
            }
            else if (isDefault())
            {
                if (_animatedComponent._currentAnimationState != "Default")
                {
                    state = States.Default;
                    _animatedComponent.ChangeAnimationState("Default");
                }
            }
            else if (isJumping())
            {
                if (_animatedComponent._currentAnimationState != "Jump" && _animatedComponent._currentAnimationState != "Soar")
                {
                    state = States.Jump;
                    _animatedComponent.ChangeAnimationState("Jump");
                    _animatedComponent.ChangeNextAnimationState("Soar");
                }
            }
            else if (isAirSpin())
            {
                if (_animatedComponent._currentAnimationState != "AirSpin")
                {
                    state = States.AirSpin;
                    _animatedComponent.ChangeNextAnimationState("AirSpin");
                }
            }
            else if (isFalling())
            {
                if (_animatedComponent._currentAnimationState == "AirSpin")
                {
                    state = States.Fall;
                    _animatedComponent.ChangeNextAnimationState("Fall");
                }
                else if (_animatedComponent._currentAnimationState != "AirSpin" && _animatedComponent._currentAnimationState != "Fall")
                {
                    state = States.Fall;
                    _animatedComponent.ChangeAnimationState("Fall");
                }
            }
        }

        /// <summary>
        /// Checks if the enemy is falling (Not Implemented)<br/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool isFalling()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the enemy is spinning in the air (Not Implemented)<br/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool isAirSpin()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the enemy is jumping (Not Implemented)<br/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool isJumping()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the enemy should be in default state (Not Implemented)<br/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool isDefault()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the enemy is running (Not Implemented)<br/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool isRunning()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the enemy is crouching (Not Implemented)<br/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool isCrouching()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Function called when the enemy collides with something horizontally<br/>
        /// Sets the x speed to 0<br/>
        /// </summary>
        /// <param name="amount"></param>
        private void OnCollideH(float amount)
        {
            _speed.X = 0;


        }
        /// <summary>
        /// Function called when the enemy collides with something vertically<br/>
        /// Sets the y speed to 0<br/>
        /// </summary>
        /// <param name="amount"></param>
        private void OnCollideV(float amount)
        {

            _speed.Y = 0;
        }


    }
}