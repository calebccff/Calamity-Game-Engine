using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TestShader;


namespace TestShader
{
    /// <summary>
    /// More complex example for a character<br/>
    /// </summary>
    class DemoBall : Character
    {
        /// <summary>
        /// The number of frames in the sprite sheet in the x direction<br/>
        /// </summary>
        int _xCount;
        /// <summary>
        /// The number of frames in the sprite sheet in the y direction<br/>
        /// </summary>
        int _yCount;

        /// <summary>
        /// The speed multiplier when in the air<br/>
        /// </summary>
        public const float AirMult = 0.65f;

        /// <summary>
        /// The max speed in the X direction<br/>
        /// </summary>
        public const float MaxRun = 90f;

        /// <summary>
        /// The horizontal acceleration boost when running<br/>
        /// </summary>
        private const float JumpHBoost = 50f;

        /// <summary>
        /// The vertical jump speed<br/>
        /// </summary>
        private const float JumpSpeed = 200f;

        /// <summary>
        /// The maximal fall speed<br/>
        /// </summary>
        private const float MaxFall = 160f;

        /// <summary>
        /// The maximal acceleration of the speed<br/>
        /// </summary>
        private const float FastMaxAccel = 300f;

        /// <summary>
        /// The list of states the character can be in<br/>
        /// </summary>
        public override List<string> States { get; } = new List<string>() { "Default", "Non-Default" };

        /// <summary>
        /// Component that animates the character depending on the state<br/>
        /// </summary>
        public StateAnimatedCharacterComponent _animatedComponent;
        
        /// <summary>
        /// Component that moves the character as an actor<br/>
        /// </summary>
        public ActorMover _actorComponent;

        /// <summary>
        /// Component that handles friction and speed calculations<br/>
        /// </summary>
        public FrictionCharacterComponent _speedComponent;

        /// <summary>
        /// Component that handles particle effects<br/>
        /// </summary>
        public ParticleCharacterComponent _particleComponent;

        /// <summary>
        /// Component that handles stats of the class<br/>
        /// </summary>
        public BallStats _statComponent;

        /// <summary>
        /// Custom stat component for the ball<br/>
        /// </summary>
        public class BallStats : StatCharacterComponent
        {
            /// <summary>
            /// Health stat of the ball<br/>
            /// </summary>
            public Stat<int> health;

            /// <summary>
            /// The amount to regenerate the health<br/>
            /// </summary>
            public Stat<int> healthRegen;

            /// <summary>
            /// The default value of the health for resets<br/>
            /// </summary>
            public Stat<int> healthReset;

            /// <summary>
            /// Initializes the stats<br/>
            /// </summary>
            /// <param name="character"> The character to attach the stats to </param>
            public BallStats(ICharacter character) : base(character)
            {
                //Initialize the stats
                health = new Stat<int>(this, 40);
                healthRegen = new Stat<int>(this, 100);
                healthReset = new Stat<int>(this, 0);

                //Add modifiers to the stats
                //Regenerate the health by healthRegen
                health.AddModifier(new RegenerationInt(health) { RegenerationStat = nameof(healthRegen) });
                //Reset the health to healthReset when the state of the ball changes
                health.AddModifier(new ResetListener<int>(health) { ResetStat = nameof(healthReset), Listener = (character as DemoBall).Events._onStateChange });
            }
        }

        //TODO: Implement Clampig modifiers, Default flag for modifiers (when something resets all modifiers)

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoBall"/> class.<br/>
        /// </summary>
        /// <param name="texture"> The sprite sheet.</param>
        /// <param name="x_count"> The number of frames in the sprite sheet in the x direction</param>
        /// <param name="y_count"> The number of frames in the sprite sheet in the y direction</param>
        public DemoBall(Texture2D texture, int x_count, int y_count) : base()
        {
            _xCount = x_count;
            _yCount = y_count;

            //Initialize the components
            _actorComponent = new ActorMover(this);
            _animatedComponent = new StateAnimatedCharacterComponent(texture, this);
            _speedComponent = new FrictionCharacterComponent(this) { _actualXAccel = 1000f, _actualYAccel = 160f, _targetXSpeed = 80f };
            _particleComponent = new ParticleCharacterComponent(0, 500, this);
            _statComponent = new BallStats(this);

            // Add the hitbox with the same size as the sprite
            _actorComponent.Hitbox.AddHitbox(new Rectangle(new Point(-texture.Width / x_count / 2, -texture.Height / y_count / 2), new Point(texture.Width / x_count, texture.Height / y_count)));
        }

        /// <summary>
        /// LoadContent<br/>
        /// Creates the animations from the sprite sheet<br/>
        /// Sets the animation to play<br/>
        /// Creates the <see cref="ParticleType"/> for dust particles<br/>
        /// Subscribes eventlisteners<br/>
        /// </summary>
        protected override void LoadContent()
        {

            base.LoadContent();//Need to load components before such actions
            _animatedComponent._anim._animations.Add("Default", new Animation(Tools.SplitTileSheet(_animatedComponent._anim._texture.Width / _xCount, _animatedComponent._anim._texture.Height / _yCount, _xCount, _yCount).GetRange(12, 3)) { _animationStepSpeed = 4 });
            _animatedComponent._anim.ChangeAnimationState("Default");
            _animatedComponent._anim._scale = new Vector2(1, 1);
            SetUpLandingDust();
            // On landing the ball will emit particles
            _speedComponent.OnGroundReset += EmitJumpParticles;
            // On landing the ball will jump (thus bouncing)
            _speedComponent.OnGroundReset += Jump;
        }

        /// <summary>
        /// A function to make the ball jump, called by OnGroundReset<br/>
        /// Changes the state of the ball back and forth<br/>
        /// </summary>
        public void Jump()
        {
            if(State== "Default") State = "Non-Default";
            else State = "Default";
            _speedComponent._speed.X += JumpHBoost * 1;
            _speedComponent._speed.Y = -JumpSpeed;
        }

        /// <summary>
        /// Set up the <see cref="ParticleType"/> for dust particles<br/>
        /// </summary>
        private void SetUpLandingDust()
        {
            _particleComponent.ParticleType.Add("landingDust", new ParticleType());
            _particleComponent.ParticleType["landingDust"]._texture = TextureManager.GetTexture("landingCloudPlayer");
            _particleComponent.ParticleType["landingDust"]._anim = new Animation(Tools.SplitTileSheet(_particleComponent.ParticleType["landingDust"]._texture.Width / 3, _particleComponent.ParticleType["landingDust"]._texture.Height / 3, 3, 3).GetRange(0, 7));
            _particleComponent.ParticleType["landingDust"]._anim._animationStepSpeed = 8;
            _particleComponent.ParticleType["landingDust"].LifeMin = 0.2f;
            _particleComponent.ParticleType["landingDust"].LifeMax = 0.6f;
            _particleComponent.ParticleType["landingDust"]._scale = 0.2f;
            _particleComponent.ParticleType["landingDust"].SpinMin = -5f;
            _particleComponent.ParticleType["landingDust"].SpinMax = 5f;
            _particleComponent.ParticleType["landingDust"].RotationMode = ParticleType.RotationModes.Random;
            _particleComponent.ParticleType["landingDust"].Direction = (float)Math.PI / 2;
            _particleComponent.ParticleType["landingDust"].DirectionRange = 1;
            _particleComponent.ParticleType["landingDust"].SpeedMin = 0;
            _particleComponent.ParticleType["landingDust"].SpeedMax = 30;
            _particleComponent.ParticleType["landingDust"].SizeRange = 0.1f;
            _particleComponent.ParticleType["landingDust"].FadeMode = ParticleType.FadeModes.Linear;
        }

        /// <summary>
        /// Emit particles when the ball hits the ground<br/>
        /// </summary>
        private void EmitJumpParticles()
        {
            _particleComponent.Emit(_particleComponent.ParticleType["landingDust"], 20, _position.ToVector2() + new Vector2(0, 14), new Vector2(8, 2));
        }

        /// <summary>
        /// Update,<br/>
        /// Clamp the value of the health between 0 and 255<br/>
        /// Vary the color of the sprite based on the health<br/>
        /// Bound the ball between x=200 and x=600<br/>
        /// Calculate the new y target speed<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            //TODO: Implement Clamp modifier
            //TODO: rework delegates as event listeners
            _statComponent.health.Value = Math.Clamp(_statComponent.health.Value, 0, 255);
            _animatedComponent._color.R = (byte)_statComponent.health;
            if (Position.X > 600 || Position.X < 200) { _speedComponent._speed.X *= -1; _speedComponent._targetXSpeed *= -1; }
            _speedComponent._targetYSpeed = Calc.Approach(_speedComponent._targetYSpeed, MaxFall, FastMaxAccel * Game.I._deltaTime);
            base.Update(gameTime);
        }


    }


}
