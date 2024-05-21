using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace TestShader
{
    /// <summary>
    /// A class handling a system of particles<br/>
    /// Handles updates of the particles<br/>
    /// Adapted from Monocle Engine from Maddy Thorson<br/>
    /// </summary>
    public class ParticleSystem : DrawableComponent
    {
        /// <summary>
        /// The array of particles<br/>
        /// </summary>
        private Particle[] _particles;

        /// <summary>
        /// The next slot to use in the array<br/>
        /// </summary>
        private int nextSlot;

        /// <summary>
        /// The depth to draw the particles at<br/>
        /// </summary>
        public int _depth;

        /// <summary>
        /// Create a new particle system<br/>
        /// </summary>
        /// <param name="depth">The depth to draw the particles</param>
        /// <param name="maxParticles">The maximum amount of particles that can be present at once</param>
        public ParticleSystem(int depth, int maxParticles)
            : base()
        {
            //Initialize the array of particles
            _particles = new Particle[maxParticles];
            
            // Create the particles for each index
            for (int i = 0; i < maxParticles; i++)
            {
                _particles[i] = new AnimatedParticle();
            }
            _depth = depth;
        }

        /// <summary>
        /// Makes all the particles invisible<br/>
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _particles.Length; i++)
                _particles[i]._visible = false;
        }

        /// <summary>
        /// Makes all the particles invisible inside or outside the given rectangle<br/>
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="inside"> If true all particles will be invisible inside the rectangle, if false all particles will be invisible outside the rectangle</param>
        public void ClearRect(Rectangle rect, bool inside)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                var pos = _particles[i]._position;
                var isInside = (pos.X > rect.Left && pos.Y > rect.Top && pos.X < rect.Right && pos.Y < rect.Bottom);

                if (isInside == inside)
                    _particles[i]._visible = false;
            }
        }

        /// <summary>
        /// Update,<br/>
        /// Updates each visible particle<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < _particles.Length; i++)
                if (_particles[i] != null && _particles[i]._visible)
                    _particles[i].Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);
        }

        /// <summary>
        /// Draw,<br/>
        /// Draws each visible particle<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            foreach (var p in _particles)
            {


                if (p != null && p._visible)
                {

                    p.Draw();
                }
            }
            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws each visible particle<br/>
        /// </summary>
        public void Draw()
        {
            foreach (var p in _particles)
                if (p._visible)
                    p.Draw();
        }

        /// <summary>
        /// Simulate the particle system for the given duration calling <paramref name="emitter"/> each every time <paramref name="interval"/> passes<br/>
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="interval"></param>
        /// <param name="emitter"></param>
        public void Simulate(float duration, float interval, Action<ParticleSystem> emitter)
        {
            var delta = (float)Game.I.TargetElapsedTime.TotalSeconds;
            for (float time = 0f; time < duration; time += delta)
            {
                if ((int)((time - delta) / interval) < (int)(time / interval))
                    emitter(this);

                for (int i = 0; i < _particles.Length; i++)
                    if (_particles[i]._visible)
                        _particles[i].Update(delta);
            }
        }

        /// <summary>
        /// Adds a particle to the particle system<br/>
        /// </summary>
        /// <param name="particle"></param>
        public void Add(Particle particle)
        {
            // Set the particle's depth
            particle._depth = _depth;

            // Add the particle to the array of particles
            _particles[nextSlot] = particle;

            // Increment the next slot for a particle
            nextSlot = (nextSlot + 1) % _particles.Length;
        }

        /// <summary>
        /// Emit a particle of type <paramref name="type"/> at <paramref name="position"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="position"> The position of the particle</param>
        public void Emit(ParticleType type, Vector2 position)
        {
            type.Create(ref _particles[nextSlot], position);
            _particles[nextSlot]._depth = _depth;

            nextSlot = (nextSlot + 1) % _particles.Length;
        }

        /// <summary>
        /// Emit a particle of type <paramref name="type"/> at <paramref name="position"/> moving in the direction <paramref name="direction"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="position"> The position of the particle</param>
        /// <param name="direction"> The direction of the particle to move in</param>
        public void Emit(ParticleType type, Vector2 position, float direction)
        {
            type.Create(ref _particles[nextSlot], position, direction);
            _particles[nextSlot]._depth = _depth;
            nextSlot = (nextSlot + 1) % _particles.Length;
        }

        /// <summary>
        /// Emit a particle of type <paramref name="type"/> at <paramref name="position"/> with color <paramref name="color"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="position"> The position of the particle</param>
        /// <param name="color"> The color of the particle</param>
        public void Emit(ParticleType type, Vector2 position, Color color)
        {
            type.Create(ref _particles[nextSlot], position, color);
            _particles[nextSlot]._depth = _depth;

            nextSlot = (nextSlot + 1) % _particles.Length;
        }

        /// <summary>
        /// Emit a particle of type <paramref name="type"/> at <paramref name="position"/> with color <paramref name="color"/> moving in the direction <paramref name="direction"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="position"> The position of the particle</param>
        /// <param name="color"> The color of the particle</param>
        /// <param name="direction"> The direction for the particles to move in</param>
        public void Emit(ParticleType type, Vector2 position, Color color, float direction)
        {
            type.Create(ref _particles[nextSlot], position, color, direction);
            _particles[nextSlot]._depth = _depth;
            nextSlot = (nextSlot + 1) % _particles.Length;
        }

        /// <summary>
        /// Emit <paramref name="amount"/> particles of type <paramref name="type"/><br/>
        /// around <paramref name="position"/> in a radius of <paramref name="positionRange"/> (randomly)<br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="amount"> The amount of particles</param>
        /// <param name="position"> The center of the possible positions for the particles</param>
        /// <param name="positionRange"> The radius of the possible positions for the particles</param>
        public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange)
        {
            for (int i = 0; i < amount; i++)
                Emit(type, Calc.Random.Range(position - positionRange, position + positionRange));
        }

        /// <summary>
        /// Emit <paramref name="amount"/> particles of type <paramref name="type"/><br/>
        /// around <paramref name="position"/> in a radius of <paramref name="positionRange"/> (randomly)<br/>
        /// Each particle is moving in the direction <paramref name="direction"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="amount"> The amount of particles</param>
        /// <param name="position"> The center of the possible positions for the particles</param>
        /// <param name="positionRange"> The radius of the possible positions for the particles</param>
        /// <param name="direction"> The direction for the particles to move in</param>
        public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange, float direction)
        {
            for (int i = 0; i < amount; i++)
                Emit(type, Calc.Random.Range(position - positionRange, position + positionRange), direction);
        }

        /// <summary>
        /// Emit <paramref name="amount"/> particles of type <paramref name="type"/><br/>
        /// around <paramref name="position"/> in a radius of <paramref name="positionRange"/> (randomly)<br/>
        /// with color <paramref name="color"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="amount"> The amount of particles</param>
        /// <param name="position"> The center of the possible positions for the particles</param>
        /// <param name="positionRange"> The radius of the possible positions for the particles</param>
        /// <param name="color"> The color of the particles</param>
        public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange, Color color)
        {
            for (int i = 0; i < amount; i++)
                Emit(type, Calc.Random.Range(position - positionRange, position + positionRange), color);
        }

        /// <summary>
        /// Emit <paramref name="amount"/> particles of type <paramref name="type"/><br/>
        /// around <paramref name="position"/> in a radius of <paramref name="positionRange"/> (randomly)<br/>
        /// moving in the direction <paramref name="direction"/> with color <paramref name="color"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="amount"> The amount of particles</param>
        /// <param name="position"> The center of the possible positions for the particles</param>
        /// <param name="positionRange"> The radius of the possible positions for the particles</param>
        /// <param name="color">The color of the particles</param>
        /// <param name="direction"> The direction for the particles to move in</param>
        public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange, Color color, float direction)
        {
            for (int i = 0; i < amount; i++)
                Emit(type, Calc.Random.Range(position - positionRange, position + positionRange), color, direction);
        }

        /// <summary>
        /// Emit <paramref name="amount"/> particles of type <paramref name="type"/><br/>
        /// around <paramref name="position"/> in a radius of <paramref name="positionRange"/> (randomly)<br/>
        /// Each particle is moving in the direction <paramref name="direction"/> each tracking the sprite <paramref name="track"/> (relativ movement)<br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="track"> The sprite to track</param>
        /// <param name="amount"> The amount of particles</param>
        /// <param name="position"> The center of the possible positions for the particles</param>
        /// <param name="positionRange"> The radius of the possible positions for the particles</param>
        /// <param name="direction"> The direction for the particles to move in</param>
        public void Emit(ParticleType type, SpriteGameComponent track, int amount, Vector2 position, Vector2 positionRange, float direction)
        {
            for (int i = 0; i < amount; i++)
            {
                type.Create(ref _particles[nextSlot], track, Calc.Random.Range(position - positionRange, position + positionRange), direction, type._color);
                _particles[nextSlot]._depth = _depth;
                nextSlot = (nextSlot + 1) % _particles.Length;
            }
        }
    }
}
