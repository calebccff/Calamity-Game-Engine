using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestShader
{
    /// <summary>
    /// A UI Component that draws a text<br/>
    /// </summary>
    class TextUIComponent : UIComponent
    {
        /// <summary>
        /// The text to be drawn<br/>
        /// </summary>
        public string _text;

        /// <summary>
        /// The font of the text<br/>
        /// </summary>
        public SpriteFont _font;

        /// <summary>
        /// The length of the textbox<br/>
        /// </summary>
        public float length = 0;

        //TODO: Proper Textbox (We could add tooltips+popups to certain words via using individual TextUI components)

        //TODO: Add popups. The popups stay open while hovering on either the text or the popup.

        /// <summary>
        /// Initializes a new instance of the <see cref="TextUIComponent"/> class.<br/>
        /// </summary>
        public TextUIComponent() : base()
        {
        }

        /// <summary>
        /// Draws the text to the screen<br/>
        /// Does not call base, thus certain functionality is lost<br/>
        /// </summary>
        /// <param name="basePos"> The position of the father plus the margin border and padding</param>
        public override void Draw(Point basePos)
        {
            //Calculate the actual rectangle by applying the base pos
            Rectangle _actRect = _rect;
            _actRect.Offset(basePos);

            // If font is not set, use default game font
            if (_font == null) _font = Game.I.font;

            // Wrap the text into lines of given length
            string currText;
            if (length > 0)
            {
                currText = Tools.WrapText(_font, _text, length);

            }
            else currText = _text;

            // Draw the text
            if (_text.Length > 0) _batch.DrawString(_font, currText, (_position + basePos).ToVector2(), _color);
            
            //Update the children
            foreach (UIComponent comp in Children)
            {
                if (comp._visible) comp.Draw((basePos + _position));
            }
        }





    }
}
