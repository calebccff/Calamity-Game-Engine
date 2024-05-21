using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace TestShader
{

    //TODO: Try MonoGame.forms for editors
    //TODO: Try https://ldtk.io/ for map editor

    /// <summary>
    /// The base class for all components in the game<br/>
    /// Implements functions for loading, updating, and disposing called by the scene automatically<br/>
    /// Handles tags<br/>
    /// Has unique id<br/>
    /// </summary>
    public class Component : IComparable<Component>, ITaggable, ITags
    {

        /// <summary>
        /// Calling order for eventlisteners of onUpdate<br/>
        /// </summary>
        public enum onUpdateOrder
        {
            Default
        }

        /// <summary>
        /// The unique id of the component<br/>
        /// </summary>
        public int _id;

        /// <summary>
        /// The maximum id of any component in the game<br/>
        /// </summary>
        public static int maxID = 0;

        /// <summary>
        /// Indicates if the component has been disposed<br/>
        /// If so, UnsafeDispose will be called after next update<br/>
        /// </summary>
        public bool _isDisposed = false;

        /// <summary>
        /// Indicates if the component has been loaded<br/>
        /// If not, LoadContent will be called by TryLoadContent when called<br/>
        /// </summary>
        public bool _isLoaded = false;

        /// <summary>
        /// Indicates if the component has been updated, used to call each update only once<br/>
        /// It set true by Update, set to false by the game after calling updates<br/>
        /// </summary>
        public bool _updated;

        /// <summary>
        /// The scene that the component is in<br/>
        /// </summary>
        public Scene _scene { get; set; }

        /// <summary>
        /// The eventlistener for triggering after calling Update<br/>
        /// </summary>
        public EventListener<onUpdateOrder> _onUpdate = new EventListener<onUpdateOrder>();

        /// <summary>
        /// The position of the component<br/>
        /// TODO: Think if this is neccesery<br/>
        /// </summary>
        public Point _position;

        /// <summary>
        /// The tags of the component<br/>
        /// </summary>
        public List<Tag> TagList { get; set; } = new List<Tag>();

        public Component()
        {
            // Set unique id and update maxID
            _id = maxID++;

            // Set the scene
            _scene = Game.I._activeScene;

            // Add the component to the game
            Game.I._gameComponents.Add(this);

            // Create tag list from the tags on the subclass
            TagList.AddRange(GetType().GetCustomAttributes(true).Where((attribute) => (attribute is Tag && (attribute as Tag) != null)).Select((attribute) => (attribute as Tag)).ToList());
        }

        /// <summary>
        /// LoadContent,<br/>
        /// Function called after initialization for functions like loading content<br/>
        /// Usually it is called by TryLoadContent to not load content multiple times<br/>
        /// </summary>
        protected virtual void LoadContent()
        {

        }


        /// <summary>
        /// TryLoadContent,<br/>
        /// Function calling LoadContent if it has not been called yet<br/>
        /// </summary>
        public virtual void TryLoadContent()
        {
            if (!_isLoaded) LoadContent();
            _isLoaded = true;

        }



        /// <summary>
        /// Returns the tags of the component<br/>
        /// </summary>
        /// <returns> The list of tags</returns>
        public List<Tag> GetTags()
        {
            return TagList;
        }

        /// <summary>
        /// Returns if the component has the tag (or a subclass of it)<br/>
        /// </summary>
        /// <typeparam name="T"> The tag</typeparam>
        /// <returns> True if the component has the tag</returns>
        public bool Tag<T>() where T : Tag
        {
            return GetTags().Any((attribute) => (attribute.GetType().IsSubclassOf(typeof(T))));
        }

        /// <summary>
        /// Returns all tags of the component matching a given tag (or a subclass of it)<br/>
        /// </summary>
        /// <typeparam name="T"> The tag</typeparam>
        /// <returns> The list of tags</returns>
        public List<Tag> Tags<T>() where T : Tag
        {
            return GetTags().Where((attribute) => (attribute.GetType().IsSubclassOf(typeof(T)))).ToList();
        }

        /// <summary>
        /// Update,<br/>
        /// Function called once each tick, updating game logic<br/>
        /// Not to be used for drawing<br/>
        /// Invokes the eventlistener _onUpdate<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            _updated = true;

            _onUpdate.Invoke(this, gameTime);
        }

        /// <summary>
        /// Dispose,<br/>
        /// Called for disposing the component, called before and of tick<br/>
        /// UnsafeDispose is called at the end of tick, to destroy the component<br/>
        /// </summary>
        public virtual void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                Game.I._cleanUp.Add(this);
            }
        }

        /// <summary>
        /// UnsafeDispose,<br/>
        /// Handles the complete deletion of the component<br/>
        /// </summary>
        public virtual void UnsafeDispose()
        {

        }

        /// <summary>
        /// Checks if the component is the same to another<br/>
        /// </summary>
        /// <param name="comp2"> The component to compare</param>
        /// <returns> True if the components are the same</returns>
        public bool isSame(Component comp2)
        {
            return _id == comp2._id;
        }

        /// <summary>
        /// Compares if the component is the same to another<br/>
        /// </summary>
        /// <param name="other"> The component to compare</param>
        /// <returns> True if the components are the same</returns>
        public int CompareTo([AllowNull] Component other)
        {
            return _id.CompareTo(other._id);
        }


        /// <summary>
        /// Adds an action to be called on update, even if the parameters are not matching<br/>
        /// </summary>
        /// <param name="action"> The action to add</param>
        public void AddActionOnUpdate(Action action) => _onUpdate.Add(new Action<List<Object>>(a => action()));

        /// <summary>
        /// Adds an action to be called on update, even if the parameters are not matching<br/>
        /// Very Unsafe<br/>
        /// </summary>
        /// <param name="action"> The action to add</param>
        public void AddActionOnUpdate(Action<Component> action) => _onUpdate.Add(new Action<List<Object>>(a => action((Component)a[0])));

        /// <summary>
        /// Adds an action to be called on update, even if the parameters are not matching<br/>
        /// Very Unsafe<br/>
        /// </summary>
        /// <param name="action"> The action to add</param>
        public void AddActionOnUpdate(Action<GameTime> action) => _onUpdate.Add(new Action<List<Object>>(a => action((GameTime)a[1])));

        /// <summary>
        /// Adds an action to be called on update, even if the parameters are not matching<br/>
        /// Very Unsafe<br/>
        /// </summary>
        /// <param name="action"> The action to add</param>
        public void AddActionOnUpdate(Action<Component, GameTime> action) => _onUpdate.Add(new Action<List<Object>>(a => action((Component)a[0],(GameTime)a[1])));




    }
}
