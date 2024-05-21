using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace TestShader
{

    /// <summary>
    /// A demo class for implementing a UI window<br/>
    /// Has multiple UI components defined inside it<br/>
    /// </summary>
    class DemoUIWindow : UIComponent
    {

        /// <summary>
        /// A demo class for implementing an animated UI window<br/>
        /// </summary>
        class DemoAnimatedUIWindow : AnimatedUIComponent
        {   

            /// <summary>
            /// Create a new DemoAnimatedUIWindow<br/>
            /// </summary>
            public DemoAnimatedUIWindow() : base()
            {

                // Set the texture of the window
                _texture = TextureManager.GetTexture("MainSheet");

                // Set the window such that its contents are only rendered inside it
                _cut = true;

                //Set the size of the window
                _rect.Width = 50;
                _rect.Height = 50;

                //TODO: Eliminate the need for animations for tiles with only 1 frame

                //Add the only animation
                _animations.Add("Default", new Animation(Tools.SplitTileSheet(_texture, 32, 32).GetRange(0, 3)) { _animationStepSpeed = 8 });
                
                //Set the animation to play
                ChangeAnimationState("Default");

                //Set the window to not be draggable, be visible, and not be activatable
                _draggable = false;
                _visible = true;
                _activatable = false;
            }
            
            /// <summary>
            /// A function that is called when the window is hovered<br/>
            /// Changes the color of the window<br/>
            /// </summary>
            public override void OnHover()
            {
                _color = Color.White * 0.5f;
                base.OnHover();
            }
            /// <summary>
            /// A function that is called when the window is unhovered<br/>
            /// Changes the color of the window back<br/>
            /// </summary>
            public override void OnUnhover()
            {
                _color = Color.White;
                base.OnUnhover();
            }

            /// <summary>
            /// A function that is called when the window is clicked<br/>
            /// Adds a PocketBox to the scene, which follows the mouse on click<br/>
            /// Removes the window
            /// </summary>
            public override void OnClick()
            {
                (Game.I._activeScene as DemoScene).pointer=new PocketBox(TextureManager.GetTexture("MainSheet"), 32, 32/*, Game.I.Content.Load<Effect>("Glow")*/) { Position = new Point(0, 0) };
                base.OnClick();
                Father.Children.Remove(this);
            }
        }


        /// <summary>
        /// Another demo class for implementing a UI window<br/>
        /// </summary>
        class DemoUIWindow2 : UIComponent
        {
            /// <summary>
            /// Create a new DemoUIWindow<br/>
            /// </summary>
            public DemoUIWindow2() : base()
            {

                //Set the size of the window
                _rect.Width = 130;
                _rect.Height = 50;

                //Set the window such that its contents are only rendered inside it
                _cut = true;

                //Creates a monochrome pixel as the texture for the window
                _texture = new Texture2D(Game.I.GraphicsDevice, 1, 1);
                _texture.SetData(new Color[] { Color.Red * 0.75f });

                //Set the window to be draggable, be visible, and have a black border with 5px thickness
                _draggable = true;
                _visible = true;
                _borderColor = Color.Black;
                _border.Top = 5;
                _border.Left = 5;
                _border.Right = 5;
                _border.Bottom = 5;

                //Add a text UI component
                Children.Add(new TextUIComponent());
                //Set it to be visible
                Children[0]._visible = true;
                //Set the text it displays to "Show Hitboxes"
                (Children[0] as TextUIComponent)._text = "Show Hitboxes";
            }
            /// <summary>
            /// A function that is called when the window is clicked<br/>
            /// Triggers the drawing of all hitboxes (this would be used for debugging)<br/>
            /// </summary>
            public override void OnClick()
            {
                Game.I.drawHitboxes = !Game.I.drawHitboxes;
                base.OnClick();
            }

            /// <summary>
            /// A function that is called when the window is hovered<br/>
            /// Changes the color of the window<br/>
            /// </summary>
            public override void OnHover()
            {
                _color = Color.White * 0.5f;
                base.OnHover();
            }

            /// <summary>
            /// A function that is called when the window is unhovered<br/>
            /// Changes the color of the window back<br/>
            /// </summary>
            public override void OnUnhover()
            {
                _color = Color.White;
                base.OnUnhover();
            }
        }


        /// <summary>
        /// Create a new DemoUIWindow<br/>
        /// </summary>
        public DemoUIWindow() : base()
        {
            //Set the size of the window
            _rect.Width = 200;
            _rect.Height = 200;

            //Creates a monochrome pixel as the texture for the window
            _texture = new Texture2D(Game.I.GraphicsDevice, 1, 1);
            _texture.SetData(new Color[] { Color.White * 0.75f });

            //Set this window as the child of the root
            Root.Children.Add(this);

            //Set the window such that its contents are only rendered inside it
            _cut = true;

            //Set the window to be draggable, be visible
            _draggable = true;
            _visible = true;

            //Add the UI components as children
            Children.Add(new DemoAnimatedUIWindow());
            Children.Add(new DemoUIWindow2(){_position = new Point(0, 100)});

            //Add a default xslider as well to the window
            Children.Add(new XSlider(100,30)
            {
                _borderColor = Color.Black,
                _border= new BoxProp(5), 
                _position = new Point(100, 0)
            });

        
            //Set the window to have a black border with 5px thickness
            _borderColor = Color.Black;
            _border.Top = 5;
            _border.Left = 5;
            _border.Right = 5;
            _border.Bottom = 5;
        }

        /// <summary>
        /// A function that is called when the window is clicked<br/>
        /// </summary>
        public override void OnClick()
        {
            base.OnClick();
        }

        /// <summary>
        /// A function that is called when the window is hovered<br/>
        /// </summary>
        public override void OnHover()
        {
            base.OnHover();
        }

        /// <summary>
        /// A function that is called when the window is unhovered<br/>
        /// </summary>
        public override void OnUnhover()
        {
            base.OnUnhover();
        }
    }





}
