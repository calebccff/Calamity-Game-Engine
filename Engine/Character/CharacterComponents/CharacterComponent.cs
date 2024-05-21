using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using TestShader;

namespace TestShader
{
    //TODO: CharacterComponents and Movers to take over the role of all Solids and Actors

    /// <summary>
    /// Base class for all character components<br/>
    /// Represents a component that can be added to a character<br/>
    /// </summary>
    public class CharacterComponent : ITags, ITaggable
    {
        /// <summary>
        /// The character that the component is attached to (Might be the part of a Character, as a SubCharacter)<br/>
        /// </summary>
        public ICharacter character;

        /// <summary>
        /// The position of the component relative to the character<br/>
        /// </summary>
        private Point _privateRelPos;

        /// <summary>
        /// The position of the component relative to the character<br/>
        /// On set, calls OnPositionChange<br/>
        /// </summary>
        public Point _relPos
        {
            get
            {
                return _privateRelPos;
            }
            set
            {
                if (value != _privateRelPos)
                {
                    Point _old = _position;
                    _privateRelPos = value;
                    if(_old != _position) OnPositionChange(_old, _position);
                }
            }
        }

        /// <summary>
        /// Stores if the component has been updated already this tick<br/>
        /// </summary>
        public bool _updated = false;


        /// <summary>
        /// Determines if the component's position is absolute or is relative to the character<br/>
        /// </summary>
        private bool _privaterelativePositioning = true;

        

        //TODO: Create OnAddedToCharacter event listener
        //TODO: Think if changing OnPoisitionChange to an eventlistener is useful

        /// <summary>
        /// Determines if the component's position is absolute or is relative to the character<br/>
        /// On setting it, might call OnPositionChange (due to the change in position)<br/>
        /// </summary>
        public bool relativePositioning
        {
            get { return _privaterelativePositioning; }
            set
            {
                Point _old = _position;
                _privaterelativePositioning = value;
                if(_old != _position)
                OnPositionChange(_old, _position);
                //Old implementation for adding _onPositionChange to the Characters event listener to save time. Not sure if it's necessary.
                
                //if (_privaterelativePositioning == value) return; 
                //_privaterelativePositioning = value;
                //if (value)
                //{
                //    if (!Actions.delHasMethod(character._onPositionChange, OnCharacterPositionChange)) character._onPositionChange += OnCharacterPositionChange;
                //}
                //else//test this
                //{
                //    if (Actions.delHasMethod(character._onPositionChange, OnCharacterPositionChange)) character._onPositionChange -= OnCharacterPositionChange;
                //}
            }
        }

        /// <summary>
        /// The scene that the component is attached to<br/>
        /// </summary>
        public Scene _scene { get; private set; }


        /// <summary>
        /// The position of the component<br/>
        /// Depends on relativePositioning it may be absolute or relative to the character's position<br/>
        /// Can only be set by functions that have the MoverTag or are the functions of an object with the MoverTag<br/>
        /// This is to make Stats and Modifiers easier to track<br/>
        /// Experimantal<br/>
        /// </summary>
        public Point _position
        {
            get
            {
                if (relativePositioning) return character.Position + _relPos;
                else return _relPos;
            }
            [MoverTag]
            set
            {
                // get call stack
                StackTrace stackTrace = new StackTrace();
                // get calling method name
                MethodInfo method = (MethodInfo)stackTrace.GetFrame(1).GetMethod();
                // get declaring type
                Type type = method.DeclaringType;
                // check if it's an ITaggable and has the mover tag, otherwise throw an exception
                if (type.IsAssignableTo(typeof(ITaggable)) && !Functions.Tag<MoverTag>(type) && !Functions.Tag<MoverTag>(method)) throw new AccessViolationException(type.Name + " tried to move a Mover of type " + this.GetType().Name + " with method " + method.Name);

                // set the position
                // if the position is relative, set the character position. Only allow this if this CharacterComponent is the active Mover of the Character.
                if (relativePositioning)
                {
                    if (this is Mover && character.Mover == (this as Mover)) character.Position = value - _relPos; else throw new AccessViolationException("tried set_position with relative positioned mover class " + this.GetType().Name + "while not being actiove, on character" + character.GetType().Name);
                }
                else
                {
                    // This already triggers the OnPositionChange if necessary
                    _relPos = value-character.Position;
                }
            }

        }

        /// <summary>
        /// The tags of the component<br/>
        /// </summary>
        public List<Tag> TagList { get; set; } = new List<Tag>();


        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterComponent"/> class.<br/>
        /// </summary>
        /// <param name="character"> The character the component is attached to</param>
        public CharacterComponent(ICharacter character)
        {
            this.character = character;
            _scene = character._scene;

            character.Components.Add(this);
            character.Events._onPositionChange.Add(PrivateOnCharacterPositionChange);
            character.Events._onStateChange.Add(OnStateChange);
            //Set the tag list from the Attributes on the subclass
            TagList.AddRange(GetType().GetCustomAttributes(true).Where((attribute) => (attribute is Tag && (attribute as Tag) != null)).Select((attribute) => (attribute as Tag)).ToList());
        }

        /// <summary>
        /// LoadContent,<br/>
        /// </summary>
        public virtual void LoadContent()
        {

        }

        /// <summary>
        /// Update,<br/>
        /// Sets the _updated flag to True<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            _updated = true;
        }

        /// <summary>
        /// Draw,<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime gameTime)
        {

        }

        /// <summary>
        /// Called when the corresponding character's position changes<br/>
        /// It calls the OnCharacterPositionChange function<br/>
        /// If relativePositioning is true, trigger the OnPositionChange function as well<br/>
        /// </summary>
        /// <param name="Old">Old position of the character</param>
        /// <param name="New">New Position of the character</param>
        private void PrivateOnCharacterPositionChange(Point Old, Point New)
        {
            OnCharacterPositionChange(Old, New);
            if (relativePositioning) { OnPositionChange(Old + _relPos, New + _relPos); }
        }

        /// <summary>
        /// Called when the corresponding character's position changes<br/>
        /// </summary>
        /// <param name="Old">Old position of the character</param>
        /// <param name="New">New Position of the character</param>
        public virtual void OnCharacterPositionChange(Point Old, Point New)
        {

        }

        /// <summary>
        /// Called when the component's position changes<br/>
        /// </summary>
        /// <param name="Old">Old position of the component</param>
        /// <param name="New">New Position of the component</param>
        public virtual void OnPositionChange(Point Old, Point New)
        {

        }

        /// <summary>
        /// Called when the character's state changes<br/>
        /// </summary>
        /// <param name="Old">Name of the old state</param>
        /// <param name="New">Name of the new state</param>
        public virtual void OnStateChange(string Old, string New)
        {

        }

        /// <summary>
        /// Dispose,<br/>
        /// Adds the component to the clean up list of the character<br/>
        /// The UnsafeDispose function is called at the end of tick, to destroy the component<br/>
        /// </summary>
        public virtual void Dispose()
        {
            if (character != null)
                character._cleanUp.Add(this);
        }

        /// <summary>
        /// UnsafeDispose,<br/>
        /// Disposes of unmanaged resources<br/>
        /// Removes the functions OnCharacterPositionChange and OnStateChange from the character's events<br/>
        /// Removes the component from the character<br/>
        /// </summary>
        public virtual void UnsafeDispose()
        {
            character.Events._onPositionChange.RemoveFunctions(OnCharacterPositionChange);
            character.Events._onStateChange.RemoveFunctions(OnStateChange);
            character.Components.Remove(this);
        }

        /// <summary>
        /// Gets the tags of the component<br/>
        /// </summary>
        /// <returns></returns>
        public List<Tag> GetTags()
        {
            return TagList;
        }

        /// <summary>
        /// Checks if the component has the specified tag or any subclass of it<br/>
        /// </summary>
        /// <typeparam name="T"> The type of the tag</typeparam>
        /// <returns> True if the component has the tag</returns>
        public bool Tag<T>() where T : Tag
        {
            return GetTags().Any((attribute) => (attribute.GetType().IsSubclassOf(typeof(T))));
        }

        /// <summary>
        /// Gets the tags of the component that are of type <typeparam name="T"/> or any subclass of it<br/>
        /// </summary>
        /// <typeparam name="T"> The type of the tag</typeparam>
        /// <returns> The list of tags of the component </returns>
        public List<Tag> Tags<T>() where T : Tag
        {
            return GetTags().Where((attribute) => (attribute.GetType().IsSubclassOf(typeof(T)))).ToList();
        }


    }
}
