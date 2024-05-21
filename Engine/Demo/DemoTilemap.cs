using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TestShader
{

    /// <summary>
    /// A demo class for implementing a collider tilemap<br/>
    /// </summary>
    class DemoTilemap : ColliderTilemap
    {

        /// <summary>
        /// Initializes a new DemoTilemap<br/>
        /// </summary>
        /// <param name="texture">The sprite sheet "MossyTileSet.png" for the tilemap</param>
        public DemoTilemap(Texture2D texture) : base(texture, 100, 100)
        {
            //Set the size of each tile
            _cellWidth = 16;
            _cellHeight = 16;
            Effect shader = null;

            //Create the source rectangles for each tile
            List<Rectangle> sourceRects;
            sourceRects = Tools.SplitTileSheet(16, 16, 32, 32);

            //TODO: Move creation of empty tile to the base class
            //Create the empty tile
            {
                List<Rectangle> currentAnimation = new List<Rectangle>();
                currentAnimation.Add(new Rectangle());
                _tileSet.Add(0, new Tile() { _sourceRectangles = currentAnimation, _shader = shader });
            }

            //Set the scale
            _scale = new Vector2(1, 1);

            //Create a tile for each source rectangle
            int i = 1;
            foreach (Rectangle rect in sourceRects)
            {
                List<Rectangle> currentAnimation = new List<Rectangle>();
                currentAnimation.Add(rect);
                List<Rectangle> colrect = new List<Rectangle>();
                _tileSet.Add(i++, new ColliderTile() { _sourceRectangles = currentAnimation, _shader = shader, _rect = colrect });
            }

            //Load the colliders from the XML
            LoadXML("../../../Content/MainSheet.tsx");
        }

        /// <summary>
        /// LoadContent<br/>
        /// Creates the tilemap<br/>
        /// Add "solid" tag for actors to collide<br/>
        /// </summary>
        protected override void LoadContent()
        {
            //Old example
            //3 |0 |0 |25|0 |0 |1
            //12|3 |0 |0 |0 |0 |8
            //0 |12 |2 |2 |2 |2 |13

            //TODO: Tileset and Tilemap import

            //These values were extracted from an XML, this is post processing
            //List<List<int>> intermediate = new List<List<int>> { new List<int> { 3, 0, 0, 25, 0, 0, 1 }, new List<int> { 12, 3, 0, 0, 0, 0, 8 }, new List<int> { 0, 12, 2, 2, 2, 2, 13 } };
            //intermediate = new List<List<int>> { new List<int> { 0, 0, 22, 24, 0, 33, 24, 0, 25, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new List<int> { 4, 0, 0, 0, 0, 18, 0, 0, 0, 0, 25, 0, 25, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new List<int> { 36, 2, 3, 4, 0, 33, 32, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 15, 16, 45, 7, 31, 27, 13, 21, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 15, 16, 17, 15, 16, 38, 23, 23, 23, 32, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 45, 2, 3, 0, 0, 0, 0, 0, 1, 3, 0 }, new List<int> { 0, 0, 0, 0, 1, 2, 2, 3, 0, 0, 0, 0, 15, 6, 10, 0, 0, 0, 1, 2, 46, 45, 3 }, new List<int> { 0, 0, 0, 1, 13, 9, 9, 12, 3, 0, 0, 0, 0, 15, 17, 0, 0, 1, 13, 5, 17, 15, 17 }, new List<int> { 22, 24, 0, 8, 9, 9, 9, 9, 12, 3, 0, 0, 0, 0, 0, 0, 1, 13, 9, 10, 0, 0, 0 }, new List<int> { 0, 0, 0, 8, 9, 9, 9, 9, 9, 12, 2, 2, 2, 2, 2, 2, 13, 9, 5, 17, 0, 0, 0 }, new List<int> { 22, 24, 0, 15, 16, 16, 6, 5, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 17, 0, 0, 0, 0 }, new List<int> { 0, 0, 0, 0, 0, 0, 15, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };
            //intermediate = new List<List<int>> { new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new List<int> { 0, 0, 0, 0, 0, 0, 22, 24, 22, 24, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 1, 46, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 22, 49, 0, 0, 0, 0, 8, 21, 24, 0, 0, 22, 24, 0, 0, 0, 0, 22, 23, 23, 39, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 22, 24, 0, 0, 0, 0, 0, 0 }, new List<int> { 1, 2, 3, 0, 0, 0, 0, 0, 1, 2, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 12, 2, 3, 0, 0, 0, 1, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 31, 24, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0 }, new List<int> { 15, 6, 10, 0, 0, 0, 1, 2, 46, 16, 38, 32, 3, 0, 0, 0, 1, 3, 0, 0, 15, 16, 28, 38, 24, 0, 0, 15, 17, 0, 0, 0, 4, 0, 0, 0, 0, 4, 0, 22, 39, 17, 0, 0, 0, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 15, 45, 3, 0, 1, 13, 5, 17, 0, 1, 46, 17, 0, 0, 0, 8, 10, 0, 0, 0, 0, 18, 0, 0, 22, 24, 0, 0, 0, 0, 0, 36, 3, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 8, 12, 2, 46, 16, 17, 0, 0, 8, 12, 3, 0, 0, 0, 15, 17, 0, 1, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 29, 30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 22, 24, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 15, 16, 16, 17, 0, 0, 0, 0, 15, 16, 38, 32, 3, 0, 0, 0, 0, 8, 12, 3, 0, 0, 0, 0, 0, 22, 35, 0, 0, 0, 36, 37, 0, 0, 0, 0, 0, 0, 4, 0, 22, 24, 0, 0, 0, 0, 1, 2, 3, 0, 0, 0, 0, 0, }, new List<int> { 2, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 30, 0, 0, 0, 1, 46, 16, 17, 0, 0, 0, 0, 0, 0, 36, 3, 0, 0, 15, 30, 0, 22, 24, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 15, 16, 17, 0, 0, 0, 0, 0, }, new List<int> { 16, 45, 3, 0, 0, 1, 2, 2, 3, 1, 3, 0, 0, 1, 37, 0, 0, 1, 46, 17, 0, 0, 0, 1, 2, 3, 0, 0, 15, 30, 0, 0, 0, 40, 23, 24, 22, 24, 0, 0, 0, 0, 0, 0, 22, 35, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 8, 10, 0, 0, 8, 5, 16, 38, 39, 17, 0, 1, 46, 17, 0, 0, 8, 10, 0, 0, 0, 33, 39, 16, 17, 0, 0, 0, 18, 0, 0, 33, 49, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 47, 32, 3, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 15, 45, 2, 2, 46, 17, 0, 0, 0, 0, 0, 15, 45, 3, 0, 0, 15, 45, 3, 0, 0, 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 15, 45, 3, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 15, 6, 5, 17, 0, 0, 1, 3, 0, 0, 0, 8, 10, 0, 0, 0, 8, 10, 0, 0, 36, 31, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 18, 0, 0, 0, 0, 0, 15, 17, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 3, 0, 0, 8, 10, 0, 0, 1, 46, 17, 0, 1, 2, 13, 12, 3, 0, 0, 15, 17, 0, 0, 8, 10, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 1, 3, 0, 0, 4, 0, 0, 0, 0, 22, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 17, 0, 1, 13, 10, 0, 0, 8, 10, 0, 0, 15, 16, 16, 16, 17, 0, 0, 0, 0, 0, 33, 39, 17, 0, 0, 0, 0, 18, 0, 0, 0, 0, 1, 46, 38, 24, 0, 18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 15, 16, 38, 24, 0, 15, 45, 2, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 18, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 15, 17, 0, 0, 0, 0, 22, 32, 3, 0, 0, 1, 31, 23, 24, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 15, 16, 45, 2, 2, 2, 3, 0, 22, 24, 0, 0, 0, 0, 0, 0, 0, 18, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 21, 32, 31, 39, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 2, 3, 0, 0, 0, 22, 24, 0, 0, 0, 15, 6, 9, 5, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 18, 0, 0, 0, 0, 1, 31, 32, 2, 2, 46, 17, 15, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 16, 38, 32, 3, 0, 0, 0, 0, 0, 0, 0, 8, 5, 17, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 22, 23, 24, 0, 0, 0, 0, 0, 0, 0, 15, 17, 15, 16, 16, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 15, 38, 23, 24, 0, 0, 0, 0, 0, 15, 17, 0, 0, 0, 0, 0, 22, 24, 0, 36, 3, 0, 0, 0, 0, 0, 0, 0, 22, 23, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 1, 3, 0, 0, 0, 0, 1, 3, 0, 0, 0, 0, 0, 15, 38, 24, 0, 22, 23, 23, 24, 0, 0, 0, 0, 0, 22, 24, 0, 1, 2, 3, 0, 0, 29, 16, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 22, 24, 0, 0, 0, 1, 46, 45, 2, 2, 2, 2, 46, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 3, 0, 0, 0, 0, 15, 6, 12, 2, 31, 49, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 1, 2, 46, 17, 15, 16, 16, 16, 16, 17, 0, 0, 0, 1, 3, 0, 0, 0, 0, 0, 0, 1, 3, 0, 0, 0, 8, 12, 3, 0, 0, 0, 0, 15, 16, 16, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 15, 16, 17, 0, 0, 0, 0, 0, 0, 0, 0, 22, 32, 13, 12, 3, 0, 0, 0, 1, 2, 46, 45, 3, 0, 0, 15, 16, 45, 2, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 16, 16, 17, 0, 0, 22, 39, 16, 17, 15, 17, 0, 0, 0, 0, 15, 16, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 1, 31, 23, 23, 23, 24, 0, 0, 4, 0, 0, 1, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 31, 14, 10, 0, 0, 0, 0, 0, 1, 37, 0, 0, 15, 17, 0, 1, 2, 3, 0, 0, 1, 31, 23, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 17, 15, 45, 2, 3, 0, 0, 0, 15, 45, 3, 0, 0, 0, 0, 15, 6, 10, 0, 0, 15, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 15, 16, 17, 0, 0, 0, 0, 15, 45, 3, 0, 0, 0, 0, 15, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 22, 24, 0, 0, 15, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } }; ;

            //List<List<int>> intermediate = new List<List<int>> { new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 473, 508, 496, 0, 0, 0, 0, 0, 0, 0, 0, 0, 468, 504, 496, 0, 0, 525, 530, 0, 0, }, new List<int> { 0, 473, 496, 0, 0, 473, 475, 0, 525, 526, 501, 502, 0, 0, 0, 0, 0, 0, 468, 475, 0, 500, 506, 507, 470, 0, 0, 0, 0, 0, }, new List<int> { 0, 558, 533, 529, 529, 526, 507, 508, 474, 505, 501, 502, 0, 0, 494, 504, 470, 0, 558, 560, 0, 532, 533, 533, 534, 0, 0, 0, 525, 530, }, new List<int> { 0, 0, 0, 0, 0, 532, 533, 559, 533, 559, 533, 534, 0, 0, 532, 533, 534, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 473, 475, 0, 0, 0, 591, 0, 591, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 525, 530, 0, 0, }, new List<int> { 0, 0, 500, 507, 504, 475, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 494, 474, 504, 508, 496, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 500, 506, 506, 502, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 558, 533, 533, 533, 534, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 500, 506, 506, 502, 0, 431, 0, 468, 470, 0, 0, 0, 0, 0, 0, 525, 530, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 532, 533, 533, 534, 0, 591, 0, 500, 507, 469, 504, 508, 469, 470, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 532, 533, 533, 533, 533, 533, 534, 0, 0, 0, 0, 473, 508, 469, 469, 496, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 532, 533, 533, 533, 534, 0, 0, 0, 0, 0, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 469, 508, 474, 475, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 501, 501, 506, 507, 474, 474, 508, 475, 0, 494, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new List<int> { 533, 533, 533, 533, 533, 533, 533, 560, 525, 526, 508, 474, 475, 468, 474, 496, 431, 431, 468, 504, 469, 508, 496, 431, 468, 474, 496, 431, 473, 475, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 525, 526, 533, 533, 534, 532, 533, 534, 591, 591, 558, 533, 533, 533, 560, 591, 532, 533, 560, 591, 532, 534, }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };


            //Create the map of solid tiles
            List<List<bool>> solidMap= new List<List<bool>>();

            //Reset the map
            for (int x = 0; x < _cellCountX; x++)
            {
                solidMap.Add(new List<bool>());
                for (int y = 0; y < _cellCountY; y++)
                {
                    solidMap[x].Add(false);
                }
            }

            //Generate large islands
            Perlin perlinLarge = new Perlin(16*12, 100, 100);
            
            for(int i = 0; i < 0; i++)
            {
                perlinLarge.Generate();

                for (int x = 0; x < _cellCountX; x++)
                {
                    for (int y = 0; y < _cellCountY; y++)
                    {
                        if (Math.Abs(perlinLarge[x * 16, y * 16]) > 0.3f)
                        {
                            solidMap[x][y] = true;
                        }
                    }
                }
            }




            //Generate large islands a different way
            Perlin perlinLarge2 = new Perlin(16 * 6, 100, 100);


            for (int i = 0; i < 1; i++)
            {
                perlinLarge2.Generate();

                for (int x = 0; x < _cellCountX; x++)
                {
                    for (int y = 0; y < _cellCountY; y++)
                    {
                        if (perlinLarge2[x * 16, y * 16] > 0.15f)
                        {
                            solidMap[x][y] = true;
                        }
                    }
                }
            }
            

            //Generate small islands away from large islands

            Perlin perlinSmall = new Perlin(16 * 6, 100, 100);


            for (int i = 0; i < 10; i++) { 
                perlinSmall.Generate();

                for (int x = 0; x < _cellCountX; x++)
                {
                    for (int y = 0; y < _cellCountY; y++)
                    {
                        if (-Math.Abs(perlinSmall[x * 16, y * 16]) * perlinLarge2[x * 16, y * 16] > 0.2f)
                        {
                            solidMap[x][y] = true;
                        }
                    }
                }      
            }


            Dictionary<List<bool>, int> dictionary = new Dictionary<List<bool>, int>(100,new ListComparer<bool>()){ 
                {new List<bool>{false,false,false,false,false,false,false,false},-1},
                {new List<bool>{false,false,false,false,false,false,true,false},529},
                {new List<bool>{false,false,false,false,false,true,true,false},529},
                {new List<bool>{false,false,false,false,false,false,true,true},529},
                {new List<bool>{false,false,false,false,false,true,true,true},529},

                {new List<bool>{false,false,false,false,true,false,false,false},430},
                {new List<bool>{false,false,false,true,true,false,false,false},430},
                {new List<bool>{false,false,false,false,true,true,false,false},430},
                {new List<bool>{false,false,false,true,true,true,false,false},430},

                {new List<bool>{false,false,false,false,true,true,true,false},469},
                {new List<bool>{false,false,false,true,true,true,true,false},469},
                {new List<bool>{false,false,false,false,true,true,true,true},469},
                {new List<bool>{false,false,false,true,true,true,true,true},469},

                {new List<bool>{false,false,false,false,true, false, true,false},469},
                {new List<bool>{false,false,false,true,true, false, true,false},469},
                {new List<bool>{false,false,false,false,true, false, true,true},469},
                {new List<bool>{false,false,false,true,true, false, true,true},469},


                //{new List<bool>{false,false,false,false,true,true,true,false},495},
                //{new List<bool>{false,false,false,false,true,true,true,false},474},
                {new List<bool>{false,false,true,false,false,false,false,false},524},
                {new List<bool>{false,true,true,false,false,false,false,false},524},
                {new List<bool>{false,false,true,true,false,false,false,false},524},
                {new List<bool>{false,true,true,true,false,false,false,false},524},

                {new List<bool>{false,false,true,false,false,false,true,false},528},
                {new List<bool>{false,true,true,false,false,false,true,false},528},
                {new List<bool>{false,false,true,true,false,false,true,false},528},
                {new List<bool>{false,true,true,true,false,false,true,false},528},


                {new List<bool>{false,false,true,false,false,true,true,false},528},
                {new List<bool>{false,true,true,false,false, true, true,false},528},
                {new List<bool>{false,false,true,true,false, true, true,false},528},
                {new List<bool>{false,true,true,true,false, true, true,false},528},


                {new List<bool>{false,false,true,false,false,false,true,true},528},
                {new List<bool>{false,true,true,false,false,false,true,true},528},
                {new List<bool>{false,false,true,true,false,false,true,true},528},
                {new List<bool>{false,true,true,true,false,false,true,true},528},



                {new List<bool>{false,false,true,false,false, true, true,true},528},
                {new List<bool>{false,true,true,false,false, true, true,true},528},
                {new List<bool>{false,false,true,true,false, true, true,true},528},
                {new List<bool>{false,true,true,true,false, true, true,true},528},



                {new List<bool>{false,false,true,true,true,false,false,false},467},
                {new List<bool>{false, true, true,true,true,false,false,false},467},
                {new List<bool>{false,false,true,true,true, true, false,false},467},
                {new List<bool>{false, true, true,true,true, true, false,false},467},

               {new List<bool>{false,false,true, false, true,false,false,false},467},
                {new List<bool>{false, true, true, false, true,false,false,false},467},
                {new List<bool>{false,false,true, false, true, true, false,false},467},
                {new List<bool>{false, true, true, false, true, true, false,false},467},

                //{new List<bool>{false,false,true,true,true,false,false,false},472},
                //{new List<bool>{false,false,true,true,true,false,false,false},493},
                {new List<bool>{false,false,true, false, true, false, true,false},468},
                {new List<bool>{false, true, true, false, true, false, true,false},468},
                {new List<bool>{false,false,true, false, true, false, true,true},468},
                {new List<bool>{false, true, true, false, true, false, true,true},468},



                {new List<bool>{false,false,true,true,true, false, true,false},468},
                {new List<bool>{false, true, true,true,true, false, true,false},468},
                {new List<bool>{false,false,true,true,true, false, true,true},468},
                {new List<bool>{false, true, true,true,true, false, true,true},468},


                {new List<bool>{false,false,true, false, true,true,true,false},468},
                {new List<bool>{false, true, true, false, true,true,true,false},468},
                {new List<bool>{false,false,true, false, true,true,true,true},468},
                {new List<bool>{false, true, true, false, true,true,true,true},468},



                {new List<bool>{false,false,true,true,true,true,true,false},468},
                {new List<bool>{false, true, true,true,true,true,true,false},468},
                {new List<bool>{false,false,true,true,true,true,true,true},468},
                {new List<bool>{false, true, true,true,true,true,true,true},468},






                //{new List<bool>{false,false,true,true,true,true,true,false},473},
                //{new List<bool>{false,false,true,true,true,true,true,false},503},
                //{new List<bool>{false,false,true,true,true,true,true,false},507},
                
                {new List<bool>{true,false,false,false,false,false,false,false},590},
                {new List<bool>{true,true,false,false,false,false,false,false},590},
                {new List<bool>{true,false,false,false,false,false,false,true},590},
                {new List<bool>{true, true, false,false,false,false,false,true},590},




                {new List<bool>{true,false,false,false,false,false,true,true},559},
                {new List<bool>{true,false,false,false,false, true, true,true},559},
                {new List<bool>{true, true, false,false,false,false,true,true},559},
                {new List<bool>{true, true, false,false,false, true, true,true},559},


                {new List<bool>{true,false,false,false,false,false,true,false},559},
                {new List<bool>{true,false,false,false,false, true, true,false},559},
                {new List<bool>{true, true, false,false,false,false,true,false},559},
                {new List<bool>{true, true, false,false,false, true, true,false},559},


                //{new List<bool>{true,false,false,false,false,false,true,true},533},
                {new List<bool>{true,false,false,false,true,false,false,false},462},
                {new List<bool>{true, true, false,false,true,false,false,false},462},
                {new List<bool>{true,false,false,false,true,false,false,true},462},
                {new List<bool>{true, true, false,false,true,false,false,true},462},


                {new List<bool>{true,false,false, true, true,false,false,false},462},
                {new List<bool>{true, true, false, true, true,false,false,false},462},
                {new List<bool>{true,false,false, true, true,false,false,true},462},
                {new List<bool>{true, true, false, true, true,false,false,true},462},


                {new List<bool>{true,false,false,false,true, true, false,false},462},
                {new List<bool>{true, true, false,false,true, true, false,false},462},
                {new List<bool>{true,false,false,false,true, true, false,true},462},
                {new List<bool>{true, true, false,false,true, true, false,true},462},


                {new List<bool>{true,false,false, true, true, true, false,false},462},
                {new List<bool>{true, true, false, true, true, true, false,false},462},
                {new List<bool>{true,false,false, true, true, true, false,true},462},
                {new List<bool>{true, true, false, true, true, true, false,true},462},



                {new List<bool>{true,false,false,false,true, false, true,false},501},
                {new List<bool>{true, true, false,false,true, false, true,false},501},
                {new List<bool>{true,false,false, true, true, false, true,false},501},
                {new List<bool>{true, true, false, true, true, false, true,false},501},

               {new List<bool>{true,false,false,false,true,true,true,false},501},
                {new List<bool>{true, true, false,false,true,true,true,false},501},
                {new List<bool>{true,false,false, true, true,true,true,false},501},
                {new List<bool>{true, true, false, true, true,true,true,false},501},

                {new List<bool>{true,false,false,false,true, false, true,true},501},
                {new List<bool>{true, true, false,false,true, false, true,true},501},
                {new List<bool>{true,false,false, true, true, false, true,true},501},
                {new List<bool>{true, true, false, true, true, false, true,true},501},

               {new List<bool>{true,false,false,false,true,true,true,true},501},
                {new List<bool>{true, true, false,false,true,true,true,true},501},
                {new List<bool>{true,false,false, true, true,true,true,true},501},
                {new List<bool>{true, true, false, true, true,true,true,true},501},





                {new List<bool>{true,false,true,false,true,true,true,true},527},
                {new List<bool>{true,false,true,true,true,true,true,false},494},
                {new List<bool>{true,false,true,true,true,true,true,true},506},
                {new List<bool>{true,true,true,false,false,false,false,false},531},
                {new List<bool>{true,true,true, true, false,false,false,false},531},
                {new List<bool>{true,true,true,false,false,false,false,true},531},
                {new List<bool>{true,true,true, true, false,false,false,true},531},


                {new List<bool>{true, false, true,false,false,false,false,false},531},
                {new List<bool>{true, false, true, true, false,false,false,false},531},
                {new List<bool>{true, false, true,false,false,false,false,true},531},
                {new List<bool>{true, false, true, true, false,false,false,true},531},

                //{new List<bool>{true,true,true,false,false,false,false,false},557},
                {new List<bool>{true, false, true,false,false,false,true,false},532},
                {new List<bool>{true, false, true, true, false,false,true,false},532},
                {new List<bool>{true, false, true,false,false, true, true,false},532},
                {new List<bool>{true, false, true, true, false, true, true,false},532},

                {new List<bool>{true,true,true,false,false,false,true,false},532},
                {new List<bool>{true,true,true, true, false,false,true,false},532},
                {new List<bool>{true,true,true,false,false, true, true,false},532},
                {new List<bool>{true,true,true, true, false, true, true,false},532},

                {new List<bool>{true, false, true,false,false,false,true,true},532},
                {new List<bool>{true, false, true, true, false,false,true,true},532},
                {new List<bool>{true, false, true,false,false, true, true,true},532},
                {new List<bool>{true, false, true, true, false, true, true,true},532},

                {new List<bool>{true,true,true,false,false,false,true,true},532},
                {new List<bool>{true,true,true, true, false,false,true,true},532},
                {new List<bool>{true,true,true,false,false, true, true,true},532},
                {new List<bool>{true,true,true, true, false, true, true,true},532},


                {new List<bool>{true,true,true,false,true,false,true,true},558},


                {new List<bool>{true, false, true, false, true,false,false,false},499},
                {new List<bool>{true, false, true, false, true, true, false,false},499},
                {new List<bool>{true, false, true, false, true,false,false,true},499},
                {new List<bool>{true, false, true, false, true, true, false,true},499},


                {new List<bool>{true,true,true, false, true,false,false,false},499},
                {new List<bool>{true,true,true, false, true, true, false,false},499},
                {new List<bool>{true,true,true, false, true,false,false,true},499},
                {new List<bool>{true,true,true, false, true, true, false,true},499},


                {new List<bool>{true, false, true,true,true,false,false,false},499},
                {new List<bool>{true, false, true,true,true, true, false,false},499},
                {new List<bool>{true, false, true,true,true,false,false,true},499},
                {new List<bool>{true, false, true,true,true, true, false,true},499},


                {new List<bool>{true,true,true,true,true,false,false,false},499},
                {new List<bool>{true,true,true,true,true, true, false,false},499},
                {new List<bool>{true,true,true,true,true,false,false,true},499},
                {new List<bool>{true,true,true,true,true, true, false,true},499},


                {new List<bool>{true,true,true,true,true,false,true,false},525},
                
                {new List<bool>{true,true,true,true,true,true,true,false},504},
                {new List<bool>{true,true,true,true,true,true,true,true},505},
                //{new List<bool>{true,true,true,true,true,true,true,true},500},
                };

            _map = WangTool.TranslateWangIDmap(WangTool.wangIDMap(solidMap),dictionary, 505);


            base.LoadContent();
            _colliderSensor._tags.Add("Solid");
            _colliderSensor._sCC = new ColliderSensor.ShouldCollideCheck(ShouldCollideCheck);
        }

        /// <summary>
        /// The function used to create the delegate that filters the collisions<br/>
        /// It returns true if the other collider corresponds to an actor<br/>
        /// </summary>
        public bool ShouldCollideCheck(ColliderSensor self, Collider other)
        {
            return other._tags.Contains("Actor");
        }

    }
}
