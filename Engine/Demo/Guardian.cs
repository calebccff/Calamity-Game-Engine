using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// An abandoned experiment to streamline enemy creation<br/>
    /// Will be replaced by Characters<br/>
    /// </summary>
    class Guardian : Component
    {
        /// <summary>
        /// The animated component corresponding to the guardian (Private)<br/>
        /// </summary>
        public AnimatedGameComponent _privateAnimatedComponent = null;

        /// <summary>
        /// The animated component corresponding to the guardian<br/>
        /// Can only be set once<br/>
        /// </summary>
        public AnimatedGameComponent _animatedComponent
        {
            get => _privateAnimatedComponent;
            set
            {
                if (_privateAnimatedComponent == null)
                {
                    _privateAnimatedComponent = value; return;
                }
                if (value == _privateAnimatedComponent) return; throw new Exception("Trying to reset _animatedComponent");
            }
        }

        /// <summary>
        /// Which direction the guardian is facing<br/>
        /// </summary>
        private int _facing;

        /// <summary>
        /// The standing hitbox of the guardian<br/>
        /// </summary>
        private readonly Rectangle StandRect = new Rectangle(new Point(0, 0), new Point(120, 20));// new Rectangle(new Point(-14, -16), new Point(28, 38));

        /// <summary>
        /// The hitbox of the ability of the guardian in the default facing<br/>
        /// </summary>
        private readonly Rectangle HitRect = new Rectangle(new Point(28, 24), new Point(92, 24));

        /// <summary>
        /// The hitbox of the ability of the guardian in the opposite facing<br/>
        /// </summary>
        private readonly Rectangle InverseHitRect = new Rectangle(new Point(-14, -16), new Point(28, 38));
        
        /// <summary>
        /// The speed of the guardian<br/>
        /// </summary>
        public Vector2 _speed;

        /// <summary>
        /// The intention of the guardian to move in the X axis<br/>
        /// This will be controlled by the enemy AI<br/>
        /// </summary>
        private float _moveX;

        /// <summary>
        /// The maximum fall speed of the guardian<br/>
        /// </summary>
        private float _maxFall;
        
        public bool _onGround = false;

        /// <summary>
        /// The modifier fot the speed of acceleration and deceleration in the air<br/>
        /// </summary>
        public const float AirMult = 0.65f;

        /// <summary>
        /// The maximum horizontal speed of the guardian<br/>
        /// </summary>
        public const float MaxRun = 90f;

        /// <summary>
        /// The decceleration of the guardian when stopping while running<br/>
        /// </summary>
        private const float RunReduce = 400f;

        /// <summary>
        /// The acceleration of the guardian when running<br/>
        /// </summary>
        private const float RunAccel = 1000f;

        /// <summary>
        /// The horizontal boost of the guardian when jumping<br/>
        /// </summary>
        private const float JumpHBoost = 50f;

        /// <summary>
        /// The vertical speed of the guardian when jumping<br/>
        /// </summary>
        private const float JumpSpeed = 200f;

        /// <summary>
        /// How strong does Gravity effects the guardian<br/>
        /// </summary>
        private const float Gravity = 1000f;

        /// <summary>
        /// What is the treshold for the guardian to have only half the gravity applying to it (makes jumps more responsive)<br/>
        /// </summary>
        private const float HalfGravThreshold = 40f;

        /// <summary>
        /// Maximum passive fall speed of the guardian<br/>
        /// </summary>
        private const float MaxFall = 160f;

        /// <summary>
        /// Maximum active fall speed of the guardian (Equivalent to pressing the down button while falling with the player)<br/>
        /// </summary>
        private const float FastMaxFall = 240f;

        /// <summary>
        /// The acceleration of the guardian when falling<br/>
        /// </summary>
        private const float FastMaxAccel = 300f;

        /// <summary>
        /// The friction of the guardian when ducking<br/>
        /// </summary>
        private const float DuckFriction = 500f;

        /// <summary>
        /// The width of the guardian sprite<br/>
        /// </summary>
        private int _width;

        /// <summary>
        /// The height of the guardian sprite<br/>
        /// </summary>
        private int _height;

        /// <summary>
        /// The possible states of the guardian<br/>
        /// </summary>
        enum States
        {
            Default,
            Shoot,
            TeleportOut,
            TeleportIn,
            Die
        };

        /// <summary>
        /// The current state of the guardian<br/>
        /// </summary>
        States state = States.Default;

        /// <summary>
        /// The sprite map corresponding to the current guardian<br/>
        /// </summary>
        private Texture2D _texture;

        /// <summary>
        /// The collider sensor of the guardian<br/>
        /// </summary>
        private ColliderSensor _colliderSensor;
        
        private ColliderSensor.ShouldCollideCheck _sCC;

        /// <summary>
        /// Creates a new instance of the Guardian<br/>
        /// </summary>
        /// <param name="texture"> The sprite sheet</param>
        public Guardian(Texture2D texture) : base()
        {

            _texture = texture;
            _colliderSensor = new ColliderSensor(this);
            //_sCC = new ColliderSensor.ShouldCollideCheck();
            _colliderSensor._sCC = _sCC;
            _colliderSensor.AddHitbox(StandRect);
            _facing = -1;

        }
        





        /// <summary>
        /// LoadContent,<br/>
        /// Initializes the animatedComponent if it is not already initialized<br/>
        /// Sets collider position, width and height<br/>
        /// Sets scale to 2<br/>
        /// Sets up animations<br/>
        /// </summary>
        protected override void LoadContent()
        {
            _colliderSensor._position = _position;
            if (_animatedComponent == null) { _animatedComponent = new AnimatedGameComponent(_texture); Debug.WriteLine("error"); }
            _width = _animatedComponent._width;
            _height = _animatedComponent._height;

            _animatedComponent.TryLoadContent();
            _animatedComponent._scale = new Vector2(2, 2);

            SetUpAnimations();


            base.LoadContent();
        }

        /// <summary>
        /// Update function called when the guardian's State is "Die" (Not implemented)<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void DieUpdate(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update function called when the guardian's State is "TeleportIn" (Not implemented)<br/>
        /// </summary>
        private void TeleportInUpdate(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update function called when the guardian's State is "TeleportOut" (Not implemented)<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void TeleportOutUpdate(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Timer tracking how long to remain in the shooting state<br/>
        /// </summary>
        private float shootTimer;

        /// <summary>
        /// Function to set the state to "Shoot"<br/>
        /// Initializes the timer and changes the animation<br/>
        /// </summary>
        private void SetShooting()
        {
            state = States.Shoot;
            shootTimer = _animatedComponent._animations["Shoot"].Length() * _animatedComponent._animations["Shoot"]._animationStepSpeed * Game.I._deltaTime;
            _animatedComponent.ChangeAnimationState("Shoot");
        }

        /// <summary>
        /// Update function called when the guardian's State is "Shoot"<br/>
        /// Enters the state after shooting<br/>
        /// After shootTimer runs out the guardian will return to the default state<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        private void ShootUpdate(GameTime gameTime)
        {
            //Debug.WriteLine(shootTimer);
            shootTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (shootTimer <= 0)
            {
                SetDefault();
            }
        }

        /// <summary>
        /// Timer tracking how long to remain in the default state<br/>
        /// </summary>
        private float defaultTimer;

        /// <summary>
        /// Function to set the state to "Default"<br/>
        /// Initializes the timer and changes the animation<br/>
        /// </summary>
        private void SetDefault()
        {
            state = States.Default;
            defaultTimer = _animatedComponent._animations["Default"].Length() * _animatedComponent._animations["Default"]._animationStepSpeed * Game.I._deltaTime; ;
            _animatedComponent.ChangeAnimationState("Default");
        }
        /// <summary>
        /// Update function called when the guardian's State is "Default"<br/>
        /// After defaultTimer runs out the guardian will shoot if something enters its ability hitboxes<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        private void DefaultUpdate(GameTime gameTime)
        {
            defaultTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            //Debug.WriteLine(defaultTimer);
            if (defaultTimer <= 0)
            {

                foreach (var player in Game.I._players)
                    if ((_facing == 1 && player._colliderSensor.Colliding(HitRect)) ||
                        (_facing == -1 && player._colliderSensor.Colliding(InverseHitRect)))
                    {
                        SetShooting();
                        break;
                    };
                if (state != States.Shoot)
                {
                    SetShooting();
                }


            }
        }

        /// <summary>
        /// Function to set the state to "TeleportOut" (Not implemented)<br/>
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void SetTeleportOut()
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Update,<br/>
        /// Updates the position of the animated component, the width and height, and the position of the collider sensor<br/>
        /// Triggers the Update corresponding to the current state<br/>
        /// Applies the correct flipping of the sprite<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            //Debug.WriteLine(state);

            _animatedComponent._position = _position;
            _width = _animatedComponent._width;
            _height = _animatedComponent._height;
            _colliderSensor._position = _position;


            if (state == States.Default) DefaultUpdate(gameTime);
            else if (state == States.Shoot) ShootUpdate(gameTime);
            else if (state == States.TeleportOut) TeleportOutUpdate(gameTime);
            else if (state == States.TeleportIn) TeleportInUpdate(gameTime);
            else if (state == States.Die) DieUpdate(gameTime);

            //TODO: Implement flipping so that it flips from the origin
            if (_facing == 1)
            {

                _animatedComponent._spriteEffect = SpriteEffects.None;
            }
            else if (_facing == -1)
            {

                _animatedComponent._spriteEffect = SpriteEffects.FlipHorizontally;
            }

            //Debug.WriteLine(_animatedComponent._origin);

            base.Update(gameTime);
        }




        /// <summary>
        /// Sets up the animations for the guardian<br/>
        /// Sets the animation to Default<br/>
        /// </summary>
        private void SetUpAnimations()
        {
            //_animatedComponent._shader = TestGame.game.Content.Load<Effect>("Glow");
            List<Rectangle> frameRect = Tools.SplitTileSheet(_texture.Width / 1, 48, 1, 37);
            _animatedComponent._animations.Add("Default", new Animation(frameRect.GetRange(0, 11)) { _animationStepSpeed = 8, _origin = new Vector2() });
            _animatedComponent._animations.Add("Shoot", new Animation(frameRect.GetRange(11, 11)) { _animationStepSpeed = 8, _origin = new Vector2() });
            _animatedComponent._animations.Add("TeleportOut", new Animation(frameRect.GetRange(22, 5)) { _animationStepSpeed = 4, _origin = new Vector2() });
            List<Rectangle> teleport = frameRect.GetRange(22, 5);
            teleport.Reverse();
            _animatedComponent._animations.Add("TeleportIn", new Animation(teleport) { _animationStepSpeed = 8, _origin = new Vector2() });
            _animatedComponent._animations.Add("Die", new Animation(frameRect.GetRange(27, 10)) { _animationStepSpeed = 8, _origin = new Vector2() });

            //_animatedComponent._animations.Add("DemoSwordCombo", new Animation(frameRect.GetRange(42, 6+6+5)) { _animationStepSpeed = 8 });


            _animatedComponent.ChangeAnimationState("Default");
        }


    }
}