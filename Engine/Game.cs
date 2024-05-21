using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using TestShader;

namespace TestShader
{
    /// <summary>
    /// The main class handling the game<br/>
    /// Singleton class<br/>
    /// Only to be used for overarching game logic<br/>
    /// Other game logic should be handled in scenes<br/>
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// Singleton instance<br/>
        /// </summary>
        public static Game I;

        //-------------Game Devices-------------
        /// <summary>
        /// The graphics manager for the game from the XNA framework<br/>
        /// </summary>
        public GraphicsDeviceManager _graphics;
        /// <summary>
        /// The sprite renderer class, handling the drawing of sprites<br/>
        /// </summary>
        public SpriteRenderer _spriteRenderer;
        /// <summary>
        /// Class handling the debugger command line for the game<br/>
        /// </summary>
        public Commands commands { get; private set; }
        /// <summary>
        /// Random number generator<br/>
        /// </summary>
        public Random rand = new Random();


        //-------------Game Settings-------------
        /// <summary>
        /// The delta time for the ticks of the game<br/>
        /// </summary>
        public float _deltaTime = 1.0f / 60.0f;
        /// <summary>
        /// Whether or not to draw hitboxes for debugging purposes<br/>
        /// </summary>
        public bool drawHitboxes = false;




        //-------------Scene variables-------------
        /// <summary>
        /// The currently active scene<br/>
        /// </summary>
        public Scene _activeScene { get; private set/*Remember ChangeScene*/; }
        /// <summary>
        /// The next scene to be loaded before next tick<br/>
        /// </summary>
        public Scene _nextScene;
        /// <summary>
        /// Scenes that are already loaded, to save progress<br/>
        /// </summary>
        public OrderedList<Scene> _passiveScenes = new OrderedList<Scene>();

        //-------------Game Settings-------------
        /// <summary>
        /// The height of the canvas the game renders to<br/>
        /// </summary>
        public int _canvasHeight = 250;
        /// <summary>
        /// The width of the canvas the game renders to<br/>
        /// </summary>
        public int _canvasWidth = 460;
        /// <summary>
        /// Scaling applied to the canvas at the very end of rendering<br/>
        /// Used to achieve pixel art<br/>
        /// </summary>
        /// <remarks>
        /// Use functions in Util to convert between coordinates
        /// </remarks>
        public Vector2 _canvasScale = new Vector2(4, 4);


        //TODO: we can have a list of IDContainers for the different scenes
        //-----------Scene scpecifics---------
        // Variables of the scene made accesible for easier access

        /// <summary>
        /// TODO: The ID count of the active scene (HAS YET TO BE IMPLEMENTED)<br/>
        /// Will be used for generating unique IDs separately for each scene<br/>
        /// </summary>
        public ref int _idCount { get => ref _activeScene._idCount; }

        /// <summary>
        /// The list of components that need to be cleaned up in the active scene<br/>
        /// </summary>
        public ref List<Component> _cleanUp { get => ref _activeScene._cleanUp; }

        /// <summary>
        /// The list of actors in the active scene (TO BE CHANGED WITH MOVERS)<br/>
        /// TODO: Change to track Movers instead<br/>
        /// </summary>
        public ref List<Actor> _actors { get => ref _activeScene._actors; }

        /// <summary>
        /// The list of game components in the active scene<br/>
        /// </summary>
        public ref IDContainer _gameComponents { get => ref _activeScene._gameComponents; }

        /// <summary>
        /// The list of actor movers in the active scene<br/>
        /// </summary>
        public ref List<ActorMover> _actorMovers { get => ref _activeScene._actorMovers; }

        /// <summary>
        /// The font used in the active scene<br/>
        /// </summary>
        public ref SpriteFont font { get => ref _activeScene.font; }



        /// <summary>
        /// The list of cameras in the active scene<br/>
        /// </summary>
        public ref List<Camera> Cameras { get => ref _activeScene.Cameras; }


        /// <summary>
        /// The list of colliders in the active scene<br/>
        /// </summary>
        public ref IDContainer _colliders { get => ref _activeScene._colliders; }

        /// <summary>
        /// The list of collider sensors in the active scene<br/>
        /// </summary>
        public ref IDContainer _colliderSensors { get => ref _activeScene._colliderSensors; }

        //-------------Player Variables-------------    
        /// <summary>
        /// The number of players in the active scene<br/>
        /// </summary>
        public ref int numPlayers { get => ref _activeScene.numPlayers; }

        /// <summary>
        /// The list of Player components in the active scene<br/>
        /// </summary>
        public ref List<Player> _players { get => ref _activeScene._players; }
        // let's allow 1,2,3,4 mb for now (because of screens)


        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.<br/>
        /// </summary>
        /// <param name="startingScene"> The starting scene of the game. If null, a new empty scene will be created. </param>
        public Game(Scene startingScene = null)
        {
            // Initalize the singleton
            I = this;

            // Initialize the graphics device manager
            _graphics = new GraphicsDeviceManager(this);
            //GraphicsDevice.SamplerStates[0] = Microsoft.Xna.Framework.Graphics.SamplerState.PointWrap;

            //Setroot directory for content manager (MonoGame)
            Content.RootDirectory = "Content";

            //Set mouse cursor to visible (MonoGame)
            IsMouseVisible = true;

            if (startingScene == null)
            {
                _activeScene = new Scene();// Set new empty scene
            }
            else
            {
                _activeScene = startingScene; // Set your initial active scene here, it is neccesery for _cameraScale
            }
        }

        /// <summary>
        /// Initializes the game, called before the first frame by MonoGame.<br/>
        /// Sets up the graphics manager of MonoGeme<br/>
        /// Initializes the InputManager, AudioManager and TextureManager, as well as the commands<br/>
        /// </summary>
        protected override void Initialize()
        {
            //Scenes specific
            _graphics.PreferredBackBufferWidth = _canvasWidth * (int)_canvasScale.X;  //Override this somehow
            _graphics.PreferredBackBufferHeight = _canvasHeight * (int)_canvasScale.Y;//Override this somehow
            _graphics.GraphicsDevice.SamplerStates[0] = Microsoft.Xna.Framework.Graphics.SamplerState.PointWrap;
            _graphics.ApplyChanges();

            //Game specific
            AudioManager.Initialization();
            TextureManager.Initialization();
            InputManager.Initialize();
            commands = new Commands();

            base.Initialize();
        }

        /// <summary>
        /// Changes the active scene to a new scene at the beginning of next tick (for safety)<br/>
        /// </summary>
        /// <param name="newScene"> The new scene to change to </param>
        public void ChangeScene(Scene newScene)
        {
            _nextScene = newScene;
        }



        /// <summary>
        /// Called before the first frame, but after Initialize<br/>
        /// Sets up the sprite renderer and sets the active scene<br/>
        /// LoadContent of unloaded components are called at the beginning of Update each tick<br/>
        /// </summary>
        protected override void LoadContent()
        {
            _spriteRenderer = new SpriteRenderer();

            _activeScene.IsActive = true;   // This loads the scene too (set triggers OnActivate in Scene)
        }





        //TODO: Save and reload (mb XML)





        /// <summary>
        /// Called each tick by MonoGame<br/>
        /// Handles exit logic<br/>
        /// Handles scene changes<br/>
        /// Updates InputManager<br/>
        /// Resets _updated flag to False for each component<br/>
        /// Tries to LoadContent for all components<br/>
        /// Updates active scene (handling components to update first)<br/>
        /// Updates all (Loaded) components in the active scene<br/>
        /// Updates the AudioManager and CollisionManager<br/>
        /// Handles cleanup<br/>
        /// Updates cameras<br/>
        /// Updates UI manager<br/>
        /// Updates commands<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {

            // Calculate elapsed time in tick (To handle lag)
            _deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;


            //Exit logic
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.End))
            {

                //TODO: Cycle through all scenes too!

                //Disposes all game components
                Component[] remover = new Component[_gameComponents.Count];
                _gameComponents._list.CopyTo(remover);
                foreach (var element in remover)
                {
                    if (!element._isDisposed)
                        element.Dispose();
                }
                for (int x = 0; x < _cleanUp.Count; x++)
                {
                    _cleanUp[x].UnsafeDispose();
                }
                for (int x = 0; x < _cleanUp.Count; x++)
                {
                    _gameComponents.Remove(_cleanUp[x]);
                }

                //Disposes graphics manager, InputManager 
                // TODO: Do we need to dispose other managers?
                GraphicsDevice.Dispose();
                InputManager.Shutdown();

                //Exits the game (MonoGame)
                Exit();
                return;
            }

            //Change scene to _nextScene 
            if (_nextScene != null && _activeScene != _nextScene)
            {
                // Deactivate the current active scene (triggers OnDeactivate in Scene)
                _activeScene.IsActive = false;

                // Add the current active scene to the passive scenes list
                if (!_passiveScenes.Contains(_activeScene)) _passiveScenes.Add(_activeScene);


                // Set the new scene as the active scene
                _activeScene = _nextScene;

                // Activate the new active scene (triggers OnActivate in Scene)
                _activeScene.IsActive = true;
            }


            //Update keyboard state
            InputManager.Update();


            //Reset _update flag
            foreach (var component in _gameComponents._list)
            {
                component._updated = false;
            }



            //LoadContent for components created during the game
            for (int index = 0; index < _gameComponents._list.Count; index++)
            {
                _gameComponents._list[index].TryLoadContent();
            }



            // Update the active scene (triggering updates on certain components to be updated first)
            _activeScene.Update(gameTime);



            //Update components
            for (int index = 0; index < _gameComponents._list.Count; index++)
            {
                // Update only components that have been loaded and have not been updated yet
                if (_gameComponents._list[index] != null && !_gameComponents._list[index]._updated && _gameComponents._list[index]._isLoaded)
                    _gameComponents._list[index].Update(gameTime);
            }

            //Update AudioManager and CollisionManager
            AudioManager.Update(gameTime);
            ColliderManager.UpdateCollision(gameTime);




            // Call unsafe dispose for all components that have been disposed (handles cleanup that would be not safe during update)
            for (int x = 0; x < _cleanUp.Count; x++)
            {
                _cleanUp[x].UnsafeDispose();
            }
            // Remove all components that have been disposed
            for (int x = 0; x < _cleanUp.Count; x++)
            {
                _gameComponents.Remove(_cleanUp[x]);
            }
            // Clear _cleanUp
            _cleanUp.Clear();


            //Update cameras
            foreach (Camera camera in Cameras)
            {
                camera.CameraUpdate(gameTime);
            }


            //Update UI
            UIComponent.SUpdate(gameTime);




            //Update debug Console
            if (commands.Open)
                commands.UpdateOpen();
            else if (commands.Enabled)
                commands.UpdateClosed();

            //Base class update (MonoGame)
            base.Update(gameTime);
        }



        /// <summary>
        /// Called after Update by MonoGame<br/>
        /// Clears the screen<br/>
        /// Draws the active scene<br/>
        /// Draws the components<br/>
        /// Draws the Hitboxes if drawHitboxes is true for debug<br/>
        /// Renders the image to the screen using the SpriteRenderer<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
            //Clear the screen
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Draw the active scene
            _activeScene.Draw(gameTime, _spriteRenderer);

            //Draw the components
            foreach (var element in _gameComponents._list)
            {
                // Draw only drawable components that have been loaded
                if (element is DrawableComponent && element._isLoaded)
                {
                    (element as DrawableComponent).Draw(gameTime);
                }
            }

            //Draw the hitboxes if needed
            if (drawHitboxes) ColliderManager.DrawHitboxes(gameTime);

            //Render the image to the screen
            _spriteRenderer.Render(gameTime, samplerState: SamplerState.PointClamp);

            //Base class draw (MonoGame)
            base.Draw(gameTime);
        }

    }
}



