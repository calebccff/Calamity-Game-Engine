using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;


namespace TestShader
{

    public interface ICharacter
    {

        /// <summary>
        /// Character Events<br/>
        /// </summary>
        CharacterEvents Events { get; set; }


        /// <summary>
        /// The CharacterComponents of the character<br/>
        /// </summary>
        List<CharacterComponent> Components { get; set; }

        /// <summary>
        /// The active Mover of the character<br/>
        /// </summary>
        Mover Mover { get; set; }

        /// <summary>
        /// The position of the character<br/>
        /// </summary>
        Point Position { get; set; }

        /// <summary>
        /// The state of the character<br/>
        /// </summary>
        string State { get; set; }

        /// <summary>
        /// The list of states of the character<br/>
        /// </summary>
        public List<string> States { get; }

        /// <summary>
        /// The list of components that need to be cleaned up after the next update<br/>
        /// </summary>
        public List<CharacterComponent> _cleanUp { get; set; }

        /// <summary>
        /// The scene that contains the character<br/>
        /// </summary>
        public Scene _scene { get; }


        void Dispose();
        void Draw(GameTime gameTime);
        void Move(Point difference);
        void Move(Vector2 difference);
        void Update(GameTime gameTime);
    }
}
