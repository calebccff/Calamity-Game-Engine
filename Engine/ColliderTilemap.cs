using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace TestShader
{
    //Can't move!

    /// <summary>
    /// Represents a tile that can be collided with<br/>
    /// </summary>
    public class ColliderTile : Tile
    {
        /// <summary>
        /// The hitboxes of the tile<br/>
        /// </summary>
        public List<Rectangle> _rect = new List<Rectangle>();
    }

    /// <summary>
    /// Represents a tilemap that can be collided with<br/>
    /// Can't move, as it is not implemented as a Solid<br/>
    /// </summary>
    public class ColliderTilemap : Tilemap
    {

        /// <summary>
        /// The collider sensor of the tilemap<br/>
        /// It will contain all hitboxes from all tiles<br/>
        /// </summary>
        public ColliderSensor _colliderSensor;

        /// <summary>
        /// Creates a new collider tilemap<br/>
        /// Initializes the collider sensor<br/>
        /// </summary>
        /// <param name="texture"> The texture of the tilemap</param>
        /// <param name="cellCountX"> The amount of cells in the x direction</param>
        /// <param name="cellCountY"> The amount of cells in the y direction</param>
        public ColliderTilemap(Texture2D texture, int cellCountX = 1, int cellCountY = 1) : base(texture, cellCountX, cellCountY)
        {
            _colliderSensor = new ColliderSensor(this);
        }

        /// <summary>
        /// LoadContent,<br/>
        /// sets the position of the the collider<br/>
        /// Adds all hitboxes to the collider with the correct offset<br/>
        /// </summary>
        protected override void LoadContent()
        {
            _colliderSensor._position = _position;
            for (int x = 0; x < _cellCountX; x++)
            {
                for (int y = 0; y < _cellCountY; y++)
                {

                    if (_tileSet[_map[x][y]] is ColliderTile)
                    {
                        foreach (Rectangle rect in (_tileSet[_map[x][y]] as ColliderTile)._rect)
                        {
                            Rectangle rect2 = new Rectangle(rect.Location, rect.Size);

                            rect2.Offset(new Point(x * _cellWidth, y * _cellHeight));
                            _colliderSensor.AddHitbox(rect2);
                        }
                    }
                }
            }
            base.LoadContent();
        }

        /// <summary>
        /// Update<br/>
        /// Sets the position of the collider (not used, it shoudln't move)<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            _colliderSensor._position = _position;
            base.Update(gameTime);
        }


        //TODO: Finish TMX loading!!

        /// <summary>
        /// Loads a tilemap from an XML file (from the TMX format)<br/>
        /// Currently only imports the colliders for each tile<br/>
        /// </summary>
        /// <param name="Url"></param>
        public void LoadXML(String Url)
        {
            //Load XML
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(Url);

            //Get all tiles from the xml
            XmlNodeList tiles = xDoc.GetElementsByTagName("tile");

            int id;

            foreach (XmlNode tileData in tiles)
            {
                //Get id of the tile
                id = Int32.Parse(tileData.Attributes.GetNamedItem("id").Value);
                
                // Check if the tile has colliders
                if (tileData.ChildNodes.Count != 0)
                    //Iterate through each collider
                    foreach (XmlNode rectData in tileData.ChildNodes[0])
                    {
                        //Check if the collider has x, y, width, and height
                        if (rectData.Attributes.GetNamedItem("x") != null && rectData.Attributes.GetNamedItem("y") != null &&
                            rectData.Attributes.GetNamedItem("width") != null && rectData.Attributes.GetNamedItem("height") != null)
                        {
                            //Create the collider by extracting the values x, y, width, and height scaled with _scale
                            (_tileSet[id + 1] as ColliderTile)._rect.Add(new Rectangle(new Point((int)(Int32.Parse(rectData.Attributes.GetNamedItem("x").Value) * _scale.X), (int)(Int32.Parse(rectData.Attributes.GetNamedItem("y").Value) * _scale.Y)), new Point((int)(Int32.Parse(rectData.Attributes.GetNamedItem("width").Value) * _scale.X), (int)(Int32.Parse(rectData.Attributes.GetNamedItem("height").Value) * _scale.Y))));
                        }

                        
                    }

            }

        }

    }
}