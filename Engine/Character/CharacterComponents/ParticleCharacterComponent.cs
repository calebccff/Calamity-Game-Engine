using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TestShader;

namespace TestShader
{

    /// <summary>
    /// A character component that shares functionality with ParticleSystem<br/>
    /// Manages particles for a Character<br/>
    /// </summary>
    public class ParticleCharacterComponent : CharacterComponent
    {
        /// <summary>
        /// Dictionary of particle types from name<br/>
        /// </summary>
        public Dictionary<string, ParticleType> ParticleType = new Dictionary<string, ParticleType>();
        
        
        /// <summary>
        /// The default particle system (Private)<br/>
        /// </summary>
        public ParticleSystem _privateParticleSystem;

        /// <summary>
        /// The default particle system<br/>
        /// Can only be set once<br/>
        /// </summary>
        public ParticleSystem ParticleSystem
        {
            get
            {
                return _privateParticleSystem;
            }
            set
            {
                if (_privateParticleSystem == null)
                {
                    _privateParticleSystem = value;
                    return;
                }
                if (value == _privateParticleSystem) return; throw new Exception("Trying to reset _privateParticleSystem");
            }
        }

        /// <summary>
        /// The depth of the particle system visually<br/>
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// The maximum amount of particles for the particle system<br/>
        /// </summary>
        public int MaxCount { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleCharacterComponent"/> class<br/>
        /// Sets the depth of the particle system visually<br/>
        /// Sets the maximum amount of particles in the particle system<br/>
        /// </summary>
        /// <param name="depth"> The depth of the particle system visually</param>
        /// <param name="maxCount"> The maximum amount of particles</param>
        /// <param name="character"> The character to attach the particle system to</param>
        public ParticleCharacterComponent(int depth, int maxCount, Character character) : base(character)
        {
            Depth = depth;
            MaxCount = maxCount;
        }

        /// <summary>
        /// LoadContent,<br/>
        /// Creates the particle system if it doesn't exist yet<br/>
        /// </summary>
        public override void LoadContent()
        {
            if(ParticleSystem == null) ParticleSystem = new ParticleSystem(Depth, MaxCount);
            ParticleSystem.TryLoadContent();

            base.LoadContent();
        }




        //TODO: Do this smarter
        // (Generated mostly automatically)

        /// <summary>
        /// Emit a particle of type <paramref name="type"/> at <paramref name="position"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="position"> The position of the particle</param>
        public void Emit(ParticleType type, Vector2 position)
        {
            ParticleSystem.Emit(type, position);
        }

        /// <summary>
        /// Emit a particle of type <paramref name="type"/> at <paramref name="position"/> moving in the direction <paramref name="direction"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="position"> The position of the particle</param>
        /// <param name="direction"> The direction of the particle to move in</param>
        public void Emit(ParticleType type, Vector2 position, float direction)
        {
            ParticleSystem.Emit(type, position, direction);
        }

        /// <summary>
        /// Emit a particle of type <paramref name="type"/> at <paramref name="position"/> with color <paramref name="color"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="position"> The position of the particle</param>
        /// <param name="color"> The color of the particle</param>
        public void Emit(ParticleType type, Vector2 position, Color color)
        {
            ParticleSystem.Emit(type, position, color);
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
            ParticleSystem.Emit(type, position, color, direction);
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
            ParticleSystem.Emit(type, amount, position, positionRange, direction);
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
            ParticleSystem.Emit(type, track, amount, position, positionRange, direction);
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
            ParticleSystem.Emit(type, amount, position, positionRange);
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
            ParticleSystem.Emit(type, amount, position, positionRange, direction);
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
            ParticleSystem.Emit(type, amount, position, positionRange, color);
        }





        /// <summary>
        /// Emit a particle of type <paramref name="type"/> at <paramref name="position"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="position"> The position of the particle</param>
        public void Emit(string type, Vector2 position)
        {
            ParticleSystem.Emit(ParticleType[type], position);
        }

        /// <summary>
        /// Emit a particle of type <paramref name="type"/> at <paramref name="position"/> moving in the direction <paramref name="direction"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="position"> The position of the particle</param>
        /// <param name="direction"> The direction of the particle to move in</param>
        public void Emit(string type, Vector2 position, float direction)
        {
            ParticleSystem.Emit(ParticleType[type], position, direction);
        }

        /// <summary>
        /// Emit a particle of type <paramref name="type"/> at <paramref name="position"/> with color <paramref name="color"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="position"> The position of the particle</param>
        /// <param name="color"> The color of the particle</param>
        public void Emit(string type, Vector2 position, Color color)
        {
            ParticleSystem.Emit(ParticleType[type], position, color);
        }

        /// <summary>
        /// Emit a particle of type <paramref name="type"/> at <paramref name="position"/> with color <paramref name="color"/> and moving in the direction <paramref name="direction"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="position"> The position of the particle</param>
        /// <param name="color"> The color of the particle</param>
        /// <param name="direction"> The direction of the particle to move in</param>
        public void Emit(string type, Vector2 position, Color color, float direction)
        {
            ParticleSystem.Emit(ParticleType[type], position, color, direction);
        }



        /// <summary>
        /// Emit <paramref name="amount"/> particles of type <paramref name="type"/><br/>
        /// around <paramref name="position"/> in a radius of <paramref name="positionRange"/> (randomly)<br/>
        /// Each particle is moving in the direction <paramref name="direction"/> with color <paramref name="color"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="amount"> The amount of particles</param>
        /// <param name="position"> The center of the possible positions for the particles</param>
        /// <param name="positionRange"> The radius of the possible positions for the particles</param>
        /// <param name="color"> The color of the particles</param>
        /// <param name="direction"> The direction of the particle to move in</param>
        public void Emit(string type, int amount, Vector2 position, Vector2 positionRange, Color color, float direction)
        {
            ParticleSystem.Emit(ParticleType[type], amount, position, positionRange, direction);
        }

        /// <summary>
        /// Emit a particle of type <paramref name="type"/><br/>
        /// tracking <paramref name="track"/> (relative movement)<br/>
        /// around <paramref name="position"/> in a radius of <paramref name="positionRange"/> (randomly)<br/>
        /// Each particle is moving in the direction <paramref name="direction"/> with color <paramref name="color"/><br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="track"> The sprite to track</param>
        /// <param name="amount"> The amount of particles</param>
        /// <param name="position"> The center of the possible positions for the particles</param>
        /// <param name="positionRange"> The radius of the possible positions for the particles</param>
        /// <param name="direction"> The direction of the particle to move in</param>
        public void Emit(string type, SpriteGameComponent track, int amount, Vector2 position, Vector2 positionRange, float direction)
        {
            ParticleSystem.Emit(ParticleType[type], track, amount, position, positionRange, direction);
        }



        /// <summary>
        /// Emit <paramref name="amount"/> particles of type <paramref name="type"/><br/>
        /// around <paramref name="position"/> in a radius of <paramref name="positionRange"/> (randomly)<br/>
        /// </summary>
        /// <param name="type"> The type of the particle</param>
        /// <param name="amount"> The amount of particles</param>
        /// <param name="position"> The center of the possible positions for the particles</param>
        /// <param name="positionRange"> The radius of the possible positions for the particles</param>
        public void Emit(string type, int amount, Vector2 position, Vector2 positionRange)
        {
            ParticleSystem.Emit(ParticleType[type], amount, position, positionRange);
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
        /// <param name="direction"> The direction of the particle to move in</param>
        public void Emit(string type, int amount, Vector2 position, Vector2 positionRange, float direction)
        {
            ParticleSystem.Emit(ParticleType[type], amount, position, positionRange, direction);
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
        public void Emit(string type, int amount, Vector2 position, Vector2 positionRange, Color color)
        {
            ParticleSystem.Emit(ParticleType[type], amount, position, positionRange, color);
        }




        /// <summary>
        /// Update,<br/>
        /// Updates the particle system if it hasn't been updated yet<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (ParticleSystem != null && !_updated) ParticleSystem.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// Draw,<br/>
        /// (The ParticleSystem's Draw is handled independently by Game as a DrawableComponent)<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        /// <summary>
        /// Dispose,<br/>
        /// Disposes the particle system<br/>
        /// </summary>
        public override void Dispose()
        {
            if (!ParticleSystem._isDisposed) ParticleSystem.Dispose();

            base.Dispose();
        }
    }
}
