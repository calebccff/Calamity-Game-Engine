using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Runtime.CompilerServices;
using System;

namespace TestShader
{
    /// <summary>
    /// Represents a texture to be drawn to the screen<br/>
    /// </summary>  
    class Drawable
    {
        /// <summary>
        /// The texture to be drawn<br/>
        /// </summary>
        public Texture2D _texture;

        /// <summary>
        /// The position of the texture on the screen<br/>
        /// </summary>
        public Vector2? _position = null;

        /// <summary>
        /// The destination on the screen for the texture<br/>
        /// </summary>
        public Rectangle? _destinationRectangle = null;

        /// <summary>
        /// The part of the texture to be drawn<br/>
        /// </summary>
        public Rectangle? _sourceRectangle = null;

        /// <summary>
        /// The color to draw the texture with<br/>
        /// </summary>
        public Color _color = Color.White;

        /// <summary>
        /// The rotation for the drawn texture<br/>
        /// </summary>
        public float _rotation = 0f;

        /// <summary>
        /// The origin of the rotation for the drawn texture<br/>
        /// </summary>
        public Vector2 _origin = new Vector2(0, 0);

        /// <summary>
        /// The scale for the drawn texture<br/>
        /// </summary>
        public Vector2 _scale = new Vector2(1, 1);

        /// <summary>
        /// If the texture should be flipped or not<br/>
        /// </summary>
        public SpriteEffects _effects = SpriteEffects.None;

        /// <summary>
        /// The depth of the drawn texture on screen<br/>
        /// It determines the order in which the textures are drawn<br/>
        /// </summary>
        public float _layerDepth = 1f;
        //TODO: Organize layer depths
        //enums
        //Background:
        //  Far
        //  Middle
        //  Near
        //Level Geometry:
        //  TileMaps
        //  Solids
        //  JumpThroughs
        //Sprites:
        //  Actors
        //  Projectiles
        //  Particles
        //  Player
        //UI:
        //  GameUI
        //  WindowUI
        //  Terminal

        /// <summary>
        /// A function delegate that returns the current position of the texture depending on the camera position<br/>
        /// Useful for things that need to be rendered in relation to the camera<br/>
        /// </summary>
        /// <example> Could be used for backgrounds that are farther away from the camera than the coordinate plane. See <seealso cref="ParallaxBackground"/> as an example. </example>
        public Func<Camera, Vector2> dynamicPosition;

        /// <summary>
        /// The shader to apply to the current texture<br/>
        /// </summary>
        public Effect? _shader = null;


        /// <summary>
        /// Creates a new drawable<br/>
        /// </summary>
        /// <param name="texture"> The texture to be drawn</param>
        public Drawable(Texture2D texture)
        {
            if (texture == null) return;
            _texture = texture;
            _sourceRectangle = new Rectangle(new Point(0, 0), new Point(texture.Width, texture.Height));
        }

        /// <summary>
        /// Draws the texture onto the sprite batch passed to it<br/>
        /// The shader will be applied at the spriteBatch.end, handled by the SpriteRenderer class<br/>
        /// </summary>
        /// <param name="spriteBatch"> The spritebatch to draw to</param>
        /// <param name="camera"> The camera to use for the drawing</param>
        /// <exception cref="System.ArgumentException"> Can't draw if no position, destinationRectangle, or dynamic Position is set</exception>
        public virtual void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            // If position is set, draw at the position
            if (_position is Vector2 position)
            {
                // Apply the camera variables as well
                spriteBatch.Draw(_texture, (position - camera._position.ToVector2()) * camera._scale + camera.destinationRectangle.Location.ToVector2(), _sourceRectangle, _color, _rotation, _origin, _scale * camera._scale, _effects, _layerDepth);
            }
            // If dynamic position is set, draw at the dynamic position
            else if (dynamicPosition != null)
            {
                //Apply the camera variables as well
                spriteBatch.Draw(_texture, (dynamicPosition(camera) - camera._position.ToVector2()) * camera._scale + camera.destinationRectangle.Location.ToVector2(), _sourceRectangle, _color, _rotation, _origin, _scale * camera._scale, _effects, _layerDepth);
            }
            else
            {  
                // If destinationRectangle is set, draw at the destinationRectangle
                if (_destinationRectangle is Rectangle destinationRectangle)
                {
                    //Apply the scaling and camera to the destinationRectangle
                    Rectangle correctedDestinationRectangle = destinationRectangle;
                    correctedDestinationRectangle = new Rectangle((correctedDestinationRectangle.Location.ToVector2() * camera._scale).ToPoint(), (correctedDestinationRectangle.Size.ToVector2() * camera._scale).ToPoint());
                    correctedDestinationRectangle.Offset(camera.destinationRectangle.Location.ToVector2() - camera._position.ToVector2() * camera._scale);
                    spriteBatch.Draw(_texture, correctedDestinationRectangle, _sourceRectangle, _color, _rotation, _origin, _effects, _layerDepth);
                }
                else
                    throw new System.ArgumentException("Image cannot be rendered with no position or destinationRectangle");
            }
        }
    }

    /// <summary>
    /// Represents a text to be drawn on the screen<br/>
    /// </summary>
    class DrawableText : Drawable
    {
        /// <summary>
        /// The text to be drawn<br/>
        /// </summary>
        public StringBuilder _text;

        /// <summary>
        /// The font to be used<br/>
        /// </summary>
        public SpriteFont _font;

        public DrawableText() : base(null)
        {

        }

        /// <summary>
        /// Draws the text onto the sprite batch<br/>
        /// The shader will be applied at the spriteBatch.end, handled by the SpriteRenderer class<br/>
        /// </summary>
        /// <param name="spriteBatch"> The spritebatch to draw to</param>
        /// <param name="camera"> The camera to use for the drawing</param>
        /// <exception cref="System.ArgumentException"> Can't draw if no position is set</exception>
        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (_position is Vector2 position)
            {
                spriteBatch.DrawString(_font, _text, position + camera.destinationRectangle.Location.ToVector2(), _color, _rotation, _origin, _scale * camera._scale, _effects, _layerDepth);
            }
            else
            {
                throw new System.ArgumentException("String cannot be rendered with no position");
            }
        }

    }

    /// <summary>
    /// Class managing the drawing of sprites to the screen<br/>
    /// </summary>
    public class SpriteRenderer
    {
        /// <summary>
        /// The current shader effect applied<br/>
        /// </summary>
        public Effect _currentEffect = null;

        /// <summary>
        /// The temporary screen to draw to before final upscaling<br/>
        /// </summary>
        public RenderTarget2D _temporaryScreen;

        /// <summary>
        /// The graphics device for the game<br/>
        /// </summary>
        public GraphicsDevice _graphicsDevice;

        /// <summary>
        /// The spritebatch used for drawing<br/>
        /// </summary>
        public SpriteBatch _spriteBatch;

        /// <summary>
        /// The sort mode for rendering<br/>
        /// </summary>
        public SpriteSortMode _sortMode = SpriteSortMode.Deferred;

        /// <summary>
        /// The current blend state for rendering<br/>
        /// </summary>
        public BlendState _blendState = null;

        /// <summary>
        /// The current sampler state for rendering<br/>
        /// </summary>
        public SamplerState _samplerState = null;

        /// <summary>
        /// The current depth stencil state for rendering<br/>
        /// </summary>
        public DepthStencilState _depthStencilState = null;

        /// <summary>
        /// The current rasterizer state for rendering<br/>
        /// </summary>
        public RasterizerState _rasterizerState = null;

        /// <summary>
        /// The current transform matrix for rendering<br/>
        /// </summary>
        public Matrix? _transformMatrix = null;

        /// <summary>
        /// A white pixel texture for drawing monochrome objects<br/>
        /// </summary>
        public Texture2D Pixel;


        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteRenderer"/> class.<br/>
        /// Initializes the temporary screen the SpriteBatch<br/>
        /// Creates the default white pixel texture<br/>
        /// </summary>
        /// <param name="graphicsDevice"> The graphics device corresponding to the game</param>
        public SpriteRenderer()
        {
            _graphicsDevice = Game.I.GraphicsDevice;

            _temporaryScreen = new RenderTarget2D(_graphicsDevice, Game.I._canvasWidth, Game.I._canvasHeight, false, _graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            _spriteBatch = new SpriteBatch(_graphicsDevice);

            Pixel = new Texture2D(Game.I.GraphicsDevice, 1, 1);
            var colors = new Color[1];
            for (int i = 0; i < 1; i++)
                colors[i] = Color.White;
            Pixel.SetData<Color>(colors);


        }

        //TODO: suppport shader settings

        /// <summary>
        /// The list of drawables to draw this frame<br/>
        /// </summary>
        List<Drawable> drawables = new List<Drawable>();


        /// <summary>
        /// Draws a rectangle with a given color<br/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public void Rect(float x, float y, float width, float height, Color color)
        {
            Rectangle rect;
            rect.X = (int)(x);
            rect.Y = (int)(y);
            rect.Width = (int)width;
            rect.Height = (int)height;

            Draw(Pixel, rect, color);
        }


        /// <summary>
        /// Draws a rectangle with a given color<br/>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public void Rect(Vector2 position, float width, float height, Color color)
        {
            Rect(position.X, position.Y, width, height, color);
        }

        /// <summary>
        /// Draws a rectangle with a given color<br/>
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        public void Rect(Rectangle rect, Color color)
        {

            Draw(Pixel, rect, color);
        }

        /// <summary>
        /// Draws the hitboxes of a collider with a given color with filled rectangles<br/>
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="color"></param>
        public void Rect(Collider collider, Color color)
        {
            foreach (Rectangle _rect in collider._hitboxes)
                Rect(_rect.X + collider.X, _rect.Y + collider.Y, _rect.Width, _rect.Height, color);
        }


        /// <summary>
        /// Draws the 1px outline of a rectangle with a given color<br/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public void HollowRect(float x, float y, float width, float height, Color color, float depth = 0f)
        {
            Rectangle rect;
            rect.X = (int)(x);
            rect.Y = (int)(y);
            rect.Width = (int)width;
            rect.Height = 1;

            Draw(Pixel, rect, null, color, 0, new Vector2(0, 0), SpriteEffects.None, depth);

            rect.Y += (int)height - 1;

            Draw(Pixel, rect, null, color, 0, new Vector2(0, 0), SpriteEffects.None, depth);

            rect.Y -= (int)height - 1;
            rect.Width = 1;
            rect.Height = (int)height;

            Draw(Pixel, rect, null, color, 0, new Vector2(0, 0), SpriteEffects.None, depth);

            rect.X += (int)width - 1;

            Draw(Pixel, rect, null, color, 0, new Vector2(0, 0), SpriteEffects.None, depth);
        }


        /// <summary>
        /// Draws the 1px outline of a rectangle with a given color<br/>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public void HollowRect(Vector2 position, float width, float height, Color color)
        {
            HollowRect(position.X, position.Y, width, height, color);
        }


        /// <summary>
        /// Draws the 1px outline of a rectangle with a given color<br/>
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        public void HollowRect(Rectangle rect, Color color)
        {
            HollowRect(rect.X, rect.Y, rect.Width, rect.Height, color);
        }


        /// <summary>
        /// Draws the hitboxes of a collider with a given color with the outlines of rectangles<br/>
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="color"></param>
        public void HollowRect(Collider collider, Color color, float depth = 0f)
        {
            foreach (Rectangle _rect in collider._hitboxes)
                HollowRect(_rect.X + collider.X, _rect.Y + collider.Y, _rect.Width, _rect.Height, color, depth);
        }



        
        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            drawables.Add(new Drawable(texture)
            {

                _destinationRectangle = destinationRectangle,

                _color = color,

            });

        }


       /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Color color)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _color = color,

            });
        }


       /// <summary>
       /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
       /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _sourceRectangle = sourceRectangle,
                _color = color,

            });
        }

       /// <summary>
       /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
       /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Color color)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _color = color,

            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Rectangle? sourceRectangle, Color color)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _sourceRectangle = sourceRectangle,
                _color = color,

            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
        {
            drawables.Add(new Drawable(texture)
            {

                _destinationRectangle = destinationRectangle,
                _sourceRectangle = sourceRectangle,
                _color = color,

            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _sourceRectangle = sourceRectangle,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _scale = new Vector2(scale, scale),
                _effects = effects,
                _layerDepth = layerDepth
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _sourceRectangle = sourceRectangle,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _scale = scale,
                _effects = effects,
                _layerDepth = layerDepth
            });
        }
        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _sourceRectangle = sourceRectangle,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _scale = new Vector2(scale, scale),
                _effects = effects,
                _layerDepth = layerDepth
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _sourceRectangle = sourceRectangle,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _scale = scale,
                _effects = effects,
                _layerDepth = layerDepth
            });
        }
        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            drawables.Add(new Drawable(texture)
            {

                _destinationRectangle = destinationRectangle,
                _sourceRectangle = sourceRectangle,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _effects = effects,
                _layerDepth = layerDepth
            });
        }



        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Rectangle destinationRectangle)
        {
            drawables.Add(new Drawable(texture)
            {

                _destinationRectangle = destinationRectangle,

            });

        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _sourceRectangle = sourceRectangle,

            });
        }
        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Rectangle? sourceRectangle)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _sourceRectangle = sourceRectangle,

            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle)
        {
            drawables.Add(new Drawable(texture)
            {

                _destinationRectangle = destinationRectangle,
                _sourceRectangle = sourceRectangle,

            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _sourceRectangle = sourceRectangle,
                _rotation = rotation,
                _origin = origin,
                _scale = new Vector2(scale, scale),
                _effects = effects,
                _layerDepth = layerDepth
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _sourceRectangle = sourceRectangle,
                _rotation = rotation,
                _origin = origin,
                _scale = scale,
                _effects = effects,
                _layerDepth = layerDepth
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Rectangle? sourceRectangle, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _sourceRectangle = sourceRectangle,
                _rotation = rotation,
                _origin = origin,
                _scale = new Vector2(scale, scale),
                _effects = effects,
                _layerDepth = layerDepth
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Rectangle? sourceRectangle, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _sourceRectangle = sourceRectangle,
                _rotation = rotation,
                _origin = origin,
                _scale = scale,
                _effects = effects,
                _layerDepth = layerDepth
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            drawables.Add(new Drawable(texture)
            {

                _destinationRectangle = destinationRectangle,
                _sourceRectangle = sourceRectangle,
                _rotation = rotation,
                _origin = origin,
                _effects = effects,
                _layerDepth = layerDepth
            });
        }






        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {

                _destinationRectangle = destinationRectangle,

                _color = color,
                _shader = shader

            });

        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Color color, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _color = color,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _sourceRectangle = sourceRectangle,
                _color = color,
                _shader = shader

            });
        }
        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Color color, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _color = color,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Rectangle? sourceRectangle, Color color, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _sourceRectangle = sourceRectangle,
                _color = color,
                _shader = shader

            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {

                _destinationRectangle = destinationRectangle,
                _sourceRectangle = sourceRectangle,
                _color = color,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _sourceRectangle = sourceRectangle,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _scale = new Vector2(scale, scale),
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _sourceRectangle = sourceRectangle,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _scale = scale,
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }
        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _sourceRectangle = sourceRectangle,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _scale = new Vector2(scale, scale),
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _sourceRectangle = sourceRectangle,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _scale = scale,
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }
        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {

                _destinationRectangle = destinationRectangle,
                _sourceRectangle = sourceRectangle,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }



        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Rectangle destinationRectangle, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {

                _destinationRectangle = destinationRectangle,
                _shader = shader
            });

        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _sourceRectangle = sourceRectangle,
                _shader = shader
            });
        }


        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {

                _destinationRectangle = destinationRectangle,
                _sourceRectangle = sourceRectangle,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _sourceRectangle = sourceRectangle,
                _rotation = rotation,
                _origin = origin,
                _scale = new Vector2(scale, scale),
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                _position = position,

                _sourceRectangle = sourceRectangle,
                _rotation = rotation,
                _origin = origin,
                _scale = scale,
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }
        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {

                _destinationRectangle = destinationRectangle,
                _sourceRectangle = sourceRectangle,
                _rotation = rotation,
                _origin = origin,
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Rectangle? sourceRectangle, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _sourceRectangle = sourceRectangle,
                _shader = shader
            });
        }
        /// <summary>
        /// Draws a texture onto the screen with given parameters. Check <seealso cref="Drawable"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Rectangle? sourceRectangle, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _sourceRectangle = sourceRectangle,
                _rotation = rotation,
                _origin = origin,
                _scale = new Vector2(scale, scale),
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a string onto the screen with given parameters. Check <seealso cref="DrawableText"/> for details of parameters.<br/>
        /// </summary>
       public void Draw(Texture2D texture, Func<Camera, Vector2> position, Rectangle? sourceRectangle, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, Effect shader)
        {
            drawables.Add(new Drawable(texture)
            {
                dynamicPosition = position,

                _sourceRectangle = sourceRectangle,
                _rotation = rotation,
                _origin = origin,
                _scale = scale,
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }


        /// <summary>
        /// Draws a string onto the screen with given parameters. Check <seealso cref="DrawableText"/> for details of parameters.<br/>
        /// </summary>
        public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, Effect shader = null)
        {
            drawables.Add(new DrawableText()
            {

                _font = spriteFont,
                _text = text,
                _position = position,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _scale = scale,
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a string onto the screen with given parameters. Check <seealso cref="DrawableText"/> for details of parameters.<br/>
        /// </summary>
        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, Effect shader = null)
        {
            drawables.Add(new DrawableText()
            {

                _font = spriteFont,
                _text = new StringBuilder(text),
                _position = position,
                _color = color,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a string onto the screen with given parameters. Check <seealso cref="DrawableText"/> for details of parameters.<br/>
        /// </summary>
        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, Effect shader = null)
        {
            drawables.Add(new DrawableText()
            {

                _font = spriteFont,
                _text = new StringBuilder(text),
                _position = position,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a string onto the screen with given parameters. Check <seealso cref="DrawableText"/> for details of parameters.<br/>
        /// </summary>
        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, Effect shader = null)
        {
            drawables.Add(new DrawableText()
            {

                _font = spriteFont,
                _text = new StringBuilder(text),
                _position = position,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _scale = scale,
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a string onto the screen with given parameters. Check <seealso cref="DrawableText"/> for details of parameters.<br/>
        /// </summary>
        public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, Effect shader = null)
        {
            drawables.Add(new DrawableText()
            {

                _font = spriteFont,
                _text = text,
                _position = position,
                _color = color,
                _shader = shader
            });
        }

        /// <summary>
        /// Draws a string onto the screen with given parameters. Check <seealso cref="DrawableText"/> for details of parameters.<br/>
        /// </summary>
        public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, Effect shader = null)
        {
            drawables.Add(new DrawableText()
            {

                _font = spriteFont,
                _text = text,
                _position = position,
                _color = color,
                _rotation = rotation,
                _origin = origin,
                _effects = effects,
                _layerDepth = layerDepth,
                _shader = shader
            });
        }


        /// <summary>
        /// Renders all the drawables onto the screen<br/>
        /// Check <seealso cref="SpriteBatch.End"/> for details of parameters<br/>
        /// It sorts the drawables based on the layer depth<br/>
        /// Renders each drawable for each camera<br/>
        /// Handles the shader effects by restarting rendering for each different shader<br/>
        /// It calls the drawing of the Command Line and the UI as well<br/>
        /// </summary>
        public void Render(Effect shader, GameTime gameTime, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, Matrix? transformMatrix = null)
        {
            RasterizerState rasterizerState;
            // Sorting List using Lambda Function as comparator
            //str1<str2
            static int compareStrings(string str1, string str2)
            {
                return Comparer.DefaultInvariant.Compare(str1, str2);
            }


            drawables.Sort(delegate (Drawable c1, Drawable c2)
            {
                if (c1._layerDepth < c2._layerDepth)
                    return 1;
                else if (c1._layerDepth > c2._layerDepth)
                    return -1;
                else
                    return compareStrings(c1._shader.Name, c2._shader.Name);
            });


            foreach (Camera camera in Game.I.Cameras)
            {
                if (!camera.active) continue;

                if (blendState == null)
                {
                    blendState = BlendState.AlphaBlend;
                }
                _currentEffect = null;

                _sortMode = sortMode;
                _blendState = blendState;
                _samplerState = samplerState;
                _depthStencilState = depthStencilState;


                RasterizerState _rats = new RasterizerState();
                _rats.MultiSampleAntiAlias = _spriteBatch.GraphicsDevice.RasterizerState.MultiSampleAntiAlias;
                _rats.DepthClipEnable = _spriteBatch.GraphicsDevice.RasterizerState.DepthClipEnable;
                _rats.DepthBias = _spriteBatch.GraphicsDevice.RasterizerState.DepthBias;
                _rats.SlopeScaleDepthBias = _spriteBatch.GraphicsDevice.RasterizerState.SlopeScaleDepthBias;
                _rats.CullMode = _spriteBatch.GraphicsDevice.RasterizerState.CullMode;
                _rats.FillMode = _spriteBatch.GraphicsDevice.RasterizerState.FillMode;
                _rats.ScissorTestEnable = true;
                Rectangle _globalCutRect = _spriteBatch.GraphicsDevice.ScissorRectangle;

                rasterizerState = _rats;

                _rasterizerState = _rats;
                _transformMatrix = transformMatrix;


                // draw to the renderTarget, instead of to the screen:
                _graphicsDevice.SetRenderTarget(_temporaryScreen);
                _spriteBatch.GraphicsDevice.ScissorRectangle = camera.destinationRectangle;

                _spriteBatch.Begin(sortMode, blendState, effect: _currentEffect, samplerState: samplerState, depthStencilState: depthStencilState, rasterizerState: rasterizerState, transformMatrix: transformMatrix);


                foreach (Drawable drawable in drawables)
                {
                    if (_currentEffect != drawable._shader)
                    {
                        _currentEffect = drawable._shader;
                        _spriteBatch.End();
                        _spriteBatch.Begin(sortMode: _sortMode, blendState: _blendState, samplerState: _samplerState, depthStencilState: _depthStencilState, rasterizerState: _rasterizerState, effect: _currentEffect, transformMatrix: _transformMatrix);
                    }

                    drawable.Draw(_spriteBatch, camera);
                }



                _spriteBatch.End();
            }

            // no more render target; we'll now draw to the screen!
            _graphicsDevice.SetRenderTarget(null);
            UIComponent.SDraw(gameTime, _spriteBatch);

            if (Game.I.commands.Open)
                Game.I.commands.Draw(_spriteBatch);


            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, effect: shader);
            _spriteBatch.Draw(_temporaryScreen, new Vector2(0, 0), Color.White);
            _spriteBatch.End();


        }

        /// <summary>        
        /// Renders all the drawables onto the screen<br/>
        /// Check <seealso cref="SpriteBatch.End"/> for details of parameters<br/>
        /// It sorts the drawables based on the layer depth<br/>
        /// Renders each drawable for each camera<br/>
        /// Handles the shader effects by restarting rendering for each different shader<br/>
        /// It calls the drawing of the Command Line and the UI as well<br/>
        /// </summary>
        public void Render(GameTime gameTime, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, Matrix? transformMatrix = null)
        {
            RasterizerState rasterizerState;
            // Sorting List using Lambda Function as comparator
            //str1<str2
            static int compareStrings(string str1, string str2)
            {
                return Comparer.DefaultInvariant.Compare(str1, str2);
            }


            drawables.Sort(delegate (Drawable c1, Drawable c2)
            {
                if (c1._layerDepth < c2._layerDepth)
                    return 1;
                else if (c1._layerDepth > c2._layerDepth)
                    return -1;
                else
                {
                    if (c1._shader == null && c2._shader != null) return 1;
                    if (c2._shader == null && c1._shader != null) return -1;
                    if (c1._shader == null && c2._shader == null) return 0;
                    return compareStrings(c1._shader.Name, c2._shader.Name);
                }
            });

            foreach (Camera camera in Game.I.Cameras)
            {
                if (!camera.active) continue;


                if (blendState == null)
                {
                    blendState = BlendState.AlphaBlend;
                }
                _currentEffect = null;

                _sortMode = sortMode;
                _blendState = blendState;
                _samplerState = samplerState;
                _depthStencilState = depthStencilState;
                RasterizerState _rats = new RasterizerState();
                _rats.MultiSampleAntiAlias = _spriteBatch.GraphicsDevice.RasterizerState.MultiSampleAntiAlias;
                _rats.DepthClipEnable = _spriteBatch.GraphicsDevice.RasterizerState.DepthClipEnable;
                _rats.DepthBias = _spriteBatch.GraphicsDevice.RasterizerState.DepthBias;
                _rats.SlopeScaleDepthBias = _spriteBatch.GraphicsDevice.RasterizerState.SlopeScaleDepthBias;
                _rats.CullMode = _spriteBatch.GraphicsDevice.RasterizerState.CullMode;
                _rats.FillMode = _spriteBatch.GraphicsDevice.RasterizerState.FillMode;
                _rats.ScissorTestEnable = true;
                Rectangle _globalCutRect = _spriteBatch.GraphicsDevice.ScissorRectangle;


                _rasterizerState = _rats;
                rasterizerState = _rats;

                _transformMatrix = transformMatrix;


                // draw to the renderTarget, instead of to the screen:
                _graphicsDevice.SetRenderTarget(_temporaryScreen);
                _spriteBatch.GraphicsDevice.ScissorRectangle = camera.destinationRectangle;

                _spriteBatch.Begin(sortMode, blendState, effect: _currentEffect, samplerState: samplerState, depthStencilState: depthStencilState, rasterizerState: rasterizerState, transformMatrix: transformMatrix);





                foreach (Drawable drawable in drawables)
                {
                    if (_currentEffect != drawable._shader)
                    {
                        _currentEffect = drawable._shader;
                        _spriteBatch.End();
                        _spriteBatch.Begin(sortMode: _sortMode, blendState: _blendState, samplerState: _samplerState, depthStencilState: _depthStencilState, rasterizerState: _rasterizerState, effect: _currentEffect, transformMatrix: _transformMatrix);
                    }

                    drawable.Draw(_spriteBatch, camera);

                }

                _spriteBatch.End();

            }
            UIComponent.SDraw(gameTime, _spriteBatch);
            // no more render target; we'll now draw to the screen!
            _graphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_temporaryScreen, new Rectangle(0, 0, Game.I._canvasWidth * (int)Game.I._canvasScale.X, Game.I._canvasHeight * (int)Game.I._canvasScale.Y), Color.White);
            _spriteBatch.End();
            if (Game.I.commands.Open)
                Game.I.commands.Draw(_spriteBatch);
            drawables.Clear();
        }


        /// <summary>
        /// Disposes the spritebatch<br/>
        /// </summary>
        public void UnsafeDispose()
        {
            _spriteBatch.Dispose();

        }

    }
}
