using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;


namespace TestShader
{



    /// <summary>
    /// Class that represents a single tile type in a tilemap<br/>
    /// </summary>
    public class Tile
    {
        //TODO: Check if implementing AnimatedTile is reasonable

        /// <summary>
        /// The list of rectangles of the textures of the tile in the sprite sheet of the TileMap<br/>
        /// </summary>
        public List<Rectangle> _sourceRectangles;
        /// <summary>
        /// The speed of the animation of the tile texture<br/>
        /// </summary>
        public int _animationStepSpeed = 1;

        /// <summary>
        /// The scaling of the tile texture<br/>
        /// </summary>
        public Vector2 _scale = new Vector2(1, 1);

        /// <summary>
        /// The shader to apply when drawing the tile<br/>
        /// </summary>
        public Effect _shader;
    }

    /// <summary>
    /// Class that represents a tilemap<br/>
    /// </summary>
    public class Tilemap : DrawableComponent
    {
        /// <summary>
        /// The dictionary of type of tiles in the tilemap with ID<br/>
        /// (will be important when loading from XML)<br/>
        /// </summary>
        public SortedDictionary<int, Tile> _tileSet =
             new SortedDictionary<int, Tile>();

        /// <summary>
        /// The height of the cells in the tilemap in world coordinates<br/>
        /// </summary>
        public int _cellHeight = 32;
        /// <summary>
        /// The width of the cells in the tilemap in world coordinates<br/>
        /// </summary>
        public int _cellWidth = 32;
        /// <summary>
        /// The number of cells in the tilemap in the x-axis<br/>
        /// </summary>
        public int _cellCountX = 1;
        /// <summary>
        /// The number of cells in the tilemap in the y-axis<br/>
        /// </summary>
        public int _cellCountY = 1;

        /// <summary>
        /// The double indexed List containing the ID for each cell<br/>
        /// </summary>
        public List<List<int>> _map;

        /// <summary>
        /// The sprite sheet of the tilemap<br/>
        /// </summary>
        public Texture2D _texture;

        /// <summary>
        /// The animation speed for all animations in the tilemap<br/>
        /// </summary>
        public int _animationTime = 0;

        /// <summary>
        /// The depth of the tilemap to render<br/>
        /// </summary>
        public float __layerDepthLayer = 0.5f;

        /// <summary>
        /// The scaling of the tilemap<br/>
        /// </summary>
        public Vector2 _scale = new Vector2(1, 1);

        //TODO: Make the sprite rendered globally accesible (likely won't use multiple renderers at the same time)

        /// <summary>
        /// The sprite renderer to draw the tilemap to<br/>
        /// </summary>
        public SpriteRenderer _spriteRenderer;

        /// <summary>
        /// Creates a new tilemap<br/>
        /// Initializes the map<br/>
        /// </summary>
        /// <param name="texture"> The sprite sheet of the tilemap</param>
        /// <param name="cellCountX"> The number of cells in the tilemap in the x-axis</param>
        /// <param name="cellCountY"> The number of cells in the tilemap in the y-axis</param>
        public Tilemap(Texture2D texture, int cellCountX = 1, int cellCountY = 1) : base()
        {
            _cellCountX = cellCountX;
            _cellCountY = cellCountY;
            _texture = texture;
            _map = new List<List<int>>();
            for (int x = 0; x < _cellCountX; x++)
            {
                _map.Add(new List<int>());
                for (int y = 0; y < _cellCountY; y++)
                {
                    _map[x].Add(new int());
                }
            }
            _spriteRenderer = Game.I._spriteRenderer;
        }

        //TODO: Put update of frame in Update for consistency

        /// <summary>
        /// Draws the tilemap<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            //Iterate through the cells in the map
            for (int x = 0; x < _cellCountX; x++)
            {
                for (int y = 0; y < _cellCountY; y++)
                {
                    //TODO: Only draw visible tiles

                    // Find the tile
                    Tile sourceTile = _tileSet[_map[x][y]];
                    
                    //Calculate the correct frame to display
                    int currentFrame = _animationTime / sourceTile._animationStepSpeed % sourceTile._sourceRectangles.Count;
                    
                    //Calculate the position of the tile
                    Point currentPosition = _position + new Point(_cellWidth * x, 0) + new Point(0, _cellHeight * y);
                    Point scaledPosition = currentPosition;
                    
                    //Draw the tile
                    _spriteRenderer.Draw(_texture, new Vector2(scaledPosition.X, scaledPosition.Y), sourceTile._sourceRectangles[currentFrame], Color.White, 0f, new Vector2(0, 0), new Vector2(sourceTile._scale.X * _scale.X, sourceTile._scale.Y * _scale.Y), SpriteEffects.None, __layerDepthLayer, sourceTile._shader);
                }
            }
            base.Draw(gameTime);
        }


        //TODO: Autotiling (kinda Done)
    }
}
