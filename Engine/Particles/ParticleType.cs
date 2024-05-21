using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace TestShader
{
    /// <summary>
    /// A blueprint for a particle<br/>
    /// Adapted from Monocle Engine from Maddy Thorson<br/>
    /// </summary>
    public class ParticleType
    {
        /// <summary>
        /// Enum for different ways to control the color of a particle<br/>
        /// Static: A static color<br/>
        /// Choose: Choose randomly from color and color2 when the particle is created<br/>
        /// Blink: The color is changed e10 times from color to color2 and back<br/>
        /// Fade: The color is fading from color to color2<br/>
        /// </summary>
        public enum ColorModes { Static, Choose, Blink, Fade };

        /// <summary>
        /// Enum for different ways to control the alpha of a particle<br/>
        /// None: alpha is 1<br/>
        /// Linear: alpha is fading from 1 to 0<br/>
        /// Late: alpha is fading from 1 to 0 in the last quarter of the particle's life<br/>
        /// InAndOut: alpha is fading from 0 to 1 in the first quarter of the particle's life and then from 1 to 0 in the last quarter<br/>
        /// </summary>
        public enum FadeModes { None, Linear, Late, InAndOut };

        /// <summary>
        /// Enum for different ways to control the rotation of a particle<br/>
        /// None: no rotation<br/>
        /// Random: random rotation<br/>
        /// SameAsDirection: rotation follows the particle's direction<br/>
        /// </summary>
        public enum RotationModes { None, Random, SameAsDirection };

        /// <summary>
        /// Static list of all particle types<br/>
        /// </summary>
        private static List<ParticleType> AllTypes = new List<ParticleType>();


        /// <summary>
        /// The texture of the particle<br/>
        /// </summary>
        public Texture2D _texture;
        /// <summary>
        /// The rectangle on the texture to be rendered for the particle<br/>
        /// </summary>
        public Rectangle _rect;

        /// <summary>
        /// The animation of the particle (null if not animated)<br/>
        /// </summary>
        public Animation? _anim;

        /// <summary>
        /// The primary color of the particle<br/>
        /// </summary>
        public Color _color;

        /// <summary>
        /// The secondary color of the particle (used for the options in ColorModes)<br/>
        /// </summary>
        public Color _color2;

        /// <summary>
        /// The way the color of the particle is controlled<br/>
        /// </summary>
        public ColorModes ColorMode;
        
        /// <summary>
        /// The way the alpha of the particle is controlled<br/>
        /// </summary>
        public FadeModes FadeMode;
        
        /// <summary>
        /// The minimum speed of the particle (random)<br/>
        /// </summary>
        public float SpeedMin;

        /// <summary>
        /// The maximum speed of the particle (random)<br/>
        /// </summary>
        public float SpeedMax;

        /// <summary>
        /// The speed multiplier of the particle see <see cref="Particle.Update(float)"/> for its use<br/>
        /// </summary>
        public float SpeedMultiplier;

        /// <summary>
        /// The acceleration of the particle<br/>
        /// </summary>
        public Vector2 Acceleration;

        /// <summary>
        /// The friction of the particle<br/>
        /// </summary>
        public float Friction;

        /// <summary>
        /// The primary direction of the particle<br/>
        /// </summary>
        public float Direction;

        /// <summary>
        /// The range of possible directions of the particle from the primary direction<br/>
        /// </summary>
        public float DirectionRange;

        /// <summary>
        /// The minimum possible starting life span of the particle<br/>
        /// </summary>
        public float LifeMin;
        /// <summary>
        /// The maximum possible starting life span of the particle<br/>
        /// </summary>
        public float LifeMax;

        /// <summary>
        /// The primary scale of the particle at the beginning<br/>
        /// </summary>
        public float _scale;

        /// <summary>
        /// The range of possible scales of the particle from the primary scale<br/>
        /// </summary>
        public float SizeRange;

        /// <summary>
        /// The minimum possible starting spin of the particle<br/>
        /// </summary>
        public float SpinMin;
        /// <summary>
        /// The maximum possible starting spin of the particle<br/>
        /// </summary>
        public float SpinMax;
        
        /// <summary>
        /// The strength of gravity affecting the particle<br/>
        /// </summary>
        public float Gravity;

        /// <summary>
        /// The maximum possible fall speed of the particle<br/>
        /// </summary>
        public float MaxFall;

        /// <summary>
        /// If true, the particle will spin in the opposite direction with a 50% chance<br/>
        /// </summary>
        public bool SpinFlippedChance;

        /// <summary>
        /// The way the rotation of the particle is controlled<br/>
        /// </summary>
        public RotationModes RotationMode;

        /// <summary>
        /// If true, the particle will scale down following a cubic root function<br/>
        /// </summary>
        public bool ScaleOut;


        /// <summary>
        /// Create a new particle type<br/>
        /// Initialize all values<br/>
        /// Add to AllTypes<br/>
        /// </summary>
        public ParticleType()
        {
            _color = _color2 = Color.White;
            ColorMode = ColorModes.Static;
            FadeMode = FadeModes.None;
            SpeedMin = SpeedMax = 0;
            SpeedMultiplier = 1;
            Acceleration = Vector2.Zero;
            Friction = 0f;
            Direction = DirectionRange = 0;
            LifeMin = LifeMax = 0;
            _scale = 2;
            SizeRange = 0;
            SpinMin = SpinMax = 0;
            SpinFlippedChance = false;
            RotationMode = RotationModes.None;
            Gravity = 0;
            MaxFall = 0;

            AllTypes.Add(this);
        }

        /// <summary>
        /// Create a new particle type from another by copying<br/>
        /// Adds to AllTypes<br/>
        /// </summary>
        public ParticleType(ParticleType copyFrom)
        {
            _texture = copyFrom._texture;
            _rect = copyFrom._rect;
            //SourceChooser = copyFrom.SourceChooser;
            _color = copyFrom._color;
            _color2 = copyFrom._color2;
            ColorMode = copyFrom.ColorMode;
            FadeMode = copyFrom.FadeMode;
            SpeedMin = copyFrom.SpeedMin;
            SpeedMax = copyFrom.SpeedMax;
            SpeedMultiplier = copyFrom.SpeedMultiplier;
            Acceleration = copyFrom.Acceleration;
            Friction = copyFrom.Friction;
            Direction = copyFrom.Direction;
            DirectionRange = copyFrom.DirectionRange;
            LifeMin = copyFrom.LifeMin;
            LifeMax = copyFrom.LifeMax;
            _scale = copyFrom._scale;
            SizeRange = copyFrom.SizeRange;
            RotationMode = copyFrom.RotationMode;
            SpinMin = copyFrom.SpinMin;
            SpinMax = copyFrom.SpinMax;
            SpinFlippedChance = copyFrom.SpinFlippedChance;
            ScaleOut = copyFrom.ScaleOut;
            _anim = copyFrom._anim;
            MaxFall = copyFrom.MaxFall;
            Gravity = copyFrom.Gravity;


            AllTypes.Add(this);
        }

        /// <summary>
        /// Modifies a particle to be one of type <see cref="ParticleType"/> at a given position<br/>
        /// </summary>
        /// <param name="particle">The particle to modify (in place)</param>
        /// <param name="position"></param>
        /// <returns> The resulting particle</returns>
        public Particle Create(ref Particle particle, Vector2 position)
        {
            return Create(ref particle, position, Direction);
        }

        /// <summary>
        /// Modifies a particle to be one of type <see cref="ParticleType"/> at a given position with a given color<br/>
        /// </summary>
        /// <param name="particle">The particle to modify (in place)</param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <returns> The resulting particle</returns>
        public Particle Create(ref Particle particle, Vector2 position, Color color)
        {
            return Create(ref particle, null, position, Direction, color);
        }

        /// <summary>
        /// Modifies a particle to be one of type <see cref="ParticleType"/> at a given position with a given direction<br/>
        /// </summary>
        /// <param name="particle"> The particle to modify (in place)</param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <returns> The resulting particle</returns>
        public Particle Create(ref Particle particle, Vector2 position, float direction)
        {
            return Create(ref particle, null, position, direction, _color);
        }

        /// <summary>
        /// Modifies a particle to be one of type <see cref="ParticleType"/> at a given position with a given color and direction<br/>
        /// </summary>
        /// <param name="particle"> The particle to modify (in place)</param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="direction"></param>
        /// <returns> The resulting particle</returns>
        public Particle Create(ref Particle particle, Vector2 position, Color color, float direction)
        {
            return Create(ref particle, null, position, direction, color);
        }

        /// <summary>
        /// Modifies a particle to be one of type <see cref="ParticleType"/> at a given position following a given entity with a given color and direction<br/>
        /// </summary>
        /// <param name="particle"> The particle to modify (in place)</param>
        /// <param name="entity"> The entity to follow</param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="color"></param>
        /// <returns> The resulting particle</returns>
        public Particle Create(ref Particle particle, SpriteGameComponent entity, Vector2 position, float direction, Color color)
        {
            //Set the track, type, visibility and position
            particle._track = entity;
            particle._type = this;
            particle._visible = true;
            particle._position = position;

            // Fill texture if not set before
            if (_texture != null)
                particle._source = _texture;
            // Copy the animation
            if (particle is AnimatedParticle)
                (particle as AnimatedParticle)._anim = _anim;
            else if(_rect != default(Rectangle))
                particle._rect = _rect;
            
            // Set size
            if (SizeRange != 0)
                particle.StartScale = particle._scale = _scale - SizeRange * .5f + Calc.Random.NextFloat(SizeRange);
            else
                particle.StartScale = particle._scale = _scale;

            // Set start color based on ColorMode
            if (ColorMode == ColorModes.Choose)
                particle.StartColor = particle._color = Calc.Random.Choose(color, _color2);
            else
                particle.StartColor = particle._color = color;

            // Calculate speed / direction
            var moveDirection = direction - DirectionRange / 2 + Calc.Random.NextFloat() * DirectionRange;
            particle._speed = Calc.AngleToVector(moveDirection, Calc.Random.Range(SpeedMin, SpeedMax));

            // Calculate starting life
            particle.StartLife = particle._life = Calc.Random.Range(LifeMin, LifeMax);

            // Calculate starting rotation based on RotationMode
            if (RotationMode == RotationModes.Random)
                particle._rotation = Calc.Random.NextAngle();
            else if (RotationMode == RotationModes.SameAsDirection)
                particle._rotation = moveDirection;
            else
                particle._rotation = 0;

            // Calculate spin and flip it if needed
            particle._spin = Calc.Random.Range(SpinMin, SpinMax);
            if (SpinFlippedChance)
                particle._spin *= Calc.Random.Choose(1, -1);

            // Set max fall and gravity
            particle._maxFall = MaxFall;
            particle._gravity = Gravity;

            // Return the modified particle
            return particle;
        }

    }
}
