using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// A CharacterComponent that is also a Character<br/>
    /// </summary>
    public class SubCharacter : CharacterComponent, ICharacter
    {
        //TODO: Implement some features already in Character

        /// <summary>
        /// The position of the character (Private)<br/>
        /// </summary>
        private new Point _position = new Point();
        /// <summary>
        /// The position of the character<br/>
        /// On set, invokes _onPositionChange<br/>
        /// </summary>
        public Point Position
        {
            get { return _position; }
            set
            {
                Events._onPositionChange.Invoke(_position, value); _position = value;
            }
        }

        /// <summary>
        /// The events corresponding to the character<br/>
        /// /// Check <seealso cref="EventCollection"/> for more details<br/>
        /// </summary>
        public CharacterEvents Events { get; set; } = new CharacterEvents();

        /// <summary>
        /// The collection of states that the character can be in<br/>
        /// </summary>
        public virtual List<string> States { get; } = new List<string>() { "Default" };

        /// <summary>
        /// The current state of the character (Private)<br/>
        /// </summary>
        private string _state = "Default";
        /// <summary>
        /// The current state of the character<br/>
        /// Can only be set to a member of States<br/>
        /// Will invoke the onStateChange event when it is changed<br/>
        /// </summary>
        public string State
        {
            get { return _state; }
            set
            {
                if (States.Contains(value))
                {
                    Events._onStateChange.Invoke(_state, value); _state = value;
                }
                else throw new ArgumentException(value + " is not a member of the States of Subcharacter");
            }
        }


        /// <summary>
        /// The collection of components for the character<br/>
        /// </summary>
        public List<CharacterComponent> Components { get; set; } = new List<CharacterComponent>();

        /// <summary>
        /// The Mover component for the character (There can only be one active mover)<br/>
        /// The only CharacterComponent that can have move the character<br/>
        /// </summary>
        public Mover Mover { get; set; }

        /// <summary>
        /// The collection of components that need to be cleaned up after next update<br/>
        /// </summary>
        public List<CharacterComponent> _cleanUp { get; set; } = new List<CharacterComponent>();

        /// <summary>
        /// Moves the Character<br/>
        /// Uses the active Mover<br/>
        /// </summary>
        /// <param name="difference"> The difference to move</param>
        public virtual void Move(Point difference)
        {
            Mover.Move(difference);
        }

        /// <summary>
        /// Moves the Character<br/>
        /// Uses the active Mover<br/>
        /// </summary>
        /// <param name="difference"> The difference to move</param>
        public virtual void Move(Vector2 difference)
        {
            Mover.Move(difference);
        }

        public SubCharacter(ICharacter character) : base(character)
        {

        }

        /// <summary>
        /// LoadContent,<br/>
        /// Loads the content of the CharacterComponents<br/>
        /// </summary>
        public override void LoadContent()
        {
            foreach (var component in Components)
            {
                component.LoadContent();
            }
            base.LoadContent();
        }

        /// <summary>
        /// Update,<br/>
        /// Triggers the update of the CharacterComponents<br/>
        /// Resets the _updated flag to False for each<br/>
        /// </summary>
        /// <param name="gameTime"> The game time</param>
        public override void Update(GameTime gameTime)
        {

            foreach (var component in Components)
            {
                if (!component._updated) component.Update(gameTime);
            }
            foreach (var component in Components)
            {
                component._updated = false;
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// Draw,<br/>
        /// Triggers the draw of the CharacterComponents<br/>
        /// </summary>
        /// <param name="gameTime"> The game time</param>
        public override void Draw(GameTime gameTime)
        {
            foreach (var component in Components)
            {
                component.Draw(gameTime);
            }
            base.Draw(gameTime);
        }

        /// <summary>
        /// Dispose,<br/>
        /// Disposes the CharacterComponents<br/>
        /// </summary>
        public override void Dispose()
        {
            for (int i = Components.Count - 1; i >= 0; i--)
            {
                Components[i].Dispose();
            }
            base.Dispose();
        }

        /// <summary>
        /// UnsafeDispose,<br/>
        /// Triggers the UnsafeDispose of the CharacterComponents in _cleanUp<br/>
        /// Clears _cleanUp<br/>
        /// </summary>
        public override void UnsafeDispose()
        {
            for (int x = 0; x < _cleanUp.Count; x++)
            {
                _cleanUp[x].UnsafeDispose();
            }
            _cleanUp.Clear();

            base.UnsafeDispose();
        }

        /// <summary>
        /// Checks if the character has a component with the given tag (or a subclass of it)<br/>
        /// </summary>
        /// <param name="tag"> The tag</param>
        /// <returns> True if the character has the component</returns>
        public bool hasCharacterComponent(CharacterComponent tag)
        {
            return Components.Any((attribute) => (attribute.GetType().IsSubclassOf(tag.GetType())));
        }

        /// <summary>
        /// Returns all components with the given tag (or a subclass of it)<br/>
        /// </summary>
        /// <param name="tag"> The tag</param>
        /// <returns> The list of components</returns>
        public List<CharacterComponent> allCharacterComponents(CharacterComponent tag)
        {
            return Components.Where((attribute) => (attribute.GetType().IsSubclassOf(tag.GetType()))).ToList();
        }

    }
}
