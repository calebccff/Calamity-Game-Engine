using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// Struct for representing a values corresponding to the sides of a box<br/>
    /// </summary>
    /// <example>This could be used to represents margins or padding around a UI element</example>
    struct BoxProp
    {
        /// <summary>
        /// The value corresponding to the top of the box<br/>
        /// </summary>
        public int Top;
        /// <summary>
        /// The value corresponding to the left wall of the box<br/>
        /// </summary>
        public int Left;
        /// <summary>
        /// The value corresponding to the right wall of the box<br/>
        /// </summary>
        public int Right;
        /// <summary>
        /// The value corresponding to the bottom of the box<br/>
        /// </summary>
        public int Bottom;

        /// <summary>
        /// The vector of the top left values of the box<br/>
        /// </summary>
        public Point Position
        {
            get { return new Point(Left, Top); }
            set { Top = value.Y; Left = value.X; }
        }

        /// <summary>
        /// The size of the box (The sum of left and right values, the sum of top and bottom values)<br/>
        /// </summary>
        public Point Size
        {
            get { return new Point(Left + Right, Top + Bottom); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxProp"/> struct, with every value set to <paramref name="all"/><br/>
        /// </summary>
        /// <param name="all"></param>
        public BoxProp(int all)
        {
            this.Top = all;
            this.Left = all;
            this.Right = all;
            this.Bottom = all;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BoxProp"/> struct.<br/>
        /// </summary>
        /// <param name="updown">The value of the top and bottom of the box</param>
        /// <param name="leftright">The vaue of the left and right of the box</param>
        public BoxProp(int updown, int leftright)
        {
            this.Top = updown;
            this.Left = leftright;
            this.Right = leftright;
            this.Bottom = updown;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BoxProp"/> struct.<br/>
        /// </summary>
        /// <param name="up"> The value of the top of the box</param>
        /// <param name="leftright"> The value of the left and right of the box</param>
        /// <param name="down"> The value of the bottom of the box</param>
        public BoxProp(int up, int leftright, int down)
        {
            this.Top = up;
            this.Left = leftright;
            this.Right = leftright;
            this.Bottom = down;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BoxProp"/> struct.<br/>
        /// </summary>
        /// <param name="up"> The value of the top of the box</param>
        /// <param name="left"> The value of the left of the box</param>
        /// <param name="right"> The value of the right of the box</param>
        /// <param name="down"> The value of the bottom of the box</param>
        public BoxProp(int up, int left, int right, int down)
        {
            this.Top = up;
            this.Left = left;
            this.Right = right;
            this.Bottom = down;
        }
    }



    /// <summary>
    /// Class for handling all UI components<br/>
    /// Its static part handles the rendering of all UI components<br/>
    /// UIComponents are organized in a tree, each component being updated by the previous node<br/>
    /// The UI components use the CSS box model for padding borders margins<br/>
    /// </summary>
    class UIComponent
    {
        /// <summary>
        /// The static root of the UI tree<br/>
        /// </summary>
        static public UIComponent Root = new UIComponent();
        
        /// <summary>
        /// The static batch used to render the UI<br/>
        /// </summary>
        static public SpriteBatch _batch;

        /// <summary>
        /// The current game time (static)<br/>
        /// </summary>
        static public GameTime _gameTime;

        /// <summary>
        /// The 1 by 1 rectangle of the mouse currently<br/>
        /// </summary>
        static public Rectangle mouseRectangle;

        /// <summary>
        /// The UI component currently being hovered by the mouse<br/>
        /// </summary>
        static public UIComponent MouseOnElement;

        /// <summary>
        /// The maximum ID of all UI components<br/>
        /// </summary>
        static public int maxID = 0;

        //DONE: AnimatedUIComponent (DONE)
        //DONE: TextUIComponent (DONE)
        //DONE: Passable cliprect for Draw (scrolling in a window or something) (DONE)
        //TODO: Manage fathers for headers to call the dragging functions of their fathers


        /// <summary>
        /// Updates all UI components<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        static public void SUpdate(GameTime gameTime)
        {
            //Check if root is visible and if not, the UI is hidden
            if (!Root._visible)
            {
                //We open the UI if U is pressed
                if (InputManager.Keyboard.Pressed(Keys.U))
                {
                    Root._visible = true;
                }
            }
            else
            {
                //We close the UI if U is pressed
                if (InputManager.Keyboard.Pressed(Keys.U))
                {
                    Root._visible = false;
                    return;
                }

                //We update the mouse rectangle
                mouseRectangle = new Rectangle((int)(InputManager.Mouse.CurrentState.X / Game.I._canvasScale.X), (int)(InputManager.Mouse.CurrentState.Y / Game.I._canvasScale.Y), 1, 1);
                
                //We initialize the mouse on element variable
                //It will be calculated recursively in the Update function
                MouseOnElement = Root;

                //Set the game time
                _gameTime = gameTime;

                //We update the UI (at the origin)
                Root.Update(new Point(0, 0));
                
                //We check if the mouse is on the UI
                if (MouseOnElement == Root)
                {
                    //If we click outside of any window, we close the UI
                    if (InputManager.Mouse.CurrentState.LeftButton == ButtonState.Pressed && InputManager.Mouse.PreviousState.LeftButton != ButtonState.Pressed) Root._visible = false;
                }
                else
                {
                    //Otherwise, trigger the MouseOn() function of the element being hovered by the mouse
                    MouseOnElement.MouseOn();
                }

            }
        }

        /// <summary>
        /// Draws all UI components<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="batch">The batch to draw on</param>
        static public void SDraw(GameTime gameTime, SpriteBatch batch)
        {
            //Set the game time and the spritebatch
            _gameTime = gameTime;
            _batch = batch;

            //Begin rendering
            _batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);

            //Call the recursive draw function at the origin
            if (Root._visible) Root.Draw(new Point(0, 0));

            //End rendering
            _batch.End();
        }



        //---------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The ID of the UI component<br/>
        /// </summary>
        public int _id;

        /// <summary>
        /// The children of the UI component<br/>
        /// The last child is drawn on top always<br/>
        /// </summary>
        public List<UIComponent> Children = new List<UIComponent>();

        /// <summary>
        /// The father of the UI component<br/>
        /// </summary>
        public UIComponent Father = null;

        /// <summary>
        /// The relative position of the UI component to its father (disregarding margins)<br/>
        /// </summary>
        public Point _position = new Point(0, 0);

        /// <summary>
        /// The rectangle that is clickable, moved by _position and _margin. Includes border and padding.<br/>
        /// </summary>
        public Rectangle _rect = new Rectangle(0, 0, 0, 0);

        /// <summary> 
        ///Source rect for drawing. On null it is the entire _texture<br/>
        /// </summary>
        public Rectangle? _sourceRect = null;

        /// <summary>
        /// The texture of the UI component<br/>
        /// </summary>
        public Texture2D _texture = null;

        /// <summary>
        /// The color to render the UI component with<br/>
        /// </summary>
        public Color _color = Color.White;

        /// <summary> 
        ///If the object updates and draws<br/>
        /// </summary>
        public bool _visible = false;

        /// <summary>
        /// If the object can be dragged<br/>
        /// If not, it will call father's drag by default. Handled inside OnDrag<br/>
        /// </summary>
        public bool _draggable = false;
        /// <summary>
        /// If the object is being dragged currently<br/>
        /// </summary>
        public bool _dragged = false;


        //CSS block model        

        
        /// <summary>
        /// Content - The content of the box, where text and images appear<br/>
        /// Rect for content inside padding to be read by the children<br/>
        /// </summary>
        public Rectangle _contentRect;

        /// <summary> 
        /// Padding - Clears an area around the content.<br/>
        /// The padding is transparent.<br/>
        /// Left and Top moves passed BasePoint,<br/>
        /// Right and Bottom sizes _contentRect<br/>
        /// </summary>
        public BoxProp _padding;

        /// <summary>
        /// Border - A border that goes around the padding and content it is inside _rect.<br/>
        /// Left and Top moves passed BasePoint,<br/>
        /// Right and Bottom sizes _contentRect<br/>
        /// </summary>
        public BoxProp _border;

        /// <summary>
        /// The color of the border<br/>
        /// </summary>
        public Color _borderColor;

        /// <summary>
        /// The texture of the border<br/>
        /// </summary>
        public Texture2D _borderTexture = null;

        /// <summary>
        ///Margin - Clears an area outside the border.<br/>
        ///The margin is transparent<br/>
        ///Left and Top moves _rect and passed BasePoint<br/>
        ///(Right and Bottom does nothing right now)<br/>
        /// </summary>
        public BoxProp _margin;

        /// <summary>
        ///If it is rendered on top after clicking on it (It's father will still activate)<br/>
        /// </summary>
        public bool _activatable = true;

        /// <summary>
        /// If it cuts the rendering of its children with _cutRect<br/>
        /// (it cuts of any part of its children of _cutRect)<br/>
        /// </summary>
        public bool _cut = false;

        /// <summary>
        /// The box to constrain its children to if _cut is true<br/>
        /// </summary>
        public Rectangle _cutRect;

        /// <summary>
        /// If the mouse is hovering over the UI component<br/>
        /// </summary>
        private bool _hover;

        /// <summary>
        /// The actual cut rectangle for rendering<br/>
        /// </summary>
        static Rectangle _globalCutRect = new Rectangle(0, 0, Game.I._canvasWidth, Game.I._canvasHeight);


        /// <summary>
        /// Initializes the UI component<br/>
        /// Creates unique ID<br/>
        /// Initializes _borderTexture to a monochrome Pixel<br/>
        /// Don't forget to set the father later<br/>
        /// </summary>
        public UIComponent()
        {
            _id = maxID++;

            _borderTexture = new Texture2D(Game.I.GraphicsDevice, 1, 1);
            _borderTexture.SetData(new Color[] { Color.White });
        }

        /// <summary>
        /// Initializes the UI component<br/>
        /// Creates unique ID<br/>
        /// Initializes _borderTexture to a monochrome Pixel<br/>
        /// Sets the father<br/>
        /// </summary>
        /// <param name="father"></param>
        public UIComponent(UIComponent father)
        {
            _id = maxID++;

            Father = father;
            _borderTexture = new Texture2D(Game.I.GraphicsDevice, 1, 1);
            _borderTexture.SetData(new Color[] { Color.White });
        }


        /// <summary>
        /// Recursively updates the UI<br/>
        /// </summary>
        /// <param name="basePos">The position of the father plus the margin border and padding</param>
        public virtual void Update(Point basePos)
        {
            // If the mouse was hovering over the element but it stops, set _hover to false and call OnUnhover
            if (_hover && MouseOnElement != this) { _hover = false; OnUnhover(); }

            // If the element was dragged, but the mouse button is let up, set _dragged to false and call OnUnDragged
            if (_dragged && !InputManager.Mouse.CheckLeftButton) { _dragged = false; OnUnDragged(); }

            //Update the _rect position 
            _rect.X = (int)_position.X + _margin.Left;
            _rect.Y = (int)_position.Y + _margin.Top;

            //Calculate the actual rect by applying the father's position
            Rectangle _actRect = _rect;
            _actRect.Offset(basePos);

            //Check if the mouse is on the element
            if (mouseRectangle.Intersects(_actRect))
            {
                //Update the mouse on element variable
                //As we do this before the induction, the latest leaf of the tree intersecting the mouse will be final value
                MouseOnElement = this;
            }


            //Update the children
            foreach (UIComponent comp in Children)
            {
                //Set the father for each child
                comp.Father = this;
                //Update the content rect
                _contentRect = new Rectangle(basePos + _position + _margin.Position + _border.Position + _padding.Position, _rect.Size - _border.Size - _padding.Size);
                //If the component is visible update it
                //The base pos is modified by the margin border andpadding
                if (comp._visible) comp.Update(basePos + _position + _margin.Position + _border.Position + _padding.Position);
            }

            //If the element is dragged (handled by MouseOn), call OnDragged
            if (_dragged)
            {
                OnDragged();
            }




        }

        /// <summary>
        /// The function called when the mouse is on the element<br/>
        /// </summary>
        private void MouseOn()
        {
            //Check if the mouse button is pressed
            if (InputManager.Mouse.CurrentState.LeftButton == ButtonState.Pressed && InputManager.Mouse.PreviousState.LeftButton != ButtonState.Pressed)
            {
                //Call OnClick
                OnClick();

                //Set the dragged to true
                _dragged = true;

            }
            //If the mouse button is not pressed, call OnHover and set _hover to true
            else if (!InputManager.Mouse.CheckLeftButton) { OnHover(); _hover = true; }
            //If the mouse wheel is not 0, call OnScroll
            if (InputManager.Mouse.WheelDelta != 0) OnScroll(InputManager.Mouse.WheelDelta);
        }

        
        /// <summary>
        /// Draws the UI recursively<br/>
        /// </summary>
        /// <param name="basePos"> The position of the father plus the margin border and padding</param>
        public virtual void Draw(Point basePos)
        {
            //Calculate the actual rectangle by applying the base pos
            Rectangle _actRect = _rect;
            _actRect.Offset(basePos);

            //Draw bordertexture under the actual rect
            if (_borderTexture != null && _rect.Width > 0 && _rect.Height > 0) _batch.Draw(_borderTexture, _actRect, _borderColor);
            
            //TODO: Check if the margin is correctly applied for drawing

            // Calculate the actual rect by applying the border
            _actRect.Offset(_border.Position);
            _actRect.Size -= _border.Size;
            //Draw the actual rect
            if (_texture != null && _actRect.Width > 0 && _actRect.Height > 0) _batch.Draw(_texture, _actRect, _sourceRect, _color);


            Rectangle _previousCutRect = _globalCutRect;
            if (_cut)
            {
                //Calculate the current cut rect by intersecting the global cut rect with the cut rect of the UI component
                Rectangle _actCutRect;
                //If the cut rect is empty, use the content rect as the cut rect
                if (_cutRect == Rectangle.Empty)
                {
                    _actCutRect = _rect;
                    _actCutRect.Width -= _border.Right + _border.Left;
                    _actCutRect.Height -= _border.Top + _border.Bottom;
                    _actCutRect.Offset(_border.Position);
                }
                else
                {
                    _actCutRect = _cutRect;
                }
                _actCutRect.Offset(basePos);


                //Set the cut rect of the SpriteBatch

                _batch.End();
                RasterizerState _rats = new RasterizerState();
                _rats.MultiSampleAntiAlias = _batch.GraphicsDevice.RasterizerState.MultiSampleAntiAlias;
                _rats.DepthClipEnable = _batch.GraphicsDevice.RasterizerState.DepthClipEnable;
                _rats.DepthBias = _batch.GraphicsDevice.RasterizerState.DepthBias;
                _rats.SlopeScaleDepthBias = _batch.GraphicsDevice.RasterizerState.SlopeScaleDepthBias;
                _rats.CullMode = _batch.GraphicsDevice.RasterizerState.CullMode;
                _rats.FillMode = _batch.GraphicsDevice.RasterizerState.FillMode;
                _rats.ScissorTestEnable = true;
                _batch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(_actCutRect, _globalCutRect);
                _globalCutRect = Rectangle.Intersect(_actCutRect, _globalCutRect);
                _batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, _rats);
            }

            //Draw each visible children in the content rect
            foreach (UIComponent comp in Children)
            {
                _contentRect = new Rectangle(basePos + _position + _margin.Position + _border.Position + _padding.Position, _rect.Size - _border.Size - _padding.Size);
                if (comp._visible) comp.Draw(basePos + _position + _margin.Position + _border.Position + _padding.Position);
            }

            if (_cut)
            {
                //Reset the cut rect to the previous one
                _batch.End();
                RasterizerState _rats = new RasterizerState();
                _rats.MultiSampleAntiAlias = _batch.GraphicsDevice.RasterizerState.MultiSampleAntiAlias;
                _rats.DepthClipEnable = _batch.GraphicsDevice.RasterizerState.DepthClipEnable;
                _rats.DepthBias = _batch.GraphicsDevice.RasterizerState.DepthBias;
                _rats.SlopeScaleDepthBias = _batch.GraphicsDevice.RasterizerState.SlopeScaleDepthBias;
                _rats.CullMode = _batch.GraphicsDevice.RasterizerState.CullMode;
                _rats.FillMode = _batch.GraphicsDevice.RasterizerState.FillMode;
                _rats.ScissorTestEnable = true;
                _batch.GraphicsDevice.ScissorRectangle = _previousCutRect;
                _globalCutRect = _previousCutRect;
                _batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, _rats);
            }

        }

        /// <summary>
        /// Function called when a component is clicked to bring it to the top if it is _activatable<br/>
        /// </summary>
        public void Activate()
        {
            // Check if the component has a father
            if (Father == null) return;

            // Check if the component is _activatable
            if (_activatable)
            {
                //Put this last in the list of children, thus rendered on top
                Father.Children.Remove(this);
                Father.Children.Add(this);
            }

            // Activate the father as well
            Father.Activate();
        }

        /// <summary>
        /// This function is called when the component is clicked<br/>
        /// </summary>
        public virtual void OnClick()
        {
            // Handle click event here
            Activate();
        }

        /// <summary>
        /// This function is called when the component begins to be hovered<br/>
        /// </summary>
        public virtual void OnHover()
        {
            // Handle hover event here
        }

        /// <summary>
        /// This function is called when the component stops being hovered<br/>
        /// </summary>
        public virtual void OnUnhover()
        {
            // Handle hover event here
        }

        /// <summary>
        /// This function is called when the component starts being dragged<br/>
        /// If it is draggable, it will follow the mouse<br/>
        /// If it is not draggable, it will call its father's OnDragged<br/>
        /// </summary>
        public virtual void OnDragged()
        {
            if (_draggable)
            {
                _position += InputManager.Mouse.deltaPosition;
            }
            else
            {
                if (Father != null) Father.OnDragged();
            }

            // Handle dragged event here
        }

        /// <summary>
        /// This function is called when the component stops being dragged<br/>
        /// </summary>
        public virtual void OnUnDragged()
        {

            // Handle dragged event here
        }

        /// <summary>
        /// This function is called when the component is hovered and scrolled<br/>
        /// By default, it will call its father's OnScroll<br/>
        /// </summary>
        /// <param name="wheelDelta"> </param>
        public virtual void OnScroll(int wheelDelta)
        {
            // Handle scroll event here
            if(Father != null) Father.OnScroll(wheelDelta);
        }


    }
}
