using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{
    public class Camera : Component
    {

        //-------------Camera Variables-------------
        /// <summary>
        /// The scaleing factor of the camera<br/>
        /// </summary>
        public Vector2 _scale { get; set; } = new Vector2(1, 1);


        /// <summary>
        /// The relative position applied by the screenshake to the camera<br/>
        /// The _position already includes the screenshake<br/>
        /// </summary>
        public Vector2 _shakePosition;

        /// <summary>
        /// The position of the camera without screenshake<br/>
        /// </summary>
        public Point _innerPosition;

        /// <summary>
        /// If the camera is currently shaking<br/>
        /// </summary>
        public bool shake = false;

        /// <summary>
        /// The remaining time for the camera shaking<br/>
        /// </summary>
        public float shake_time = 0;

        /// <summary>
        /// The current magnitude of the camera shake<br/>
        /// </summary>
        public float shake_magnitude = 0;

        /// <summary>
        /// The rate for the magnitude to decrease during the screenshake<br/>
        /// </summary>
        public float shake_fade = 0.25f;

        /// <summary>
        /// If the camera is currently active<br/>
        /// Inactive cameras will not render<br/>
        /// </summary>
        public bool active = true;

        /// <summary>
        /// If the camera should draw the outline of what it sees. Used for debugging<br/>
        /// </summary>
        public bool drawCameraBox = false;

        /// <summary>
        /// The rectangle where the camera should render on the screen (before applying the final canvas scaling)<br/>
        /// </summary>
        public Rectangle destinationRectangle = new Rectangle(0, 0, 450, 260);//Before Canvas scaling


        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.<br/>
        /// Adds itself to the list of cameras in the scene<br/>
        /// </summary>
        public Camera() : base()
        {
            Game.I.Cameras.Add(this);
        }

        /// <summary>
        /// Updates the position of the camera based on the screenshake<br/>
        /// Draws the outline of the camera view if drawCameraBox is true<br/>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void CameraUpdate(GameTime gameTime)
        {

            if (shake)
            {
                shake_time -= Game.I._deltaTime;
                //TODO: Check this, it seems erronous
                if (shake_time <= 0)
                {

                    shake_magnitude -= shake_fade * Game.I._deltaTime;

                    if (shake_magnitude <= 0)
                    {
                        _shakePosition = new Vector2();
                        shake = false;
                    }
                }
                else
                {
                    var _xval = Game.I.rand.Choose(-shake_magnitude, shake_magnitude) * Game.I._deltaTime;
                    var _yval = Game.I.rand.Choose(-shake_magnitude, shake_magnitude) * Game.I._deltaTime;
                    _shakePosition += (new Vector2((int)_xval, (int)_yval));

                }

            }
            _position = _shakePosition.ToPoint() + _innerPosition;

            if (drawCameraBox || Game.I.drawHitboxes) Game.I._spriteRenderer.HollowRect(_position.ToVector2(), destinationRectangle.Width / _scale.X, destinationRectangle.Height / _scale.Y, Color.Green);

        }

        /// <summary>
        /// Applies a screenshake effect to the camera for the specified time and magnitude<br/>
        /// </summary>
        public void Screenshake(float _time, float _magnitude)
        {
            shake = true;
            shake_time = _time;
            shake_magnitude = _magnitude;
        }

        /// <summary>
        /// UnsafeDispose<br/>
        /// Removes the camera from the list of cameras<br/>
        /// </summary>
        public override void UnsafeDispose()
        {
            Game.I.Cameras.Remove(this);
            base.UnsafeDispose();
        }
    }
}
