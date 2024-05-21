using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

namespace TestShader
{
    /// <summary>
    /// An implementation for a slider UI element<br/>
    /// </summary>
    class XSlider : UIComponent
    {
        /// <summary>
        /// The x padding of the slider handle<br/>
        /// </summary>
        public int _paddingX
        {
            get => handle._paddingX;
            set => handle._paddingX = value;
        }
        /// <summary>
        /// The y padding of the slider handle<br/>
        /// </summary>
        public int _paddingY
        {
            get => handle._paddingY;
            set => handle._paddingY = value;
        }
        /// <summary>
        /// The width of the slider handle<br/>
        /// </summary>
        public int _handleWidth
        {
            get => handle._handleWidth;
            set => handle._handleWidth = value;
        }

        /// <summary>
        /// The value of the slider<br/>
        /// </summary>
        public int _value
        {
            get => handle._value;
            set => handle._value = value;
        }

        /// <summary>
        /// The handle of the slider (as a UI component)<br/>
        /// </summary>
        public XHandle handle;

        /// <summary>
        /// The UI component class for the handle<br/>
        /// </summary>
        public class XHandle : UIComponent
        {
            /// <summary>
            /// The x padding of the slider handle<br/>
            /// </summary>
            public int _paddingX;
            /// <summary>
            /// The y padding of the slider handle<br/>
            /// </summary>
            public int _paddingY;
            /// <summary>
            /// The width of the slider handle<br/>
            /// </summary>
            public int _handleWidth;
            /// <summary>
            /// The value of the slider<br/>
            /// </summary>
            public int _value;

            /// <summary>
            /// Initializes a new instance of the <see cref="XHandle"/> class.<br/>
            /// Sets tne padding of the handle and the width<br/>
            /// Sets the texture to a monochrome white pixel<br/>
            /// Sets it to be draggable and visible<br/>
            /// </summary>
            /// <param name="paddingX"></param>
            /// <param name="paddingY"></param>
            /// <param name="handleWidth"></param>
            public XHandle(int paddingX, int paddingY, int handleWidth) : base()
            {
                _paddingX = paddingX;
                _paddingY = paddingY;
                _handleWidth = handleWidth;

                _texture = new Texture2D(Game.I.GraphicsDevice, 1, 1);
                _texture.SetData(new Color[] { Color.White });
                _draggable = true;
                _visible = true;
                _position.X = _paddingX;
                _position.Y = _paddingY;
            }

            /// <summary>
            /// Updates the position of the handle<br/>
            /// Fixes the y position<br/>
            /// Calculates the value of the slider based on the x position<br/>
            /// </summary>
            /// <param name="basePos"></param>
            public override void Update(Point basePos)
            {
                _position.Y = _paddingY;
                _rect.Width = _handleWidth;
                _rect.Height = Father._contentRect.Height - 2 * _paddingY;
                _value = (int)(((float)(_position.X - _paddingX)) / ((float)(Father._rect.Width - 2 * _paddingX - _handleWidth)));//Scale _value between 0,1, to be tested
                base.Update(basePos);
            }

            /// <summary>
            /// The method called when the handle is dragged<br/>
            /// It will keep the Y coordinate fixed and clamps the handle between the ends of the slider (padding included)<br/>
            /// </summary>
            public override void OnDragged()
            {
                if (_draggable)
                {
                    _position.X += InputManager.Mouse.deltaPosition.X;
                    _position.X = Math.Clamp(_position.X, _paddingX-_handleWidth/2, Father._contentRect.Width - _paddingX - _handleWidth);
                    _position.Y = _paddingY;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XSlider"/> class.<br/>
        /// Sets the width, height of the slider<br/>
        /// Initializes the handle with give width and padding<br/>
        /// Initializes the texture to a monochrome white pixel<br/>
        /// Sets it to be not draggable but visible and to use cut rect<br/>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="paddingX"></param>
        /// <param name="paddingY"></param>
        /// <param name="handleWidth"></param>
        public XSlider(int width = 100, int height = 20, int paddingX = 5, int paddingY = 5, int handleWidth = 10) : base()
        {
            handle = new XHandle(paddingX, paddingY, handleWidth);
            Children.Add(handle);

            _rect.Width = width;
            _rect.Height = height;
            _texture = new Texture2D(Game.I.GraphicsDevice, 1, 1);
            _texture.SetData(new Color[] { Color.White * 0.75f });
            _cut = true;
            _draggable = false;
            _visible = true;
        }
    }
}
