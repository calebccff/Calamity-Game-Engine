using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{
    /// <summary>
    /// A scene for testing and demonstrating the completed features<br/>
    /// </summary>
    class DemoScene : Scene
    {
        //-------------Demo Game speifics

        /// <summary>
        /// (WIP) An enemy being implemented<br/>
        /// </summary>
        private Guardian _guardian;

        /// <summary>
        /// Enables a parralax background<br/>
        /// </summary>
        const bool Parralax = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoScene"/> class.<br/>
        /// </summary>
        public DemoScene() : base()
        {
        }

        

        /// <summary>
        /// LoadContent,<br/>
        /// Loads the Resources, Players and the level itself<br/>
        /// </summary>
        protected override void LoadContent()
        {
            //Scene specific
            LoadResources();
            LoadPlayers();
            LoadDemoLevel();
            base.LoadContent();
        }

        /// <summary>
        /// Loads the resources for the level using MonoGame's ContentManager<br/>
        /// </summary>
        private void LoadResources()
        {
            font = Game.I.Content.Load<SpriteFont>("myFont");
            //use this.Content to load your game content here
            TextureManager.LoadTexture("MainSheet", "FarmerStorySprites");
            TextureManager.LoadTexture("FireKnight", "fire_knight");
            
            // If Parralax is enabled loads each parallax background layer texture
            if (Parralax)
            {
                TextureManager.LoadTexture("Layer1", "BackgroundLayers/Layer_0000_1");
                TextureManager.LoadTexture("Layer2", "BackgroundLayers/Layer_0000_2");
                TextureManager.LoadTexture("Layer3", "BackgroundLayers/Layer_0000_3");
            }

        }

        /// <summary>
        /// Loads the players for the demo scene<br/>
        /// </summary>
        private void LoadPlayers()
        {
            // Initializes the players list
            _players = new List<Player>();

            // Adds numPlayers many players
            for (int i = 0; i < numPlayers; i++)
            {
                //Spawn each player at a slightly different position and give a unique id to each
                _players.Add(new Player(TextureManager.GetTexture("FireKnight")) { Position = new Point(200 + 100 * i, 0), _playerId = i });
            }

        }

        

        /// <summary>
        /// A variable containing the DemoBall in the scene for further testing<br/>
        /// </summary>
        DemoBall ball;

        /// <summary>
        /// A variable containing the DemoUIWindow in the scene<br/>
        /// </summary>
        DemoUIWindow UI;

        /// <summary>
        /// A variable containing the DemoUIWindow in the scene<br/>
        /// </summary>
        public PocketBox pointer;

        /// <summary>
        /// Loads the level for the demo scene<br/>
        /// </summary>
        private void LoadDemoLevel()
        {
            // Creates each entity in the level
            new DemoSolidCharacter(TextureManager.GetTexture("MainSheet"), 32, 32) { Position = new Point(200, 100) };
            ball = new DemoBall(TextureManager.GetTexture("MainSheet"), 32, 32) { Position = new Point(300, 0) };
            new DemoSolid2(TextureManager.GetTexture("MainSheet"), 32, 32) { Position = new Point(400, 100) };
            new DemoJumpThrough(TextureManager.GetTexture("MainSheet"), 32, 32) { _position = new Point(100, 30) };
            TextureManager.LoadTexture("landingCloudPlayer", "landingCloudPlayer");
            UI =new DemoUIWindow();
            Tilemap map = new DemoTilemap(TextureManager.GetTexture("MainSheet")) { _position = new Point(50, 50) };
            //_guardian = new Guardian(TextureManager.GetTexture("MainSheet")) { _position = new Point(400, 100) };
            
            // If Parralax is enabled loads each parallax background layer
            if (Parralax)
            {
                new ParallaxBackground(TextureManager.GetTexture("Layer3"), (1f - 1f / 7.8f)){_scale = new Vector2(2f, 2f)};
                new ParallaxBackground(TextureManager.GetTexture("Layer2"), (1f - 1f / 12.4f)) { _scale = new Vector2(2f, 2f) };
                new ParallaxBackground(TextureManager.GetTexture("Layer1"), (1f - 1f / 17f)) { _scale = new Vector2(2f, 2f) };
            }

            // Creates each camera
            //The first two follows each player and has size half the screen, the third follows the avarege of the two players and has size the screen
            // Used to implement split screen
            new DemoCamera() { destinationRectangle = new Rectangle(0, 0, 450 / 2, 260), active = false, focusPlayerID = 0 };
            new DemoCamera() { destinationRectangle = new Rectangle(450 / 2, 0, 450 / 2, 260), active = false, focusPlayerID = 1 };
            new DemoCamera() { destinationRectangle = new Rectangle(0, 0, 450, 260) };
        }

        /// <summary>
        /// Update,<br/>
        /// Updates the active cameras to create dynamic split screen for players<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Test if separate or shared cameras should be used for split screen
            if (!Cameras[2].destinationRectangle.Contains((_players[0]._position - _players[1]._position).ToVector2() / 2 * 1.2f * Cameras[2]._scale + Cameras[2].destinationRectangle.Center.ToVector2()))
            {
                Cameras[0].active = true;
                Cameras[1].active = true;
                Cameras[2].active = false;
            }
            else
            {
                Cameras[0].active = false;
                Cameras[1].active = false;
                Cameras[2].active = true;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Draw,<br/>
        /// Draws the debug text in the top left<br/>
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteRenderer spriteRenderer)
        {
            spriteRenderer.DrawString(font, "Debug_Build", new Vector2(10, 10), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0f);
            base.Draw(gameTime, spriteRenderer);
        }

        /// <summary>
        /// OnActivate,<br/>
        /// Adds the UI to the game if it is already loaded<br/>
        /// </summary>
        public override void OnActivate()
        {   
            if(UI != null) UIComponent.Root.Children.Add(UI);
            base.OnActivate();
        }

        /// <summary>
        /// OnDeactivate,<br/>
        /// Removes the UI from the game<br/>
        /// </summary>
        public override void OnDeactivate()
        {
            UIComponent.Root.Children.Remove(UI);
            base.OnDeactivate();
        }
    }
}
