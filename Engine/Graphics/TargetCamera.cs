using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{

    //Parts of the camera movement implementations are base off of Monocle engine from Maddy Thorson

    /// <summary>
    /// The type of camera movement to apply<br/>
    /// </summary>
    public enum CameraControlType
    {
        Target,
        Clamp,
        Linear,
        None
    }

    /// <summary>
    /// A basic camera implementation with a few movement options<br/>
    /// </summary>
    public abstract class TargetCamera : Camera
    {
        //TODO!: Implement fps correction

        /// <summary>
        /// The anchor for the camera<br/>
        /// The camera will target the linear combination of the target and the anchor<br/>
        /// </summary>
        public Vector2 _anchor;
        /// <summary>
        /// The linear combination for the anchoring of the camera<br/>
        /// The camera will target the linear combination of the target and the anchor<br/>
        /// </summary>
        public Vector2 _anchorLerp;
        /// <summary>
        /// Whether or not to ignore the x axis when anchoring<br/>
        /// </summary>
        public bool _anchorIgnoreX;
        /// <summary>
        /// Whether or not to ignore the y axis when anchoring<br/>
        /// </summary>
        public bool _anchorIgnoreY;

        /// <summary>
        /// The option to control the camera from <seealso cref="CameraControlType"/><br/>
        /// </summary>
        public CameraControlType _controlType = CameraControlType.Linear;

        /// <summary>
        /// The target component for the camera<br/>
        /// </summary>
        public Component focus = null;
        
        /// <summary>
        /// If the target is a player, the id of the player<br/>
        /// If the target is not a player, -1<br/>
        /// </summary>
        public int focusPlayerID = -1;

        /// <summary>
        /// A function delegate for a dynamically set target<br/>
        /// </summary>
        public Func<Vector2> getTarget;
        public abstract Vector2 GetTarget();

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetCamera"/> class.<br/>
        /// If no getTarget is provided, it will default to GetTarget for deciding the target<br/>
        /// </summary>
        public TargetCamera() : base()
        {
            if (getTarget == null) getTarget = GetTarget;
        }

        /// <summary>
        /// Updates the position of the camera<br/>
        /// </summary>
        public override void CameraUpdate(GameTime gameTime)
        {
            //TODO: Why is this here?
            if (_scene._players != null && !_scene._players.Contains(null))
            {

                // The current position of the camera (without screenshake)
                Vector2 from = _innerPosition.ToVector2();
                // The target of the camera
                Vector2 target = CameraTarget(GetTarget());

                //Depending on the camera control type calculate the next position
                switch (_controlType)
                {
                    case CameraControlType.Linear:
                        _innerPosition = GetNextPositionLinear(from, target);
                        break;
                    case CameraControlType.Clamp:
                        _innerPosition = GetNextPositionClamp(from, target);
                        break;
                    case CameraControlType.Target:
                        _innerPosition = target.ToPoint();
                        break;
                }
            }

            // Call the base update
            base.CameraUpdate(gameTime);
        }

        /// <summary>
        /// Gets the next position of the camera in the <seealso cref="CameraControlType.Clamp"/> camera control mode<br/>
        /// </summary>
        /// <param name="from">The current position of the camera (without screenshake)</param>
        /// <param name="target">The target position of the camera</param>
        /// <returns> The next position of the camera </returns>
        private static Point GetNextPositionClamp(Vector2 from, Vector2 target)
        {
            //TODO: Extract 20,35 into variables

            //Clamp the position to the target with a maximum distance of 20, 35 in directions X, Y respectively
            Vector2 newPosition = from;
            newPosition.X = Math.Clamp(newPosition.X, target.X - 20, target.X + 20);
            newPosition.Y = Math.Clamp(newPosition.Y, target.Y - 35, target.Y + 35);

            return newPosition.ToPoint();
        }

        private static Point GetNextPositionLinear(Vector2 from, Vector2 target)
        {
            //Get the next position of the camera by a linear combination of the current position and the target based on the time.
            float multiplier = 1f;
            Vector2 newPosition = from + (target - from) * (1f - (float)Math.Pow(0.01f / multiplier, Game.I._deltaTime));
            return newPosition.ToPoint();
        }


        /// <summary>
        /// Gets the average position of a list of players<br/>
        /// </summary>
        public static Vector2 PlayerAverage(List<Player> players)
        {
            Point playerSum = new Point();
            foreach (Player player in players) playerSum += player.Position;
            Point playerAverage = (playerSum.ToVector2() / players.Count).ToPoint();
            return playerAverage.ToVector2();
        }

        /// <summary>
        /// Gets the target point of the camera<br/>
        /// </summary>
        /// <param name="worldTarget">The target of the camera in world coordinates</param>
        /// <returns> The target point of the camera in screen coordinates after applyig anchoring</returns>
        private Vector2 CameraTarget(Vector2 worldTarget)
        {
            Vector2 at;
            Vector2 target = new Vector2(worldTarget.X - destinationRectangle.Width / _scale.X / 2, worldTarget.Y - destinationRectangle.Height / _scale.X / 2);

            if (_anchorLerp.Length() > 0)
            {
                if (_anchorIgnoreX && !_anchorIgnoreY)
                    target.Y = MathHelper.Lerp(target.Y, _anchor.Y, _anchorLerp.Y);
                else if (!_anchorIgnoreX && _anchorIgnoreY)
                    target.X = MathHelper.Lerp(target.X, _anchor.X, _anchorLerp.X);
                else if (_anchorLerp.X == _anchorLerp.Y)
                    target = Vector2.Lerp(target, _anchor, _anchorLerp.X);
                else
                {
                    target.X = MathHelper.Lerp(target.X, _anchor.X, _anchorLerp.X);
                    target.Y = MathHelper.Lerp(target.Y, _anchor.Y, _anchorLerp.Y);
                }
            }

            at.X = target.X;
            at.Y = target.Y;
            return at;

            //TODO: Room bounds
            //at.X = MathHelper.Clamp(target.X, level.Bounds.Left, level.Bounds.Right - Celeste.GameWidth);
            //at.Y = MathHelper.Clamp(target.Y, level.Bounds.Top, level.Bounds.Bottom - Celeste.GameHeight);

            /*if (CameraLockMode != CameraLockModes.None)
            {
                var locker = Scene.Tracker.GetComponent<CameraLocker>();

                //X Snapping
                if (level.CameraLockMode != Level.CameraLockModes.BoostSequence)
                {
                    at.X = Math.Max(at.X, level.Camera.X);
                    if (locker != null)
                        at.X = Math.Min(at.X, Math.Max(level.Bounds.Left, locker.Entity.X - locker.MaxXOffset));
                }

                //Y Snapping
                if (level.CameraLockMode == Level.CameraLockModes.FinalBoss)
                {
                    at.Y = Math.Max(at.Y, level.Camera.Y);
                    if (locker != null)
                        at.Y = Math.Min(at.Y, Math.Max(level.Bounds.Top, locker.Entity.Y - locker.MaxYOffset));
                }
                else if (level.CameraLockMode == Level.CameraLockModes.BoostSequence)
                {
                    level.CameraUpwardMaxY = Math.Min(level.Camera.Y + CameraLocker.UpwardMaxYOffset, level.CameraUpwardMaxY);
                    at.Y = Math.Min(at.Y, level.CameraUpwardMaxY);
                    if (locker != null)
                        at.Y = Math.Max(at.Y, Math.Min(level.Bounds.Bottom - 180, locker.Entity.Y - locker.MaxYOffset));
                }
            }*/


            //TODO non camera ready boxes (killbox for example)
            // snap above killboxes
            /*var killboxes = Scene.Tracker.GetEntities<Killbox>();
            foreach (var box in killboxes)
            {
                if (!box.Collidable)
                    continue;

                if (Top < box.Bottom && Right > box.Left && Left < box.Right)
                    at.Y = Math.Min(at.Y, box.Top - 180);
            }*/
        }
    }
}
