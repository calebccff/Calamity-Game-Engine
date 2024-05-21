using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestShader
{

    /// <summary>
    /// Component that can be collided with<br/>
    /// </summary>
    public class Collider : Component //NO UPDATE
    {

        /// <summary>
        /// Hitboxes for the collider from the origin<br/>
        /// </summary>
        private List<Rectangle> _startHitboxes = new List<Rectangle>();

        /// <summary>
        /// Hitboxes for the collider from _position<br/>
        /// </summary>
        public List<Rectangle> _hitboxes = new List<Rectangle>();

        /// <summary>
        /// Tags for the collider<br/>
        /// Used to determine if a collision occurs<br/>
        /// </summary>
        public List<string> _tags = new List<string>();

        /// <summary>
        /// Object corresponding to the collider<br/>
        /// Used to handle outcomes of a collision<br/>
        /// </summary>
        public object _father;

        /// <summary>
        /// Whether the collider is active<br/>
        /// Only active colliders can collide<br/>
        /// </summary>
        public bool _active = true;

        /// <summary>
        /// Constructor<br/>
        /// </summary>
        /// <param name="comp"> Object that this collider is attached to</param>
        public Collider(object comp) : base()
        {
            _father = comp;

            Game.I._colliders.Add(this);
        }

        /// <summary>
        /// Adds a hitbox<br/>
        /// </summary>
        /// <param name="hitbox"> Hitbox to add</param>
        public void AddHitbox(Rectangle hitbox)
        {
            _hitboxes.Add(hitbox);

        }

        /// <summary>
        /// Removes a hitbox<br/>
        /// </summary>
        /// <param name="place"> Index of the hitbox</param>
        public void RemoveAtHitbox(int place)
        {
            _hitboxes.RemoveAt(place);

        }

        /// <summary>
        /// Gets the position<br/>
        /// </summary>
        /// <returns> Position</returns>
        public Point getPosition()
        {
            return _position;
        }

        /// <summary>
        /// Sets the position<br/>
        /// </summary>
        /// <param name="position"> Position</param>
        public void setPosition(Point position)
        {
            _position = position;
        }

        /// <summary>
        /// Dispose<br/>
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }


        /// <summary>
        /// UnsafeDispose,<br/>
        /// removes the collider from the list of colliders<br/>
        /// </summary>
        public override void UnsafeDispose()
        {
            Game.I._colliders.Remove(this);
            base.UnsafeDispose();
        }

        public int X
        {
            get
            {
                return _position.X;
            }
        }
        public int Y
        {
            get
            {
                return _position.Y;
            }
        }


        /*
        // Function to update the hitboxes
        // Might reinclude later
         public void UpdateHitboxes()
         {
             _hitboxes.Clear();
             _hitboxes.Capacity = _startHitboxes.Capacity;
             foreach (Rectangle rect in _startHitboxes) {

                 Rectangle rect2 = rect;
                 rect2.Location += new Point((int)_position.X, (int)_position.Y);
                 _hitboxes.Add(rect2);
             }
         }
        */
    }
}
