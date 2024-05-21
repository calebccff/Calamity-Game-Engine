using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;





namespace TestShader
{

    /// <summary>
    /// The current Player component<br/>
    /// Will be replaced with a character based version<br/>
    /// Implements logic from the publicized code snippets from Celeste (for more details check out the readme)<br/>
    /// </summary>
    public class Player : Actor
    {
        /// <summary>
        /// The input button for Jump. It may be rebound dynamically (Thanks to the input manager class from the Monocle engine of Maddy Thorson)<br/>
        /// </summary>
        public VirtualButton _inputJump;

        /// <summary>
        /// The input button for Ducking. It may be rebound dynamically (Thanks to the input manager class from the Monocle engine of Maddy Thorson)<br/>
        /// </summary>
        public VirtualButton _inputDuck;

        /// <summary>
        /// The axis input for movement left and right. It may be rebound dynamically (Thanks to the input manager class from the Monocle engine of Maddy Thorson)<br/>
        /// </summary>
        public VirtualAxis _inputMoveX;

        /// <summary>
        /// The axis input for movement up and down. It may be rebound dynamically (Thanks to the input manager class from the Monocle engine of Maddy Thorson)<br/>
        /// </summary>
        public VirtualIntegerAxis _inputMoveY;

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
        /// The particle type of the dust particles emitted on landing or jumping<br/>
        /// </summary>
        public ParticleType LandingDust;
        
        /// <summary>
        /// The number of dust particles emitted on landing<br/>
        /// </summary>
        private readonly int LandingDustCount = 2;

        /// <summary>
        /// The particle type of the dust particles emitted on jumping<br/>
        /// </summary>
        private readonly int JumpingDustCount = 6;

        /// <summary>
        /// The number of dust particles emitted on fast landing (pressing the down button while landing)<br/>
        /// </summary>
        private const int FastLandingDustCount = 10;

        /// <summary>
        /// The particle system of the player<br/>
        /// </summary>
        public ParticleSystem PlayerParticles;

        /// <summary>
        /// The Hitbox of the player when standing<br/>
        /// </summary>
        private Rectangle StandRect = new Rectangle(new Point(-14,  - 16), new Point(28, 38));

        /// <summary>
        /// The Hitbox of the player when ducking<br/>
        /// </summary>
        private Rectangle CrouchRect = new Rectangle(new Point(-14, 0), new Point(28, 22));

        /// <summary>
        /// The modifier for deceleration while ducking (Not Implemented)<br/>
        /// </summary>
        private const float DuckReduce = 500f;

        /// <summary>
        /// The modifier for acceleration while ducking (Not Implemented)<br/>
        /// </summary>
        private const float DuckAccel = 700f;

        /// <summary>
        /// The number of Air jumps allowed, resets on touching the ground<br/>
        /// </summary>
        private const int MaxExtraJumps = 1;

        /// <summary>
        /// The time after jumping when pressing the jump button still maintains upward speed (thus jumping higher when holding the jump button)<br/>
        /// </summary>
        private const float VarJumpTime = 0.2f;

        /// <summary>
        /// The amount of time after leaving the ground where we can still jump (Coyote time),<br/>
        /// </summary>
        private const float JumpGraceTime = 0.2f;

        //TODO: Test if extrajumpgrace would be more convinient to be the same as jumpgrace

        /// <summary>
        /// The amount of time after leaving the ground where airjump is disabled (to avoid the player using airjump without knowing about it)<br/>
        /// </summary>
        private const float ExtraJumpGraceTime = 0.2f;

        /// <summary>
        /// Hitting the ceiling or ground in a jump after this time, disables varjump (holding the jump button for larger jump)<br/>
        /// </summary>
        private const float CeilingVarJumpGrace = .05f;

        /// <summary>
        /// The amount of pixels to try to wiggle the player when stuck in a jump because of hitting the ceiling<br/>
        /// Looking for position where jump can continue.<br/>
        /// </summary>
        private const int _upwardCornerCorrection = 4;

        /// <summary>
        /// The amount of pixels to try to wiggle the player when stuck in a jump because of hitting a wall<br/>
        /// Looking for position where jump can continue.<br/>
        /// </summary>
        private const int _sidewayCornerCorrection = 3;

        //TODO: Reset squishwiggle to sth sensible

        /// <summary>
        /// The amount of pixels to wiggle the player when squished by an object in the x direction<br/>
        /// The search is in a spiral patter from the original position<br/>
        /// Looking for a position where player is not dead<br/>
        /// The solids move in two steps (x and y), so squish may only trigger twice a move (thus won't move player too far)<br/>
        /// </summary>
        private const int _squishWiggleX = 10;

        /// <summary>
        /// The amount of pixels to wiggle the player when squished by an object in the y direction<br/>
        /// The search is in a spiral patter from the original position<br/>
        /// Looking for a position where player is not dead<br/>
        /// The solids move in two steps (x and y), so squish may only trigger twice a move (thus won't move player too far)<br/>
        /// </summary>
        private const int _squishWiggleY = 10;

        /// <summary>
        /// The speed recorded at the start of the jump. Used to maintain speed if jump button is held<br/>
        /// </summary>
        public float _varJumpSpeed;

        /// <summary>
        /// The current remaining time to hold the jump button for maintaining y speed<br/>
        /// </summary>
        public float _varJumpTimer;

        /// <summary>
        /// The current time left after leaving the ground where we can still jump (Coyote time)<br/>
        /// </summary>
        private float _jumpGraceTimer;

        /// <summary>
        /// The current time left after leaving the ground where airjump is disabled (to avoid the player using airjump without knowing about it)<br/>
        /// </summary>
        private float _extraJumpGraceTimer;

        /// <summary>
        /// The number of air jumps the player has left (reset when touching the ground)<br/>
        /// </summary>
        public int _extraJumps;

        /// <summary>
        /// The player input for the movement in the x direction<br/>
        /// </summary>
        private float _moveX;

        /// <summary>
        /// The current maximal falling speed the player approaches<br/>
        /// </summary>
        private float _maxFall;

        /// <summary>
        /// The way the player is facing (1 for right -1 for left)<br/>
        /// </summary>
        private int _facing;

        /// <summary>
        /// Is the player ducking<br/>
        /// </summary>
        public bool _ducking;

        /// <summary>
        /// If the player is standing on the ground (can't have negative y speed)<br/>
        /// </summary>
        public bool _onGround = false;

        /// <summary>
        /// The highes position achieved in a jump (Not yet in use)<br/>
        /// </summary>
        public int _highestAirY;

        /// <summary>
        /// The speed of the player<br/>
        /// </summary>
        public Vector2 _speed;

        /// <summary>
        /// If the player is dying this tick.<br/>
        /// Currently used for cleanUp<br/>
        /// Later to be used for death animation<br/>
        /// </summary>
        public bool _isDying = false;

        /// <summary>
        /// The id of the player in the current scene<br/>
        /// </summary>
        public int _playerId = 0;

        /// <summary>
        /// The audio emitter corresponding to the player (Not yet in use)<br/>
        /// </summary>
        AudioEmitter _audioEmitter = new AudioEmitter();
        
        /// <summary>
        /// The animated component corresponding to the player (Private)<br/>
        /// </summary>
        public AnimatedGameComponent _privateAnimatedComponent = null;

        /// <summary>
        /// The animated component corresponding to the player<br/>
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
        /// Initializes a new instance of the <see cref="Player"/> class.<br/>
        /// Sets the current hitbox to Standing<br/>
        /// Resets the number extra jumps<br/>
        /// </summary>
        /// <param name="texture">The loaded sprite sheet</param>
        public Player(Texture2D texture) : base(texture)
        {

            _colliderSensor.AddHitbox(StandRect);

            spriteOffset= new Point(0, -38);

            _extraJumps = MaxExtraJumps;
        }

        /// <summary>
        /// LoadContent,<br/>
        /// Initializes animated component<br/>
        /// Sets up controls<br/>
        /// Initializes particle system<br/>
        /// Creates landing dust particle type<br/>
        /// Sets up animations<br/>
        /// </summary>
        protected override void LoadContent()
        {

            if (_animatedComponent == null) _animatedComponent = new AnimatedGameComponent(_texture);
            SetControls();


            PlayerParticles = new ParticleSystem(0, 500);
            SetUpLandingDust();
            SetUpAnimations();


            base.LoadContent();
        }

        /// <summary>
        /// Create the animation dictionary in the animated component by subdividing the sprite sheet<br/>
        /// Sets the current animation to default<br/>
        /// </summary>
        private void SetUpAnimations()
        {
            //_animatedComponent._shader = TestGame.game.Content.Load<Effect>("Glow");
            List<Rectangle> frameRect = Tools.SplitTileSheet(_texture.Width / 28, _texture.Height / 14, 28, 14);
            _animatedComponent._animations.Add("SwordDefault", new Animation(frameRect.GetRange(0, 8)) { _animationStepSpeed = 4 });
            _animatedComponent._animations.Add("Default", new Animation(frameRect.GetRange(0, 8)) { _animationStepSpeed = 4 });
            _animatedComponent._animations.Add("Run", new Animation(frameRect.GetRange(28, 8)) { _animationStepSpeed = 4 });
            _animatedComponent._animations.Add("Jump", new Animation(frameRect.GetRange(4*28, 8)) { _animationStepSpeed = 4 });
            _animatedComponent._animations.Add("Soar", new Animation(frameRect.GetRange(4*28+8, 1)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("Fall", new Animation(frameRect.GetRange(4*28+9, 7)) { _animationStepSpeed = 4 });
            _animatedComponent._animations.Add("AirSpin", new Animation(frameRect.GetRange(6*28+2, 4)) { _animationStepSpeed = 8 });
            _animatedComponent._animations.Add("Crouch", new Animation(frameRect.GetRange(11*28+2, 6)) { _animationStepSpeed = 8 });

            //_animatedComponent._animations.Add("Slide", new Animation(frameRect.GetRange(24, 2)) { _animationStepSpeed = 8 });
            //_animatedComponent._animations.Add("SlideEnd", new Animation(frameRect.GetRange(26, 3)) { _animationStepSpeed = 8 });
            //_animatedComponent._animations.Add("Hang", new Animation(frameRect.GetRange(29, 4)) { _animationStepSpeed = 8 });
            //_animatedComponent._animations.Add("HangPull", new Animation(frameRect.GetRange(33, 5)) { _animationStepSpeed = 8 });
            //_animatedComponent._animations.Add("SwordDefault", new Animation(frameRect.GetRange(38, 4)) { _animationStepSpeed = 8 });
            //_animatedComponent._animations.Add("SwordBigHit1", new Animation(frameRect.GetRange(42, 6)) { _animationStepSpeed = 8 });
            //_animatedComponent._animations.Add("SwordBigHitIdle1", new Animation(frameRect.GetRange(48, 2)) { _animationStepSpeed = 8 });
            //_animatedComponent._animations.Add("SwordBigHit2", new Animation(frameRect.GetRange(50, 3)) { _animationStepSpeed = 8 });
            //_animatedComponent._animations.Add("SwordBigHit3", new Animation(frameRect.GetRange(53, 6)) { _animationStepSpeed = 8 });
            //_animatedComponent._animations.Add("DemoSwordCombo", new Animation(frameRect.GetRange(42, 6+6+5)) { _animationStepSpeed = 8 });


            _animatedComponent.ChangeAnimationState("SwordDefault");
        }

        /// <summary>
        /// Creates the landing dust particle type<br/>
        /// Sets the texture, animation and constants<br/>
        /// </summary>
        private void SetUpLandingDust()
        {
            LandingDust = new ParticleType();
            LandingDust._texture = TextureManager.GetTexture("landingCloudPlayer");
            LandingDust._anim = new Animation(Tools.SplitTileSheet(LandingDust._texture.Width / 3, LandingDust._texture.Height / 3, 3, 3).GetRange(0, 7));
            LandingDust._anim._animationStepSpeed = 4;
            LandingDust.LifeMin = 0.2f;
            LandingDust.LifeMax = 0.6f;
            LandingDust._scale = 0.2f;
            LandingDust.SpinMin = -5f;
            LandingDust.SpinMax = 5f;
            LandingDust.RotationMode = ParticleType.RotationModes.Random;
            //LandingDust.Gravity = 100f;
            //LandingDust.MaxFall = 200f;
            LandingDust.Direction = (float)Math.PI / 2;
            LandingDust.DirectionRange = 1;
            LandingDust.SpeedMin = 0;
            LandingDust.SpeedMax = 30;
            LandingDust.SizeRange = 0.1f;
            LandingDust.FadeMode = ParticleType.FadeModes.Linear;
        }


        /// <summary>
        /// Sets the controls of the player depending on their number and on the id (Player 0 and 1 have different default controls when we have 2 players)<br/>
        /// </summary>
        private void SetControls()
        {
            if (Game.I.numPlayers == 2)
            {
                if (_playerId == 0)
                {
                    _inputJump = new VirtualButton(0.31f, new VirtualButton.KeyboardKey(Keys.W), new VirtualButton.PadButton(0, Buttons.A));
                    _inputMoveX = new VirtualAxis(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehaviors.TakeNewer, Keys.A, Keys.D), new VirtualAxis.PadLeftStickX(0, 1));
                    _inputMoveY = new VirtualIntegerAxis(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehaviors.TakeNewer, Keys.W, Keys.S), new VirtualAxis.PadLeftStickY(0, 1));
                    _inputDuck = new VirtualButton(0.31f, new VirtualButton.KeyboardKey(Keys.S), new VirtualButton.PadLeftStickDown(0, 1));

                }
                if (_playerId == 1)
                {
                    _inputJump = new VirtualButton(0.31f, new VirtualButton.KeyboardKey(Keys.Up));
                    _inputMoveX = new VirtualAxis(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehaviors.TakeNewer, Keys.Left, Keys.Right));
                    _inputMoveY = new VirtualIntegerAxis(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehaviors.TakeNewer, Keys.Up, Keys.Down));
                    _inputDuck = new VirtualButton(0.31f, new VirtualButton.KeyboardKey(Keys.Down));

                }
            }
            else
            {
                _inputJump = new VirtualButton(0.31f, new VirtualButton.KeyboardKey(Keys.Up), new VirtualButton.KeyboardKey(Keys.Space), new VirtualButton.KeyboardKey(Keys.W), new VirtualButton.PadButton(0, Buttons.A));
                _inputMoveX = new VirtualAxis(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehaviors.TakeNewer, Keys.Left, Keys.Right), new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehaviors.TakeNewer, Keys.A, Keys.D), new VirtualAxis.PadLeftStickX(0, 1));
                _inputMoveY = new VirtualIntegerAxis(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehaviors.TakeNewer, Keys.Up, Keys.Down), new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehaviors.TakeNewer, Keys.W, Keys.S), new VirtualAxis.PadLeftStickY(0, 1));
                _inputDuck = new VirtualButton(0.31f, new VirtualButton.KeyboardKey(Keys.Down), new VirtualButton.KeyboardKey(Keys.S), new VirtualButton.PadLeftStickDown(0, 1));
            }
        }

        /// <summary>
        /// Update,<br/>
        /// Handle Death<br/>
        /// Set audio emitter position<br/>
        /// Check and handle being on the ground<br/>
        /// Update moveX<br/>
        /// Check and handle ducking and unducking (_ducking variable and update collider)<br/>
        /// Select currently playing animation<br/>
        /// Update facing<br/>
        /// Calculate speed<br/>
        /// Update timers<br/>
        /// Handle varjumping and jumping<br/>
        /// Move<br/>
        /// Update Highest air<br/>
        /// Update collider sensor<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {

            if (_isDying)
            {
                Dispose();

                return;
            }


            _audioEmitter.Position = new Vector3(Position.X, Position.Y, 0);


            //Is is on ground

            _onGround = CheckOnGround();

            if (_onGround)
            {
                OnGroundReset();
            }


            _moveX = _inputMoveX.Value;

            //DONE: Implement CanDuck() and  CanUnduck()
            //TODO: Move while ducked
            if (canDuck())
            {
                if (!_ducking)
                {
                    _ducking = true;
                    _colliderSensor.RemoveAtHitbox(0);
                    _colliderSensor.AddHitbox(CrouchRect);
                }
            }
            else if (canUnduck() && _ducking)
            {
                _ducking = false;
                _colliderSensor.RemoveAtHitbox(0);
                _colliderSensor.AddHitbox(StandRect);
            }



            //TODO: Introduce states, and index hitboxes with them

            SelectAnimation();

            _facing = Math.Sign(_moveX);
            if (_facing == 1)
            {
                _animatedComponent._spriteEffect = SpriteEffects.None;
            }
            else if (_facing == -1)
            {
                _animatedComponent._spriteEffect = SpriteEffects.FlipHorizontally;
            }
            //TODO: Jump Thru Assist


            _speed.Y = CalculateSpeedY();
            //Running and Friction
            _speed.X = CalculateSpeedX();
            UpdateTimers();
            //Variable Jumping
            if (_varJumpTimer > 0)
            {
                if (_inputJump.Check)
                    _speed.Y = Math.Min(_speed.Y, _varJumpSpeed);
                else
                    _varJumpTimer = 0;
            }
            if (canJump())
            {
                Jump();
            }

            //Debug.WriteLine("speed: "+_speed);
            MoveX(this._speed.X * Game.I._deltaTime, new Actor.Action(OnCollideH));
            MoveY(this._speed.Y * Game.I._deltaTime, new Actor.Action(OnCollideV));



            CalculateHighestAir();

            _colliderSensor.Update(gameTime);
            base.Update(gameTime);

        }

        /// <summary>
        /// Check if player can unduck<br/>
        /// (By hitbox)<br/>
        /// </summary>
        /// <returns></returns>
        private bool canUnduck()
        {
            _colliderSensor.AddHitbox(StandRect);
            bool isColliding = _colliderSensor.CollideAt(Position, onlySolids)._isColliding;
            _colliderSensor.RemoveAtHitbox(_colliderSensor._hitboxes.Count - 1);

            return !isColliding;
        }

        /// <summary>
        /// Check if player is ducking<br/>
        /// On the ground and presses duck button<br/>
        /// </summary>
        /// <returns></returns>
        private bool canDuck()
        {
            return _inputDuck.Check && _onGround;
        }

        /// <summary>
        /// Check if on the fround<br/>
        /// touching ground and Y speed not negative<br/>
        /// </summary>
        /// <returns></returns>
        private bool CheckOnGround()
        {
            return (_speed.Y >= 0) && (_colliderSensor.CollideAt(Position + new Point(0, 1), onlySolids)._isColliding || _colliderSensor.CollideOutsideAt(Position + new Point(0, 1), onlyJumpThroughs)._isColliding);
        }

        /// <summary>
        /// Reset variables after touching the ground<br/>
        /// _jumpGraceTimer, _extraJumpGraceTimer, _extraJumps<br/>
        /// </summary>
        private void OnGroundReset()
        {
            _jumpGraceTimer = JumpGraceTime;
            _extraJumpGraceTimer = ExtraJumpGraceTime;
            _extraJumps = Math.Max(MaxExtraJumps, _extraJumps);
        }

        /// <summary>
        /// Update timers by delta time<br/>
        /// _varJumpTimer, _jumpGraceTimer, _extraJumpGraceTimer<br/>
        /// </summary>
        private void UpdateTimers()
        {
            if (_varJumpTimer > 0)
                _varJumpTimer -= Math.Min(Game.I._deltaTime, _varJumpTimer);

            if (_jumpGraceTimer > 0)
                _jumpGraceTimer -= Math.Min(Game.I._deltaTime, _jumpGraceTimer);

            if (_extraJumpGraceTimer > 0)
                _extraJumpGraceTimer -= Math.Min(Game.I._deltaTime, _extraJumpGraceTimer);
        }

        /// <summary>
        /// Select current animation (and sometime next animation)<br/>
        /// based on isXY() functions<br/>
        /// </summary>
        private void SelectAnimation()
        {
            if (isCrouching())
            {
                if (_animatedComponent._currentAnimationState != "Crouch")
                {
                    _animatedComponent.ChangeAnimationState("Crouch");
                }
            }
            else if (isRunning())
            {
                if (_animatedComponent._currentAnimationState != "Run")
                {
                    _animatedComponent.ChangeAnimationState("Run");
                }
            }
            else if (isDefault())
            {
                if (_animatedComponent._currentAnimationState != "Default")
                {
                    _animatedComponent.ChangeAnimationState("Default");
                }
            }
            else if (isRising())
            {
                if (_animatedComponent._currentAnimationState != "Jump" && _animatedComponent._currentAnimationState != "Soar")
                {
                    _animatedComponent.ChangeAnimationState("Jump");
                    _animatedComponent.ChangeNextAnimationState("Soar");
                }
            }
            else if (isAirSpin())
            {
                if (_animatedComponent._currentAnimationState != "AirSpin")
                {
                    _animatedComponent.ChangeNextAnimationState("AirSpin");
                }
            }
            else if (isFalling())
            {
                if (_animatedComponent._currentAnimationState == "AirSpin")
                {
                    _animatedComponent.ChangeNextAnimationState("Fall");
                }
                else if (_animatedComponent._currentAnimationState != "AirSpin" && _animatedComponent._currentAnimationState != "Fall")
                {
                    _animatedComponent.ChangeAnimationState("Fall");
                }
            }
        }

        /// <summary>
        /// Perform a Jump<br/>
        /// Sets the speed, emits particles, consume jump input buffer<br/>
        /// Update variables (_extraJumps,_varJumpSpeed,_varJumpTimer)<br/>
        /// </summary>
        public void Jump()
        {
            //If we are air jumping, reduce the remaining number of air jumps
            if (_extraJumpGraceTimer == 0) _extraJumps--;

            //Reset the jump input (the jump input implements a buffer)
            _inputJump.ConsumeBuffer();

            //Set speed
            _speed.X += JumpHBoost * _moveX;
            _speed.Y = -JumpSpeed;

            //Set variables for var jump
            _varJumpSpeed = _speed.Y;
            _varJumpTimer = VarJumpTime;

            //Emit particles
            EmitJumpParticles();
        }

        /// <summary>
        /// Emit dust particles when jumping<br/>
        /// </summary>
        private void EmitJumpParticles()
        {
            PlayerParticles.Emit(LandingDust, JumpingDustCount, Position.ToVector2() + new Vector2(0, 14), new Vector2(8, 2));
        }


        /// <summary>
        /// Check if player is jumping<br/>
        /// Button pressed, and is in jumpGrace<br/>
        /// Or button pressed after extraJumpGrace and has remaining air jump<br/>
        /// </summary>
        /// <returns></returns>
        private bool canJump()
        {

            return (_jumpGraceTimer > 0 && _inputJump.Pressed) || (_extraJumpGraceTimer == 0 && _extraJumps > 0 && _inputJump.Pressed);
        }

        /// <summary>
        /// Update the highest Y position in the current jump<br/>
        /// (Current position if on the ground)<br/>
        /// </summary>
        private void CalculateHighestAir()
        {
            if (_onGround)
                _highestAirY = (int)Position.Y;
            else
                _highestAirY = Math.Min((int)Position.Y, _highestAirY);
        }


        /// <summary>
        /// Calculate the current speed in the X axis<br/>
        /// </summary>
        /// <returns></returns>
        private float CalculateSpeedX()
        {
            //Check if currently ducking
            if (_ducking && _onGround)
            {
                //Reduce speed to zero (currently not possible to move while ducking)
                return Calc.Approach(_speed.X, 0, DuckFriction * Game.I._deltaTime);

                //Possible implementation for movement while ducking
                /*float mult = _onGround ? 1 : AirMult;

                float max = MaxRun;
                if (Math.Abs(_speed.X) > max && Math.Sign(_speed.X) == _moveX)
                    return Calc.Approach(_speed.X, max * _moveX, DuckReduce * mult * TestGame.game._deltaTime);
                else
                    return Calc.Approach(_speed.X, max * _moveX, DuckAccel * mult * TestGame.game._deltaTime);
                */
            }
            else
            {
                //Calculate the acceleration multiplier (smaller if we are in the air)
                float mult = _onGround ? 1 : AirMult;

                //Calculate the speed we are approaching (might modify calculation later) 
                float max = MaxRun;

                //Depending on if we are moving in the same direction or currently turning around
                //approach the desired speed according to the acceleration and decceleration used for running
                if (Math.Abs(_speed.X) > max && Math.Sign(_speed.X) == _moveX)
                    return Calc.Approach(_speed.X, max * _moveX, RunReduce * mult * Game.I._deltaTime);
                else
                    return Calc.Approach(_speed.X, max * _moveX, RunAccel * mult * Game.I._deltaTime);
            }
        }

        /// <summary>
        /// Calculate the current speed in the Y axis<br/>
        /// Handles landing<br/>
        /// </summary>
        /// <returns></returns>
        private float CalculateSpeedY()
        {


            //Calculate current max fall speed
            {
                float mf = MaxFall;
                float fmf = FastMaxFall;

                //Fast Fall
                if (_inputMoveY == 1 && _speed.Y >= mf)
                {
                    _maxFall = Calc.Approach(_maxFall, fmf, FastMaxAccel * Game.I._deltaTime);
                }
                else
                    _maxFall = Calc.Approach(_maxFall, mf, FastMaxAccel * Game.I._deltaTime);
            }
            float mult = (Math.Abs(_speed.Y) < HalfGravThreshold && (_inputJump.Check)) ? .5f : 1f;

            float max = _maxFall;
            float returnValue = Calc.Approach(_speed.Y, max, Gravity * mult * Game.I._deltaTime);
            if (_onGround)
            {
                if (_speed.Y > 0)
                {
                    HandleLanding();
                }
                returnValue = Math.Min(0, returnValue);
            }
            return returnValue;
        }

        /// <summary>
        /// Called when player touches the ground whith a downward speed<br/>
        /// Emits dust particles<br/>
        /// </summary>
        private void HandleLanding()
        {
            EmitLandingParticles();
        }

        /// <summary>
        /// Check if player is currently falling<br/>
        /// Not on the ground with a downward speed<br/>
        /// </summary>
        /// <returns></returns>
        private bool isFalling()
        {
            return !_onGround && _speed.Y > 0;
        }

        /// <summary>
        /// Check if player is currently rising<br/>
        /// Not on the ground with an upward speed<br/>
        /// </summary>
        /// <returns></returns>
        private bool isRising()
        {
            return !_onGround && _speed.Y < 0;
        }

        /// <summary>
        /// Check if the player can be in a default position<br/>
        /// If the player is on the ground<br/>
        /// </summary>
        /// <returns></returns>
        private bool isDefault()
        {
            return _onGround&& !isRunning() && !isCrouching();
        }

        /// <summary>
        /// Check if Player should currently spin in the air<br/>
        /// If player is not on the ground, and is not rising<br/>
        /// </summary>
        /// <returns></returns>
        private bool isAirSpin()
        {
            return !_onGround && _inputMoveY > 0 && !isRising();
        }

        /// <summary>
        /// If the player is currently duckig<br/>
        /// Uses the _ducking variable<br/>
        /// </summary>
        /// <returns></returns>
        private bool isCrouching()
        {
            return _ducking;
        }

        /// <summary>
        /// If the player should run<br/>
        /// If the player is on the ground,<br/>
        /// has a large enough value on the corresponding input axis,<br/>
        /// and is not ducking<br/>
        /// </summary>
        /// <returns></returns>
        private bool isRunning()
        {
            return Math.Abs(_moveX) > 0.5 && _onGround && !isCrouching();
        }

        /// <summary>
        /// UnsafeDispose,<br/>
        /// Replaces the player with a new player with the same ID<br/>
        /// </summary>
        public override void UnsafeDispose()
        {
            Game.I._players[_playerId] = null;
            int playerID = _playerId;
            Game.I._players[_playerId] = new Player(_texture) { Position = new Point(50, 0), _playerId = playerID };
            Game.I._players[_playerId].TryLoadContent();

            base.UnsafeDispose();
        }

        /// <summary>
        /// The X coordinate of the player (calculated from the middle)<br/>
        /// </summary>
        public int X
        {
            get
            {
                return Position.X;
            }
        }

        /// <summary>
        /// The Y coordinate of the player (calculated from the middle)<br/>
        /// </summary>
        public int Y
        {
            get
            {
                return Position.Y;
            }
        }

        /// <summary>
        /// Is called when the player collides with a solid horizontally<br/>
        /// Tries to wiggle the player  up then down to continue the movement<br/>
        /// </summary>
        /// <param name="amount"></param>
        private void OnCollideH(float amount)
        {
            if (_speed.X < 0)
            {
                //Upward corner Correction
                {
                    if (_speed.Y <= 0)
                    {
                        for (int i = 1; i <= _sidewayCornerCorrection; i++)
                        {
                            if (!_colliderSensor.CollideAt(Position + new Point(-1, -i), onlySolids)._isColliding)
                            {
                                Position += new Point(-1, -i);
                                return;
                            }
                        }
                    }

                    if (_speed.Y >= 0)
                    {
                        for (int i = 1; i <= _sidewayCornerCorrection; i++)
                        {
                            if (!_colliderSensor.CollideAt(Position + new Point(-1, i), onlySolids)._isColliding)
                            {
                                Position += new Point(-1, i);
                                return;
                            }
                        }
                    }
                }
            }

            if (_speed.X > 0)
            {
                //Upward corner Correction
                {
                    if (_speed.Y <= 0)
                    {
                        for (int i = 1; i <= _sidewayCornerCorrection; i++)
                        {
                            if (!_colliderSensor.CollideAt(Position + new Point(1, -i), onlySolids)._isColliding)
                            {
                                Position += new Point(1, -i);
                                return;
                            }
                        }
                    }

                    if (_speed.Y >= 0)
                    {
                        for (int i = 1; i <= _sidewayCornerCorrection; i++)
                        {
                            if (!_colliderSensor.CollideAt(Position + new Point(1, i), onlySolids)._isColliding)
                            {
                                Position += new Point(1, i);
                                return;
                            }
                        }
                    }
                }
            }

            _speed.X = 0;
        }

        /// <summary>
        /// Is called when the player collides with a solid vertically<br/>
        /// Tries to wiggle the player left and right to continue the movement (if moving up)<br/>
        /// </summary>
        /// <param name="amount"></param>
        private void OnCollideV(float amount)
        {
            if (_speed.Y < 0)
            {

                //Upward corner Correction
                {
                    if (_speed.X <= 0)
                    {
                        for (int i = 1; i <= _upwardCornerCorrection; i++)
                        {
                            if (!_colliderSensor.CollideAt(Position + new Point(-i, -1), onlySolids)._isColliding)
                            {
                                Position += new Point(-i, -1);
                                return;
                            }
                        }
                    }

                    if (_speed.X >= 0)
                    {
                        for (int i = 1; i <= _upwardCornerCorrection; i++)
                        {
                            if (!_colliderSensor.CollideAt(Position + new Point(i, -1), onlySolids)._isColliding)
                            {
                                Position += new Point(i, -1);
                                return;
                            }
                        }
                    }
                }

                if (_varJumpTimer < VarJumpTime - CeilingVarJumpGrace)
                    _varJumpTimer = 0;
            }
            if (_speed.Y > 0)
            {
                EmitLandingParticles();

            }
            _speed.Y = 0;
        }

        /// <summary>
        /// Emit dust particles when landing<br/>
        /// </summary>
        private void EmitLandingParticles()
        {
            //ScreenShake
            //TestGame.game.Screenshake(0.1f, 100);

            //Emits dust particles based on falling fast or normally (is the y input axis pressed dows)
            PlayerParticles.Emit(LandingDust, _inputMoveY.Value > 0 ? FastLandingDustCount : LandingDustCount, Position.ToVector2() + new Vector2(0, 14), new Vector2(8, 2));
        }

        /// <summary>
        /// Called to make player die<br/>
        /// Sets _isDying to true<br/>
        /// </summary>
        public void Die()
        {
            _isDying = true;
        }

        /// <summary>
        /// Called when player collides with a solid and can't be pushed any further<br/>
        /// To avoid a solid entering the player we first try to wiggle the player in a spiral pattern where player survives<br/>
        /// Otherwise make player Die<br/>
        /// The solids move in two steps (x and y), so squish may only trigger twice a move (thus won't move player too far)<br/>
        /// </summary>
        public override void Squish(float amount)
        {
            List<Point> spiral = Tools.spiralOrder(_squishWiggleX * 2 + 1, _squishWiggleY * 2 + 1);
            for (int i = 0; i < spiral.Count; i++)
            {
                if (!_colliderSensor.CollideAt(Position + spiral[i] - spiral[0], onlySolids)._isColliding)
                {
                    Position += spiral[i] - spiral[0];
                    return;
                }
            }
            Die();
        }
    }
}