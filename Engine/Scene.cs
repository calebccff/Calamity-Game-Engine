using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;




namespace TestShader
{
    /// <summary>
    /// The Scene class is the base class for all scenes in the game.
    /// Each scene manages a separate list of components and cameras.
    /// </summary>
    public class Scene : IComparable<Scene>
    {
        /// <summary>
        /// Count of scenes (used for unique ID creation)<br/>
        /// </summary>
        static int IdCount = 0;

        /// <summary>
        /// Unique ID of current scene<br/>
        /// </summary>
        public int ID;

        /// <summary>
        /// Is the scene disposed<br/>
        /// </summary>
        public bool _isDisposed = false;

        /// <summary>
        /// Is the scene loaded<br/>
        /// If not, LoadContent will be called at the beginning of next frame<br/>
        /// </summary>
        public bool _isLoaded = false;


        //-----------Scene scpecifics---------
        /// <summary>
        /// Maximal id of components in scene (Not yet implemented)<br/>
        ///TODO:we can have a list of IDContainers for the different scenes<br/>
        /// </summary>
        public int _idCount;

        /// <summary>
        /// List of components that need to be cleaned up at the end of the frame<br/>
        /// </summary>
        public List<Component> _cleanUp = new List<Component>();

        /// <summary>
        /// List of components that are in the scene<br/>
        /// </summary>
        public IDContainer _gameComponents = new IDContainer();

        /// <summary>
        /// List of actors that are in the scene<br/>
        /// </summary>
        public List<Actor> _actors = new List<Actor>();

        /// <summary>
        /// List of actorMovers that are in the scene<br/>
        /// </summary>
        public List<ActorMover> _actorMovers = new List<ActorMover>();

        /// <summary>
        /// Font used in the scene<br/>
        /// </summary>
        public SpriteFont font;

        /// <summary>
        /// List of cameras that are in the scene<br/>
        /// </summary>
        public List<Camera> Cameras = new List<Camera>();

        /// <summary>
        /// List of colliders that are in the scene<br/>
        /// </summary>
        public IDContainer _colliders = new IDContainer();

        /// <summary>
        /// List of colliderSensors that are in the scene<br/>
        /// </summary>
        public IDContainer _colliderSensors = new IDContainer();

        /// <summary>
        /// List of AudioListeners in the scene<br/>
        /// </summary>
        public List<AudioListener> _listeners = new List<AudioListener>();


        //-------------Player Variables-------------
        /// <summary>
        /// Number of players in the scene<br/>
        /// Only works with 1,2 at the moment<br/>
        /// TODO: let's allow 1,2,3,4 mb for now (because of screens)<br/>
        /// </summary>
        public int numPlayers = 2;

        /// <summary>
        /// List of players in the scene<br/>
        /// </summary>
        public List<Player> _players = new List<Player>();


        public Scene()
        {
            // Set unique ID
            ID = IdCount++;
        }

        private bool _isActive = false;
        /// <summary>
        /// Is the scene active<br/>
        /// There should be exactly 1 active scene at any time<br/>
        /// This should be set by Game in update<br/>
        /// Setting it to true will call OnActivate<br/>
        /// Setting it to false will call OnDeactivate<br/>
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (value != _isActive)
                {
                    // Call OnActivate/OnDeactivate if value changes
                    if (value)
                        OnActivate();
                    else
                        OnDeactivate();

                    _isActive = value;
                }
            }
        }

        /// <summary>
        /// Loads contents if not already loaded<br/>
        /// </summary>
        public void TryLoadContent()
        {
            if (!_isLoaded) LoadContent();
        }

        /// <summary>
        /// LoadContent,<br/>
        /// Tries to load the content of the components<br/>
        /// </summary>
        protected virtual void LoadContent()
        {

            _isLoaded = true;
            for (int index = 0; index < _gameComponents._list.Count; index++)
            {
                _gameComponents._list[index].TryLoadContent();
            }
        }



        public virtual void Update(GameTime gameTime)
        {
        }

        public virtual void Draw(GameTime gameTime, SpriteRenderer spriteRenderer)
        {
        }

        /// <summary>
        /// OnActivate will be called when the scene is set to active<br/>
        /// Tries to load content<br/>
        /// </summary>
        public virtual void OnActivate()
        {
            TryLoadContent();
        }

        /// <summary>
        /// OnDeactivate will be called when the scene is set to inactive<br/>
        /// </summary>
        public virtual void OnDeactivate() { }

        /// <summary>
        /// Implements IComparable<br/>
        /// Compares Scenes by their ID<br/>
        /// </summary>
        /// <param name="other"> The scene to compare to</param>
        /// <returns> Returns the result of the comparison</returns>
        public int CompareTo(Scene other)
        {
            return ID.CompareTo(other.ID);
        }
    }
}
